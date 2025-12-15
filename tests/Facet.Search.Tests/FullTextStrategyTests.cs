using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Facet.Search.Generators;

namespace Facet.Search.Tests;

/// <summary>
/// Tests for full-text search strategies.
/// </summary>
public class FullTextStrategyTests
{
    [Fact]
    public void Generator_WithLinqContainsStrategy_GeneratesContainsCode()
    {
        var source = @"
using Facet.Search;

namespace TestNamespace;

[FacetedSearch(FullTextStrategy = FullTextSearchStrategy.LinqContains)]
public class Product
{
    [FullTextSearch]
    public string Name { get; set; } = null!;
}
";
        var (compilation, diagnostics) = RunGenerator(source);

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

        var extensionTree = compilation.SyntaxTrees
            .FirstOrDefault(t => t.FilePath.Contains("SearchExtensions"));

        Assert.NotNull(extensionTree);
        var code = extensionTree!.ToString();

        Assert.Contains("Contains", code);
        Assert.Contains("LinqContains", code); // Strategy should be mentioned in comments
    }

    [Fact]
    public void Generator_WithClientSideStrategy_GeneratesAsEnumerable()
    {
        var source = @"
using Facet.Search;

namespace TestNamespace;

[FacetedSearch(FullTextStrategy = FullTextSearchStrategy.ClientSide)]
public class Product
{
    [FullTextSearch]
    public string Name { get; set; } = null!;
}
";
        var (compilation, diagnostics) = RunGenerator(source);

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

        var extensionTree = compilation.SyntaxTrees
            .FirstOrDefault(t => t.FilePath.Contains("SearchExtensions"));

        Assert.NotNull(extensionTree);
        var code = extensionTree!.ToString();

        // Client-side should use AsEnumerable
        Assert.Contains("AsEnumerable", code);
        Assert.Contains("CLIENT-SIDE", code);
    }

    [Fact]
    public void Generator_WithSqlServerFreeTextStrategy_GeneratesAppropriateCode()
    {
        var source = @"
using Facet.Search;

namespace TestNamespace;

[FacetedSearch(FullTextStrategy = FullTextSearchStrategy.SqlServerFreeText)]
public class Product
{
    [FullTextSearch]
    public string Name { get; set; } = null!;
}
";
        var (compilation, diagnostics) = RunGenerator(source);

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

        var extensionTree = compilation.SyntaxTrees
            .FirstOrDefault(t => t.FilePath.Contains("SearchExtensions"));

        Assert.NotNull(extensionTree);
        var code = extensionTree!.ToString();

        // Should mention FREETEXT in comments
        Assert.Contains("FREETEXT", code);
    }

    [Fact]
    public void Generator_WithPostgreSqlStrategy_GeneratesAppropriateCode()
    {
        var source = @"
using Facet.Search;

namespace TestNamespace;

[FacetedSearch(FullTextStrategy = FullTextSearchStrategy.PostgreSqlFullText)]
public class Product
{
    [FullTextSearch]
    public string Name { get; set; } = null!;
}
";
        var (compilation, diagnostics) = RunGenerator(source);

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

        var extensionTree = compilation.SyntaxTrees
            .FirstOrDefault(t => t.FilePath.Contains("SearchExtensions"));

        Assert.NotNull(extensionTree);
        var code = extensionTree!.ToString();

        // Should mention PostgreSQL in comments
        Assert.Contains("PostgreSQL", code);
    }

    [Fact]
    public void Generator_DefaultStrategy_IsLinqContains()
    {
        var source = @"
using Facet.Search;

namespace TestNamespace;

[FacetedSearch]
public class Product
{
    [FullTextSearch]
    public string Name { get; set; } = null!;
}
";
        var (compilation, diagnostics) = RunGenerator(source);

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

        var extensionTree = compilation.SyntaxTrees
            .FirstOrDefault(t => t.FilePath.Contains("SearchExtensions"));

        Assert.NotNull(extensionTree);
        var code = extensionTree!.ToString();

        // Default should be LinqContains
        Assert.Contains("LIKE", code);
        Assert.Contains("LinqContains", code);
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
