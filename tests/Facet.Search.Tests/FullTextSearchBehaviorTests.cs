using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Facet.Search.Generators;

namespace Facet.Search.Tests;

/// <summary>
/// Tests for full-text search behavior options.
/// </summary>
public class FullTextSearchBehaviorTests
{
    [Fact]
    public void Generator_WithContainsBehavior_GeneratesContainsExpression()
    {
        var source = @"
using Facet.Search;

namespace TestNamespace;

[FacetedSearch]
public class Article
{
    [FullTextSearch(Behavior = TextSearchBehavior.Contains)]
    public string Title { get; set; } = null!;
}
";
        var (compilation, diagnostics) = RunGenerator(source);

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

        var extensionTree = compilation.SyntaxTrees
            .FirstOrDefault(t => t.FilePath.Contains("SearchExtensions"));

        Assert.NotNull(extensionTree);
        var code = extensionTree!.ToString();

        Assert.Contains("Contains(searchTerm)", code);
    }

    [Fact]
    public void Generator_WithStartsWithBehavior_GeneratesStartsWithExpression()
    {
        var source = @"
using Facet.Search;

namespace TestNamespace;

[FacetedSearch]
public class Article
{
    [FullTextSearch(Behavior = TextSearchBehavior.StartsWith)]
    public string Slug { get; set; } = null!;
}
";
        var (compilation, diagnostics) = RunGenerator(source);

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

        var extensionTree = compilation.SyntaxTrees
            .FirstOrDefault(t => t.FilePath.Contains("SearchExtensions"));

        Assert.NotNull(extensionTree);
        var code = extensionTree!.ToString();

        Assert.Contains("StartsWith(searchTerm)", code);
    }

    [Fact]
    public void Generator_WithEndsWithBehavior_GeneratesEndsWithExpression()
    {
        var source = @"
using Facet.Search;

namespace TestNamespace;

[FacetedSearch]
public class Article
{
    [FullTextSearch(Behavior = TextSearchBehavior.EndsWith)]
    public string Extension { get; set; } = null!;
}
";
        var (compilation, diagnostics) = RunGenerator(source);

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

        var extensionTree = compilation.SyntaxTrees
            .FirstOrDefault(t => t.FilePath.Contains("SearchExtensions"));

        Assert.NotNull(extensionTree);
        var code = extensionTree!.ToString();

        Assert.Contains("EndsWith(searchTerm)", code);
    }

    [Fact]
    public void Generator_WithExactBehavior_GeneratesEqualityExpression()
    {
        var source = @"
using Facet.Search;

namespace TestNamespace;

[FacetedSearch]
public class Article
{
    [FullTextSearch(Behavior = TextSearchBehavior.Exact)]
    public string Code { get; set; } = null!;
}
";
        var (compilation, diagnostics) = RunGenerator(source);

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

        var extensionTree = compilation.SyntaxTrees
            .FirstOrDefault(t => t.FilePath.Contains("SearchExtensions"));

        Assert.NotNull(extensionTree);
        var code = extensionTree!.ToString();

        Assert.Contains("== searchTerm", code);
    }

    [Fact]
    public void Generator_WithMultipleFullTextProperties_GeneratesOrExpression()
    {
        var source = @"
using Facet.Search;

namespace TestNamespace;

[FacetedSearch]
public class Article
{
    [FullTextSearch]
    public string Title { get; set; } = null!;

    [FullTextSearch]
    public string? Description { get; set; }

    [FullTextSearch]
    public string? Content { get; set; }
}
";
        var (compilation, diagnostics) = RunGenerator(source);

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

        var extensionTree = compilation.SyntaxTrees
            .FirstOrDefault(t => t.FilePath.Contains("SearchExtensions"));

        Assert.NotNull(extensionTree);
        var code = extensionTree!.ToString();

        // Should have OR operators combining the search conditions
        Assert.Contains("||", code);
        Assert.Contains("Title", code);
        Assert.Contains("Description", code);
        Assert.Contains("Content", code);
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
