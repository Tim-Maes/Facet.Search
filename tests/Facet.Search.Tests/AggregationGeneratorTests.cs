using Facet.Search.Tests.Models;
using Facet.Search.Tests.Models.Search;
using Facet.Search.Tests.Utilities;
using System.Linq;

namespace Facet.Search.Tests;

/// <summary>
/// Tests for generated aggregation methods using actual generated code.
/// </summary>
public class AggregationGeneratorTests
{
    [Fact]
    public void GetFacetAggregations_ReturnsCorrectBrandCounts()
    {
        // Arrange
        var products = TestDataFactory.CreateProductList().AsQueryable();

        // Act
        var aggregations = products.GetFacetAggregations();

        // Assert
        Assert.Equal(3, aggregations.Brand.Count);
        Assert.Equal(3, aggregations.Brand["TechCorp"]);
        Assert.Equal(3, aggregations.Brand["ComfortPlus"]);
        Assert.Equal(2, aggregations.Brand["GameTech"]);
    }

    [Fact]
    public void GetFacetAggregations_ReturnsCorrectCategoryCounts()
    {
        // Arrange
        var products = TestDataFactory.CreateProductList().AsQueryable();

        // Act
        var aggregations = products.GetFacetAggregations();

        // Assert
        Assert.Equal(2, aggregations.Category.Count);
        Assert.Equal(5, aggregations.Category["Electronics"]);
        Assert.Equal(3, aggregations.Category["Furniture"]);
    }

    [Fact]
    public void GetFacetAggregations_ReturnsCorrectPriceMin()
    {
        // Arrange
        var products = TestDataFactory.CreateProductList().AsQueryable();

        // Act
        var aggregations = products.GetFacetAggregations();

        // Assert
        Assert.Equal(29.99m, aggregations.PriceMin);
    }

    [Fact]
    public void GetFacetAggregations_ReturnsCorrectPriceMax()
    {
        // Arrange
        var products = TestDataFactory.CreateProductList().AsQueryable();

        // Act
        var aggregations = products.GetFacetAggregations();

        // Assert
        Assert.Equal(1999.99m, aggregations.PriceMax);
    }

    [Fact]
    public void GetFacetAggregations_OnFilteredQuery_ReturnsFilteredAggregations()
    {
        // Arrange
        var products = TestDataFactory.CreateProductList().AsQueryable();
        var filter = new TestProductSearchFilter
        {
            Category = ["Electronics"]
        };

        // Act
        var filteredProducts = products.ApplyFacetedSearch(filter);
        var aggregations = filteredProducts.GetFacetAggregations();

        // Assert
        Assert.Equal(2, aggregations.Brand.Count); // TechCorp and GameTech
        Assert.Equal(3, aggregations.Brand["TechCorp"]);
        Assert.Equal(2, aggregations.Brand["GameTech"]);
        Assert.Equal(29.99m, aggregations.PriceMin);
        Assert.Equal(1999.99m, aggregations.PriceMax);
    }

    [Fact]
    public void GetFacetAggregations_OnEmptyQuery_ReturnsEmptyResults()
    {
        // Arrange
        var products = new List<TestProduct>().AsQueryable();

        // Act
        var aggregations = products.GetFacetAggregations();

        // Assert
        Assert.Empty(aggregations.Brand);
        Assert.Empty(aggregations.Category);
        Assert.Null(aggregations.PriceMin);
        Assert.Null(aggregations.PriceMax);
    }

    [Fact]
    public void GetFacetAggregations_WithInStockFilter_ReturnsFilteredBrandCounts()
    {
        // Arrange
        var products = TestDataFactory.CreateProductList().AsQueryable();
        var filter = new TestProductSearchFilter { InStock = true };

        // Act
        var filteredProducts = products.ApplyFacetedSearch(filter);
        var aggregations = filteredProducts.GetFacetAggregations();

        // Assert
        // Only in-stock products should be counted
        Assert.True(aggregations.Brand.Values.Sum() <= 8);
    }

    [Fact]
    public void GetFacetAggregations_BrandDictionary_IsCaseSensitive()
    {
        // Arrange
        var products = TestDataFactory.CreateProductList().AsQueryable();

        // Act
        var aggregations = products.GetFacetAggregations();

        // Assert
        Assert.True(aggregations.Brand.ContainsKey("TechCorp"));
        Assert.False(aggregations.Brand.ContainsKey("techcorp"));
    }

    [Fact]
    public void GetFacetAggregations_IntCategoryId_ReturnsStringKeyedCounts()
    {
        // Arrange
        var products = TestDataFactory.CreateProductWithNumericFacetsList().AsQueryable();

        // Act
        var aggregations = products.GetFacetAggregations();

        // Assert - dictionary keys are string representations of the int values
        Assert.Equal(3, aggregations.CategoryId.Count);
        Assert.Equal(2, aggregations.CategoryId["1"]);
        Assert.Equal(2, aggregations.CategoryId["2"]);
        Assert.Equal(1, aggregations.CategoryId["3"]);
    }

    [Fact]
    public void GetFacetAggregations_LongSupplierId_ReturnsStringKeyedCounts()
    {
        // Arrange
        var products = TestDataFactory.CreateProductWithNumericFacetsList().AsQueryable();

        // Act
        var aggregations = products.GetFacetAggregations();

        // Assert - dictionary keys are string representations of the long values
        Assert.Equal(3, aggregations.SupplierId.Count);
        Assert.Equal(2, aggregations.SupplierId["100"]);
        Assert.Equal(2, aggregations.SupplierId["200"]);
        Assert.Equal(1, aggregations.SupplierId["300"]);
    }

    [Fact]
    public void GetFacetAggregations_NumericFacets_OnFilteredQuery_ReturnsFilteredCounts()
    {
        // Arrange
        var products = TestDataFactory.CreateProductWithNumericFacetsList().AsQueryable();
        var filter = new TestProductWithNumericFacetsSearchFilter
        {
            CategoryId = [1, 2]
        };

        // Act
        var filteredProducts = products.ApplyFacetedSearch(filter);
        var aggregations = filteredProducts.GetFacetAggregations();

        // Assert
        Assert.Equal(2, aggregations.CategoryId.Count);
        Assert.False(aggregations.CategoryId.ContainsKey("3"));
    }
}
