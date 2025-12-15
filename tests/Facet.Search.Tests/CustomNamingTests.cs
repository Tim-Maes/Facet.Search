using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Facet.Search.Generators;

namespace Facet.Search.Tests;

/// <summary>
/// Tests for custom filter class names and namespaces.
/// </summary>
public class CustomNamingTests
{
    [Fact]
    public void Generator_WithCustomFilterClassName_GeneratesCorrectClassName()
    {
        var source = @"
using Facet.Search;

namespace TestNamespace;

[FacetedSearch(FilterClassName = ""MyCustomFilter"")]
public class Product
{
    [SearchFacet(Type = FacetType.Categorical)]
    public string Brand { get; set; } = null!;
}
";
        var (compilation, diagnostics) = RunGenerator(source);

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

        var filterTree = compilation.SyntaxTrees
            .FirstOrDefault(t => t.FilePath.Contains("MyCustomFilter"));

        Assert.NotNull(filterTree);
        var code = filterTree!.ToString();

        Assert.Contains("class MyCustomFilter", code);
    }

    [Fact]
    public void Generator_WithCustomNamespace_GeneratesInCorrectNamespace()
    {
        var source = @"
using Facet.Search;

namespace TestNamespace;

[FacetedSearch(Namespace = ""Custom.Search.Namespace"")]
public class Product
{
    [SearchFacet(Type = FacetType.Categorical)]
    public string Brand { get; set; } = null!;
}
";
        var (compilation, diagnostics) = RunGenerator(source);

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

        var filterTree = compilation.SyntaxTrees
            .FirstOrDefault(t => t.FilePath.Contains("ProductSearchFilter"));

        Assert.NotNull(filterTree);
        var code = filterTree!.ToString();

        Assert.Contains("namespace Custom.Search.Namespace", code);
    }

    [Fact]
    public void Generator_WithBothCustomizations_AppliesBoth()
    {
        var source = @"
using Facet.Search;

namespace TestNamespace;

[FacetedSearch(FilterClassName = ""SpecialFilter"", Namespace = ""My.Custom.NS"")]
public class Product
{
    [SearchFacet(Type = FacetType.Categorical)]
    public string Brand { get; set; } = null!;
}
";
        var (compilation, diagnostics) = RunGenerator(source);

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

        var filterTree = compilation.SyntaxTrees
            .FirstOrDefault(t => t.FilePath.Contains("SpecialFilter"));

        Assert.NotNull(filterTree);
        var code = filterTree!.ToString();

        Assert.Contains("namespace My.Custom.NS", code);
        Assert.Contains("class SpecialFilter", code);
    }

    [Fact]
    public void Generator_DefaultNaming_UsesModelNameWithSuffix()
    {
        var source = @"
using Facet.Search;

namespace TestNamespace;

[FacetedSearch]
public class Order
{
    [SearchFacet(Type = FacetType.Range)]
    public decimal Total { get; set; }
}
";
        var (compilation, diagnostics) = RunGenerator(source);

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

        var filterTree = compilation.SyntaxTrees
            .FirstOrDefault(t => t.FilePath.Contains("OrderSearchFilter"));

        Assert.NotNull(filterTree);
        var code = filterTree!.ToString();

        Assert.Contains("namespace TestNamespace.Search", code);
        Assert.Contains("class OrderSearchFilter", code);
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
