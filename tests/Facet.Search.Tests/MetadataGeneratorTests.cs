using Facet.Search.Tests.Models.Search;

namespace Facet.Search.Tests;

/// <summary>
/// Tests for generated metadata using actual generated code.
/// </summary>
public class MetadataGeneratorTests
{
    [Fact]
    public void Metadata_ContainsAllFacets()
    {
        // Act
        var facets = TestProductSearchMetadata.Facets;

        // Assert - Brand, Category, Price, InStock, CreatedAt = 5 facets
        Assert.Equal(5, facets.Count);
    }

    [Fact]
    public void Metadata_BrandFacet_HasCorrectProperties()
    {
        // Act
        var brandFacet = TestProductSearchMetadata.Facets.First(f => f.PropertyName == "Brand");

        // Assert
        Assert.Equal("Brand", brandFacet.PropertyName);
        Assert.Equal("Brand Name", brandFacet.DisplayName);
        Assert.Equal("Categorical", brandFacet.Type);
        Assert.False(brandFacet.IsHierarchical);
    }

    [Fact]
    public void Metadata_PriceFacet_HasCorrectType()
    {
        // Act
        var priceFacet = TestProductSearchMetadata.Facets.First(f => f.PropertyName == "Price");

        // Assert
        Assert.Equal("Price", priceFacet.PropertyName);
        Assert.Equal("Price", priceFacet.DisplayName);
        Assert.Equal("Range", priceFacet.Type);
    }

    [Fact]
    public void Metadata_InStockFacet_HasCorrectType()
    {
        // Act
        var inStockFacet = TestProductSearchMetadata.Facets.First(f => f.PropertyName == "InStock");

        // Assert
        Assert.Equal("InStock", inStockFacet.PropertyName);
        Assert.Equal("In Stock", inStockFacet.DisplayName);
        Assert.Equal("Boolean", inStockFacet.Type);
    }

    [Fact]
    public void Metadata_CreatedAtFacet_HasCorrectType()
    {
        // Act
        var createdAtFacet = TestProductSearchMetadata.Facets.First(f => f.PropertyName == "CreatedAt");

        // Assert
        Assert.Equal("CreatedAt", createdAtFacet.PropertyName);
        Assert.Equal("Created Date", createdAtFacet.DisplayName);
        Assert.Equal("DateRange", createdAtFacet.Type);
    }

    [Fact]
    public void Metadata_FacetsAreReadOnly()
    {
        // Act
        var facets = TestProductSearchMetadata.Facets;

        // Assert
        Assert.IsAssignableFrom<IReadOnlyList<TestProductFacetMetadata>>(facets);
    }

    [Fact]
    public void Metadata_AllFacetsHaveOrderBy()
    {
        // Act
        var facets = TestProductSearchMetadata.Facets;

        // Assert
        Assert.All(facets, f => Assert.NotNull(f.OrderBy));
    }

    [Fact]
    public void Metadata_CategoryFacet_HasCorrectDisplayName()
    {
        // Act
        var categoryFacet = TestProductSearchMetadata.Facets.First(f => f.PropertyName == "Category");

        // Assert
        Assert.Equal("Category", categoryFacet.DisplayName);
    }
}
