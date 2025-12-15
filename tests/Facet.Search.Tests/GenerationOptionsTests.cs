using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Facet.Search.Generators;

namespace Facet.Search.Tests;

/// <summary>
/// Tests for generation options (aggregations, metadata).
/// </summary>
public class GenerationOptionsTests
{
    [Fact]
    public void Generator_WithAggregationsDisabled_DoesNotGenerateAggregations()
    {
        var source = @"
using Facet.Search;

namespace TestNamespace;

[FacetedSearch(GenerateAggregations = false)]
public class Product
{
    [SearchFacet(Type = FacetType.Categorical)]
    public string Brand { get; set; } = null!;
}
";
        var (compilation, diagnostics) = RunGenerator(source);

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

        var aggregationTree = compilation.SyntaxTrees
            .FirstOrDefault(t => t.FilePath.Contains("FacetAggregations"));

        // Should not generate aggregation file
        Assert.Null(aggregationTree);
    }

    [Fact]
    public void Generator_WithMetadataDisabled_DoesNotGenerateMetadata()
    {
        var source = @"
using Facet.Search;

namespace TestNamespace;

[FacetedSearch(GenerateMetadata = false)]
public class Product
{
    [SearchFacet(Type = FacetType.Categorical)]
    public string Brand { get; set; } = null!;
}
";
        var (compilation, diagnostics) = RunGenerator(source);

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

        var metadataTree = compilation.SyntaxTrees
            .FirstOrDefault(t => t.FilePath.Contains("SearchMetadata"));

        // Should not generate metadata file
        Assert.Null(metadataTree);
    }

    [Fact]
    public void Generator_WithBothDisabled_OnlyGeneratesFilterAndExtensions()
    {
        var source = @"
using Facet.Search;

namespace TestNamespace;

[FacetedSearch(GenerateAggregations = false, GenerateMetadata = false)]
public class Product
{
    [SearchFacet(Type = FacetType.Categorical)]
    public string Brand { get; set; } = null!;
}
";
        var (compilation, diagnostics) = RunGenerator(source);

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

        var generatedTrees = compilation.SyntaxTrees
            .Where(t => t.FilePath.Contains(".g.cs"))
            .ToList();

        // Should only generate filter and extensions
        Assert.Equal(2, generatedTrees.Count);
        Assert.True(generatedTrees.Any(t => t.FilePath.Contains("SearchFilter")));
        Assert.True(generatedTrees.Any(t => t.FilePath.Contains("SearchExtensions")));
    }

    [Fact]
    public void Generator_WithDefaultOptions_GeneratesAllFiles()
    {
        var source = @"
using Facet.Search;

namespace TestNamespace;

[FacetedSearch]
public class Product
{
    [SearchFacet(Type = FacetType.Categorical)]
    public string Brand { get; set; } = null!;
}
";
        var (compilation, diagnostics) = RunGenerator(source);

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

        var generatedTrees = compilation.SyntaxTrees
            .Where(t => t.FilePath.Contains(".g.cs"))
            .ToList();

        // Should generate all 4 files
        Assert.Equal(4, generatedTrees.Count);
        Assert.True(generatedTrees.Any(t => t.FilePath.Contains("SearchFilter")));
        Assert.True(generatedTrees.Any(t => t.FilePath.Contains("SearchExtensions")));
        Assert.True(generatedTrees.Any(t => t.FilePath.Contains("FacetAggregations")));
        Assert.True(generatedTrees.Any(t => t.FilePath.Contains("SearchMetadata")));
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
