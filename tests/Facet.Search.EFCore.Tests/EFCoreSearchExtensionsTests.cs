using Facet.Search.EFCore.Tests.Fixtures;
using Facet.Search.EFCore.Tests.Models.Search;

namespace Facet.Search.EFCore.Tests;

/// <summary>
/// Integration tests for Facet.Search with EF Core using SQLite.
/// </summary>
[Collection("Database")]
public class EFCoreSearchExtensionsTests
{
    private readonly DatabaseFixture _fixture;

    public EFCoreSearchExtensionsTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ExecuteSearchAsync_ReturnsAllProducts_WhenNoFilter()
    {
        // Arrange
        var filter = new ProductSearchFilter();

        // Act
        var results = await _fixture.Context.Products
            .ApplyFacetedSearch(filter)
            .ExecuteSearchAsync();

        // Assert
        Assert.Equal(10, results.Count);
    }

    [Fact]
    public async Task ExecuteSearchAsync_FiltersByBrand_Correctly()
    {
        // Arrange
        var filter = new ProductSearchFilter
        {
            Brand = ["TechCorp"]
        };

        // Act
        var results = await _fixture.Context.Products
            .ApplyFacetedSearch(filter)
            .ExecuteSearchAsync();

        // Assert
        Assert.Equal(5, results.Count);
        Assert.All(results, p => Assert.Equal("TechCorp", p.Brand));
    }

    [Fact]
    public async Task ExecuteSearchAsync_FiltersByMultipleBrands_Correctly()
    {
        // Arrange
        var filter = new ProductSearchFilter
        {
            Brand = ["TechCorp", "GameTech"]
        };

        // Act
        var results = await _fixture.Context.Products
            .ApplyFacetedSearch(filter)
            .ExecuteSearchAsync();

        // Assert
        Assert.Equal(7, results.Count);
    }

    [Fact]
    public async Task ExecuteSearchAsync_FiltersByPriceRange_Correctly()
    {
        // Arrange
        var filter = new ProductSearchFilter
        {
            MinPrice = 100m,
            MaxPrice = 500m
        };

        // Act
        var results = await _fixture.Context.Products
            .ApplyFacetedSearch(filter)
            .ExecuteSearchAsync();

        // Assert
        Assert.All(results, p => Assert.InRange(p.Price, 100m, 500m));
    }

    [Fact]
    public async Task ExecuteSearchAsync_FiltersByInStock_Correctly()
    {
        // Arrange
        var filter = new ProductSearchFilter
        {
            InStock = true
        };

        // Act
        var results = await _fixture.Context.Products
            .ApplyFacetedSearch(filter)
            .ExecuteSearchAsync();

        // Assert
        Assert.Equal(7, results.Count);
        Assert.All(results, p => Assert.True(p.InStock));
    }

    [Fact]
    public async Task ExecuteSearchAsync_FiltersByFullTextSearch_Correctly()
    {
        // Arrange
        var filter = new ProductSearchFilter
        {
            SearchText = "laptop"
        };

        // Act
        var results = await _fixture.Context.Products
            .ApplyFacetedSearch(filter)
            .ExecuteSearchAsync();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.All(results, p => Assert.Contains("Laptop", p.Name));
    }

    [Fact]
    public async Task ExecuteSearchAsync_CombinedFilters_WorkCorrectly()
    {
        // Arrange
        var filter = new ProductSearchFilter
        {
            Category = ["Electronics"],
            InStock = true,
            MaxPrice = 500m
        };

        // Act
        var results = await _fixture.Context.Products
            .ApplyFacetedSearch(filter)
            .ExecuteSearchAsync();

        // Assert
        Assert.All(results, p =>
        {
            Assert.Equal("Electronics", p.Category);
            Assert.True(p.InStock);
            Assert.True(p.Price <= 500m);
        });
    }

    [Fact]
    public async Task CountSearchResultsAsync_ReturnsCorrectCount()
    {
        // Arrange
        var filter = new ProductSearchFilter
        {
            Brand = ["TechCorp"]
        };

        // Act
        var count = await _fixture.Context.Products
            .ApplyFacetedSearch(filter)
            .CountSearchResultsAsync();

        // Assert
        Assert.Equal(5, count);
    }

    [Fact]
    public async Task HasResultsAsync_ReturnsTrue_WhenResultsExist()
    {
        // Arrange
        var filter = new ProductSearchFilter
        {
            Brand = ["TechCorp"]
        };

        // Act
        var hasResults = await _fixture.Context.Products
            .ApplyFacetedSearch(filter)
            .HasResultsAsync();

        // Assert
        Assert.True(hasResults);
    }

    [Fact]
    public async Task HasResultsAsync_ReturnsFalse_WhenNoResults()
    {
        // Arrange
        var filter = new ProductSearchFilter
        {
            Brand = ["NonExistentBrand"]
        };

        // Act
        var hasResults = await _fixture.Context.Products
            .ApplyFacetedSearch(filter)
            .HasResultsAsync();

        // Assert
        Assert.False(hasResults);
    }
}
