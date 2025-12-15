using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Facet.Search.Generators;

namespace Facet.Search.Tests;

/// <summary>
/// Tests for navigation property paths.
/// </summary>
public class NavigationPropertyTests
{
    [Fact]
    public void Generator_WithNavigationPath_GeneratesFilterProperty()
    {
        var source = @"
using Facet.Search;

namespace TestNamespace;

[FacetedSearch]
public class Order
{
    public int Id { get; set; }

    [SearchFacet(Type = FacetType.Categorical, NavigationPath = ""Customer.Name"")]
    public Customer? Customer { get; set; }
}

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
}
";
        var (compilation, diagnostics) = RunGenerator(source);

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

        var filterTree = compilation.SyntaxTrees
            .FirstOrDefault(t => t.FilePath.Contains("OrderSearchFilter"));

        Assert.NotNull(filterTree);
        var code = filterTree!.ToString();

        // Should generate a filter property for Customer
        Assert.Contains("Customer", code);
        Assert.Contains("string[]?", code);
    }

    [Fact]
    public void Generator_WithNavigationPath_IncludesPathInExtensions()
    {
        var source = @"
using Facet.Search;

namespace TestNamespace;

[FacetedSearch]
public class Order
{
    [SearchFacet(Type = FacetType.Categorical, NavigationPath = ""Customer.Name"")]
    public Customer? Customer { get; set; }
}

public class Customer
{
    public string Name { get; set; } = null!;
}
";
        var (compilation, diagnostics) = RunGenerator(source);

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

        var extensionTree = compilation.SyntaxTrees
            .FirstOrDefault(t => t.FilePath.Contains("SearchExtensions"));

        Assert.NotNull(extensionTree);
        var code = extensionTree!.ToString();

        // Should reference the navigation path in the filter
        Assert.Contains("Customer", code);
    }

    [Fact]
    public void Generator_WithoutNavigationPath_UsesPropertyNameDirectly()
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

        var extensionTree = compilation.SyntaxTrees
            .FirstOrDefault(t => t.FilePath.Contains("SearchExtensions"));

        Assert.NotNull(extensionTree);
        var code = extensionTree!.ToString();

        // Should use x.Brand directly
        Assert.Contains("x.Brand", code);
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
