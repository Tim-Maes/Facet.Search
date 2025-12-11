using Facet.Search.Tests.Models.Search;

namespace Facet.Search.Tests;

/// <summary>
/// Tests for generated metadata.
/// </summary>
public class MetadataGeneratorTests
{
    [Fact]
    public void Metadata_ShouldContainAllFacets()
    {
        // Arrange & Act
        var facets = TestProductSearchMetadata.Facets;

        // Assert
        Assert.Equal(5, facets.Count);
    }

    [Fact]
    public void Metadata_ShouldHaveCorrectFacetNames()
    {
        // Arrange & Act
        var facets = TestProductSearchMetadata.Facets;
        var facetNames = facets.Select(f => f.Name).ToList();

        // Assert
        Assert.Contains("Brand", facetNames);
        Assert.Contains("Category", facetNames);
        Assert.Contains("Price", facetNames);
        Assert.Contains("InStock", facetNames);
        Assert.Contains("CreatedAt", facetNames);
    }

    [Fact]
    public void Metadata_ShouldHaveCorrectDisplayNames()
    {
        // Arrange & Act
        var facets = TestProductSearchMetadata.Facets;
        var brandFacet = facets.First(f => f.Name == "Brand");

        // Assert
        Assert.Equal("Brand Name", brandFacet.DisplayName);
    }

    [Fact]
    public void Metadata_ShouldHaveCorrectFacetTypes()
    {
        // Arrange & Act
        var facets = TestProductSearchMetadata.Facets;

        // Assert
        Assert.Equal("Categorical", facets.First(f => f.Name == "Brand").Type);
        Assert.Equal("Range", facets.First(f => f.Name == "Price").Type);
        Assert.Equal("Boolean", facets.First(f => f.Name == "InStock").Type);
        Assert.Equal("DateRange", facets.First(f => f.Name == "CreatedAt").Type);
    }
}
