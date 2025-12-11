using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Facet.Search.Generators;

namespace Facet.Search.Tests;

/// <summary>
/// Tests that verify the source generator produces correct output.
/// </summary>
public class SourceGeneratorTests
{
    [Fact]
    public void Generator_WithFacetedSearchAttribute_GeneratesFilterClass()
    {
        // Arrange
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
        // Act
        var (compilation, diagnostics) = RunGenerator(source);

        // Assert
        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        
        var generatedTrees = compilation.SyntaxTrees
            .Where(t => t.FilePath.Contains(".g.cs"))
            .ToList();
        
        Assert.True(generatedTrees.Count >= 1, "Should generate at least one file");
        
        var filterClassGenerated = generatedTrees.Any(t => 
            t.FilePath.Contains("ProductSearchFilter"));
        Assert.True(filterClassGenerated, "Should generate ProductSearchFilter.g.cs");
    }

    [Fact]
    public void Generator_WithRangeFacet_GeneratesMinMaxProperties()
    {
        // Arrange
        var source = @"
using Facet.Search;

namespace TestNamespace;

[FacetedSearch]
public class Product
{
    [SearchFacet(Type = FacetType.Range)]
    public decimal Price { get; set; }
}
";
        // Act
        var (compilation, diagnostics) = RunGenerator(source);

        // Assert
        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        
        var filterClassTree = compilation.SyntaxTrees
            .FirstOrDefault(t => t.FilePath.Contains("ProductSearchFilter"));
        
        Assert.NotNull(filterClassTree);
        var filterCode = filterClassTree!.ToString();
        
        Assert.Contains("MinPrice", filterCode);
        Assert.Contains("MaxPrice", filterCode);
    }

    [Fact]
    public void Generator_WithFullTextSearch_GeneratesSearchTextProperty()
    {
        // Arrange
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
        // Act
        var (compilation, diagnostics) = RunGenerator(source);

        // Assert
        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        
        var filterClassTree = compilation.SyntaxTrees
            .FirstOrDefault(t => t.FilePath.Contains("ProductSearchFilter"));
        
        Assert.NotNull(filterClassTree);
        var filterCode = filterClassTree!.ToString();
        
        Assert.Contains("SearchText", filterCode);
    }

    [Fact]
    public void Generator_WithBooleanFacet_GeneratesNullableBoolProperty()
    {
        // Arrange
        var source = @"
using Facet.Search;

namespace TestNamespace;

[FacetedSearch]
public class Product
{
    [SearchFacet(Type = FacetType.Boolean)]
    public bool InStock { get; set; }
}
";
        // Act
        var (compilation, diagnostics) = RunGenerator(source);

        // Assert
        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        
        var filterClassTree = compilation.SyntaxTrees
            .FirstOrDefault(t => t.FilePath.Contains("ProductSearchFilter"));
        
        Assert.NotNull(filterClassTree);
        var filterCode = filterClassTree!.ToString();
        
        Assert.Contains("bool?", filterCode);
        Assert.Contains("InStock", filterCode);
    }

    [Fact]
    public void Generator_WithDateRangeFacet_GeneratesFromToProperties()
    {
        // Arrange
        var source = @"
using System;
using Facet.Search;

namespace TestNamespace;

[FacetedSearch]
public class Product
{
    [SearchFacet(Type = FacetType.DateRange)]
    public DateTime CreatedAt { get; set; }
}
";
        // Act
        var (compilation, diagnostics) = RunGenerator(source);

        // Assert
        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        
        var filterClassTree = compilation.SyntaxTrees
            .FirstOrDefault(t => t.FilePath.Contains("ProductSearchFilter"));
        
        Assert.NotNull(filterClassTree);
        var filterCode = filterClassTree!.ToString();
        
        Assert.Contains("CreatedAtFrom", filterCode);
        Assert.Contains("CreatedAtTo", filterCode);
    }

    [Fact]
    public void Generator_GeneratesExtensionMethods()
    {
        // Arrange
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
        // Act
        var (compilation, diagnostics) = RunGenerator(source);

        // Assert
        var extensionTree = compilation.SyntaxTrees
            .FirstOrDefault(t => t.FilePath.Contains("SearchExtensions"));
        
        Assert.NotNull(extensionTree);
        var extensionCode = extensionTree!.ToString();
        
        Assert.Contains("ApplyFacetedSearch", extensionCode);
    }

    [Fact]
    public void Generator_GeneratesAggregations()
    {
        // Arrange
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
        // Act
        var (compilation, diagnostics) = RunGenerator(source);

        // Assert
        var aggregationTree = compilation.SyntaxTrees
            .FirstOrDefault(t => t.FilePath.Contains("FacetAggregations"));
        
        Assert.NotNull(aggregationTree);
        var aggregationCode = aggregationTree!.ToString();
        
        Assert.Contains("GetFacetAggregations", aggregationCode);
        Assert.Contains("ProductFacetResults", aggregationCode);
    }

    [Fact]
    public void Generator_GeneratesMetadata()
    {
        // Arrange
        var source = @"
using Facet.Search;

namespace TestNamespace;

[FacetedSearch]
public class Product
{
    [SearchFacet(Type = FacetType.Categorical, DisplayName = ""Brand Name"")]
    public string Brand { get; set; } = null!;
}
";
        // Act
        var (compilation, diagnostics) = RunGenerator(source);

        // Assert
        var metadataTree = compilation.SyntaxTrees
            .FirstOrDefault(t => t.FilePath.Contains("SearchMetadata"));
        
        Assert.NotNull(metadataTree);
        var metadataCode = metadataTree!.ToString();
        
        Assert.Contains("ProductSearchMetadata", metadataCode);
        Assert.Contains("FacetMetadata", metadataCode);
        Assert.Contains("Brand Name", metadataCode);
    }

    private static (Compilation, ImmutableArray<Diagnostic>) RunGenerator(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .Cast<MetadataReference>()
            .ToList();
        
        // Add reference to Facet.Search assembly
        var facetSearchAssembly = typeof(FacetedSearchAttribute).Assembly;
        references.Add(MetadataReference.CreateFromFile(facetSearchAssembly.Location));

        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new FacetSearchGenerator();
        
        CSharpGeneratorDriver.Create(generator)
            .RunGeneratorsAndUpdateCompilation(
                compilation, 
                out var outputCompilation, 
                out var diagnostics);

        return (outputCompilation, diagnostics);
    }
}
