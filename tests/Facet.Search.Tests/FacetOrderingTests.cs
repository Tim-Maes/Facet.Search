using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Facet.Search.Generators;

namespace Facet.Search.Tests;

/// <summary>
/// Tests for facet ordering and limit options.
/// </summary>
public class FacetOrderingTests
{
    [Fact]
    public void Generator_WithOrderByCount_IncludesOrderByInMetadata()
    {
        var source = @"
using Facet.Search;

namespace TestNamespace;

[FacetedSearch]
public class Product
{
    [SearchFacet(Type = FacetType.Categorical, OrderBy = FacetOrder.Count)]
    public string Brand { get; set; } = null!;
}
";
        var (compilation, diagnostics) = RunGenerator(source);

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

        var metadataTree = compilation.SyntaxTrees
            .FirstOrDefault(t => t.FilePath.Contains("SearchMetadata"));

        Assert.NotNull(metadataTree);
        var code = metadataTree!.ToString();

        Assert.Contains("Count", code);
    }

    [Fact]
    public void Generator_WithLimit_IncludesLimitInMetadata()
    {
        var source = @"
using Facet.Search;

namespace TestNamespace;

[FacetedSearch]
public class Product
{
    [SearchFacet(Type = FacetType.Categorical, Limit = 10)]
    public string Brand { get; set; } = null!;
}
";
        var (compilation, diagnostics) = RunGenerator(source);

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

        var metadataTree = compilation.SyntaxTrees
            .FirstOrDefault(t => t.FilePath.Contains("SearchMetadata"));

        Assert.NotNull(metadataTree);
        var code = metadataTree!.ToString();

        Assert.Contains("10", code);
    }

    [Fact]
    public void Generator_WithRangeIntervals_IncludesIntervalsInMetadata()
    {
        var source = @"
using Facet.Search;

namespace TestNamespace;

[FacetedSearch]
public class Product
{
    [SearchFacet(Type = FacetType.Range, RangeIntervals = ""0-50,50-100,100-500,500+"")]
    public decimal Price { get; set; }
}
";
        var (compilation, diagnostics) = RunGenerator(source);

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

        var metadataTree = compilation.SyntaxTrees
            .FirstOrDefault(t => t.FilePath.Contains("SearchMetadata"));

        Assert.NotNull(metadataTree);
        var code = metadataTree!.ToString();

        Assert.Contains("0-50", code);
    }

    [Fact]
    public void Generator_WithDisplayName_IncludesDisplayNameInMetadata()
    {
        var source = @"
using Facet.Search;

namespace TestNamespace;

[FacetedSearch]
public class Product
{
    [SearchFacet(Type = FacetType.Categorical, DisplayName = ""Product Brand"")]
    public string Brand { get; set; } = null!;
}
";
        var (compilation, diagnostics) = RunGenerator(source);

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

        var metadataTree = compilation.SyntaxTrees
            .FirstOrDefault(t => t.FilePath.Contains("SearchMetadata"));

        Assert.NotNull(metadataTree);
        var code = metadataTree!.ToString();

        Assert.Contains("Product Brand", code);
    }

    [Fact]
    public void Generator_WithDefaultOrderBy_UsesCount()
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

        var metadataTree = compilation.SyntaxTrees
            .FirstOrDefault(t => t.FilePath.Contains("SearchMetadata"));

        Assert.NotNull(metadataTree);
        var code = metadataTree!.ToString();

        // Default should be Count
        Assert.Contains("Count", code);
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
