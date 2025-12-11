using Facet.Search.Tests.Models;
using Facet.Search.Tests.Models.Search;

namespace Facet.Search.Tests;

/// <summary>
/// Tests for generated aggregation methods.
/// </summary>
public class AggregationGeneratorTests
{
    private static List<TestProduct> GetTestProducts() =>
    [
        new() { Id = 1, Name = "Laptop Pro", Brand = "TechCorp", Category = "Electronics", Price = 1299.99m, InStock = true, CreatedAt = DateTime.Now.AddDays(-30), Rating = 5 },
        new() { Id = 2, Name = "Wireless Mouse", Brand = "TechCorp", Category = "Electronics", Price = 49.99m, InStock = true, CreatedAt = DateTime.Now.AddDays(-10), Rating = 4 },
        new() { Id = 3, Name = "Office Chair", Brand = "ComfortPlus", Category = "Furniture", Price = 299.99m, InStock = false, CreatedAt = DateTime.Now.AddDays(-60), Rating = 4 },
        new() { Id = 4, Name = "Standing Desk", Brand = "ComfortPlus", Category = "Furniture", Price = 599.99m, InStock = true, CreatedAt = DateTime.Now.AddDays(-5), Rating = 5 },
        new() { Id = 5, Name = "Gaming Laptop", Brand = "GameTech", Category = "Electronics", Price = 1999.99m, InStock = false, CreatedAt = DateTime.Now.AddDays(-15), Rating = 5 },
    ];

    [Fact]
    public void GetFacetAggregations_ReturnsCorrectBrandCounts()
    {
        // Arrange
        var products = GetTestProducts().AsQueryable();

        // Act
        var aggregations = products.GetFacetAggregations();

        // Assert
        Assert.Equal(3, aggregations.Brand.Count);
        Assert.Equal(2, aggregations.Brand["TechCorp"]);
        Assert.Equal(2, aggregations.Brand["ComfortPlus"]);
        Assert.Equal(1, aggregations.Brand["GameTech"]);
    }

    [Fact]
    public void GetFacetAggregations_ReturnsCorrectCategoryCounts()
    {
        // Arrange
        var products = GetTestProducts().AsQueryable();

        // Act
        var aggregations = products.GetFacetAggregations();

        // Assert
        Assert.Equal(2, aggregations.Category.Count);
        Assert.Equal(3, aggregations.Category["Electronics"]);
        Assert.Equal(2, aggregations.Category["Furniture"]);
    }

    [Fact]
    public void GetFacetAggregations_ReturnsCorrectPriceRange()
    {
        // Arrange
        var products = GetTestProducts().AsQueryable();

        // Act
        var aggregations = products.GetFacetAggregations();

        // Assert
        Assert.Equal(49.99m, aggregations.PriceMin);
        Assert.Equal(1999.99m, aggregations.PriceMax);
    }

    [Fact]
    public void GetFacetAggregations_OnFilteredQuery_ReturnsFilteredAggregations()
    {
        // Arrange
        var products = GetTestProducts().AsQueryable();
        var filter = new TestProductSearchFilter
        {
            Category = ["Electronics"]
        };

        // Act
        var filteredProducts = products.ApplyFacetedSearch(filter);
        var aggregations = filteredProducts.GetFacetAggregations();

        // Assert
        Assert.Equal(2, aggregations.Brand.Count); // TechCorp and GameTech
        Assert.Equal(2, aggregations.Brand["TechCorp"]);
        Assert.Equal(1, aggregations.Brand["GameTech"]);
        Assert.Equal(49.99m, aggregations.PriceMin);
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
}
