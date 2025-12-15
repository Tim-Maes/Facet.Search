using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Facet.Search.Generators;

namespace Facet.Search.Tests;

/// <summary>
/// Tests for hierarchical facets and facet dependencies.
/// </summary>
public class HierarchicalFacetTests
{
    [Fact]
    public void Generator_WithHierarchicalFacet_GeneratesStringArrayProperty()
    {
        var source = @"
using Facet.Search;

namespace TestNamespace;

[FacetedSearch]
public class Category
{
    [SearchFacet(Type = FacetType.Hierarchical, IsHierarchical = true)]
    public string Path { get; set; } = null!;
}
";
        var (compilation, diagnostics) = RunGenerator(source);

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

        var filterTree = compilation.SyntaxTrees
            .FirstOrDefault(t => t.FilePath.Contains("CategorySearchFilter"));

        Assert.NotNull(filterTree);
        var code = filterTree!.ToString();

        Assert.Contains("string[]?", code);
        Assert.Contains("Path", code);
    }

    [Fact]
    public void Generator_WithDependsOn_IncludesDependencyInfo()
    {
        var source = @"
using Facet.Search;

namespace TestNamespace;

[FacetedSearch]
public class Product
{
    [SearchFacet(Type = FacetType.Categorical)]
    public string Category { get; set; } = null!;

    [SearchFacet(Type = FacetType.Categorical, DependsOn = ""Category"")]
    public string SubCategory { get; set; } = null!;
}
";
        var (compilation, diagnostics) = RunGenerator(source);

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

        var metadataTree = compilation.SyntaxTrees
            .FirstOrDefault(t => t.FilePath.Contains("SearchMetadata"));

        Assert.NotNull(metadataTree);
        var code = metadataTree!.ToString();

        // Metadata should include dependency information
        Assert.Contains("Category", code);
        Assert.Contains("SubCategory", code);
    }

    [Fact]
    public void Generator_WithHierarchicalFacet_GeneratesContainsFilter()
    {
        var source = @"
using Facet.Search;

namespace TestNamespace;

[FacetedSearch]
public class Category
{
    [SearchFacet(Type = FacetType.Hierarchical)]
    public string Path { get; set; } = null!;
}
";
        var (compilation, diagnostics) = RunGenerator(source);

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

        var extensionTree = compilation.SyntaxTrees
            .FirstOrDefault(t => t.FilePath.Contains("SearchExtensions"));

        Assert.NotNull(extensionTree);
        var code = extensionTree!.ToString();

        // Should generate Contains filter like categorical
        Assert.Contains("filter.Path", code);
        Assert.Contains("Contains", code);
    }

    private static (Compilation, ImmutableArray<Diagnostic>) RunGenerator(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .Cast<MetadataReference>()
            .ToList();

        references.Add(MetadataReference.CreateFromFile(typeof(FacetedSearchAttribute).Assembly.Location));

        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        CSharpGeneratorDriver.Create(new FacetSearchGenerator())
            .RunGeneratorsAndUpdateCompilation(
                compilation,
                out var outputCompilation,
                out var diagnostics);

        return (outputCompilation, diagnostics);
    }
}
