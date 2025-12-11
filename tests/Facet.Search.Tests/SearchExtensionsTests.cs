using Facet.Search.Tests.Models;
using Facet.Search.Tests.Models.Search;

namespace Facet.Search.Tests;

/// <summary>
/// Tests for generated search extension methods.
/// </summary>
public class SearchExtensionsTests
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
    public void ApplyFacetedSearch_WithNullFilter_ReturnsAllItems()
    {
        // Arrange
        var products = GetTestProducts().AsQueryable();

        // Act
        var results = products.ApplyFacetedSearch(null!).ToList();

        // Assert
        Assert.Equal(5, results.Count);
    }

    [Fact]
    public void ApplyFacetedSearch_WithEmptyFilter_ReturnsAllItems()
    {
        // Arrange
        var products = GetTestProducts().AsQueryable();
        var filter = new TestProductSearchFilter();

        // Act
        var results = products.ApplyFacetedSearch(filter).ToList();

        // Assert
        Assert.Equal(5, results.Count);
    }

    [Fact]
    public void ApplyFacetedSearch_WithBrandFilter_FiltersCorrectly()
    {
        // Arrange
        var products = GetTestProducts().AsQueryable();
        var filter = new TestProductSearchFilter
        {
            Brand = ["TechCorp"]
        };

        // Act
        var results = products.ApplyFacetedSearch(filter).ToList();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.All(results, p => Assert.Equal("TechCorp", p.Brand));
    }

    [Fact]
    public void ApplyFacetedSearch_WithMultipleBrands_FiltersCorrectly()
    {
        // Arrange
        var products = GetTestProducts().AsQueryable();
        var filter = new TestProductSearchFilter
        {
            Brand = ["TechCorp", "GameTech"]
        };

        // Act
        var results = products.ApplyFacetedSearch(filter).ToList();

        // Assert
        Assert.Equal(3, results.Count);
    }

    [Fact]
    public void ApplyFacetedSearch_WithPriceRange_FiltersCorrectly()
    {
        // Arrange
        var products = GetTestProducts().AsQueryable();
        var filter = new TestProductSearchFilter
        {
            MinPrice = 100m,
            MaxPrice = 600m  // Fixed: Standing Desk is 599.99
        };

        // Act
        var results = products.ApplyFacetedSearch(filter).ToList();

        // Assert
        Assert.Equal(2, results.Count);  // Office Chair (299.99) and Standing Desk (599.99)
        Assert.All(results, p => Assert.InRange(p.Price, 100m, 600m));
    }

    [Fact]
    public void ApplyFacetedSearch_WithBooleanFilter_FiltersCorrectly()
    {
        // Arrange
        var products = GetTestProducts().AsQueryable();
        var filter = new TestProductSearchFilter
        {
            InStock = true
        };

        // Act
        var results = products.ApplyFacetedSearch(filter).ToList();

        // Assert
        Assert.Equal(3, results.Count);
        Assert.All(results, p => Assert.True(p.InStock));
    }

    [Fact]
    public void ApplyFacetedSearch_WithFullTextSearch_FiltersCorrectly()
    {
        // Arrange
        var products = GetTestProducts().AsQueryable();
        var filter = new TestProductSearchFilter
        {
            SearchText = "laptop"
        };

        // Act
        var results = products.ApplyFacetedSearch(filter).ToList();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.All(results, p => Assert.Contains("Laptop", p.Name));
    }

    [Fact]
    public void ApplyFacetedSearch_WithCombinedFilters_FiltersCorrectly()
    {
        // Arrange
        var products = GetTestProducts().AsQueryable();
        var filter = new TestProductSearchFilter
        {
            Category = ["Electronics"],
            InStock = true,
            MaxPrice = 1500m
        };

        // Act
        var results = products.ApplyFacetedSearch(filter).ToList();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.All(results, p =>
        {
            Assert.Equal("Electronics", p.Category);
            Assert.True(p.InStock);
            Assert.True(p.Price <= 1500m);
        });
    }

    [Fact]
    public void ApplyFacetedSearch_WithDateRangeFilter_FiltersCorrectly()
    {
        // Arrange
        var products = GetTestProducts().AsQueryable();
        var filter = new TestProductSearchFilter
        {
            CreatedAtFrom = DateTime.Now.AddDays(-20),
            CreatedAtTo = DateTime.Now
        };

        // Act
        var results = products.ApplyFacetedSearch(filter).ToList();

        // Assert
        Assert.Equal(3, results.Count); // Mouse (-10), Desk (-5), Gaming Laptop (-15)
    }
}
