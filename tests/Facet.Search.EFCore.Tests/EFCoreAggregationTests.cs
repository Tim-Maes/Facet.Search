using Facet.Search.EFCore.Tests.Fixtures;

namespace Facet.Search.EFCore.Tests;

/// <summary>
/// Integration tests for facet aggregations with EF Core.
/// </summary>
[Collection("Database")]
public class EFCoreAggregationTests
{
    private readonly DatabaseFixture _fixture;

    public EFCoreAggregationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task AggregateFacetAsync_ReturnsBrandCounts()
    {
        // Arrange & Act
        var brandCounts = await _fixture.Context.Products
            .AggregateFacetAsync(p => p.Brand);

        // Assert
        Assert.Equal(4, brandCounts.Count);
        Assert.Equal(5, brandCounts["TechCorp"]);
        Assert.Equal(2, brandCounts["ComfortPlus"]);
        Assert.Equal(2, brandCounts["GameTech"]);
        Assert.Equal(1, brandCounts["HomeLight"]);
    }

    [Fact]
    public async Task AggregateFacetAsync_WithLimit_ReturnsTopN()
    {
        // Arrange & Act
        var brandCounts = await _fixture.Context.Products
            .AggregateFacetAsync(p => p.Brand, limit: 2);

        // Assert
        Assert.Equal(2, brandCounts.Count);
        Assert.True(brandCounts.ContainsKey("TechCorp")); // Most products
    }

    [Fact]
    public async Task AggregateFacetAsync_ReturnsCategoryCounts()
    {
        // Arrange & Act
        var categoryCounts = await _fixture.Context.Products
            .AggregateFacetAsync(p => p.Category);

        // Assert
        Assert.Equal(2, categoryCounts.Count);
        Assert.Equal(7, categoryCounts["Electronics"]);
        Assert.Equal(3, categoryCounts["Furniture"]);
    }

    [Fact]
    public async Task GetRangeAsync_ReturnsPriceMinMax()
    {
        // Arrange & Act
        var (min, max) = await _fixture.Context.Products
            .GetRangeAsync(p => p.Price);

        // Assert
        Assert.NotNull(min);
        Assert.NotNull(max);
        Assert.Equal(29.99m, min);
        Assert.Equal(1999.99m, max);
    }

    [Fact]
    public async Task GetMinAsync_ReturnsMinPrice()
    {
        // Arrange & Act
        var minPrice = await _fixture.Context.Products
            .GetMinAsync(p => p.Price);

        // Assert
        Assert.Equal(29.99m, minPrice);
    }

    [Fact]
    public async Task GetMaxAsync_ReturnsMaxPrice()
    {
        // Arrange & Act
        var maxPrice = await _fixture.Context.Products
            .GetMaxAsync(p => p.Price);

        // Assert
        Assert.Equal(1999.99m, maxPrice);
    }

    [Fact]
    public async Task CountBooleanAsync_ReturnsInStockCounts()
    {
        // Arrange & Act
        var (trueCount, falseCount) = await _fixture.Context.Products
            .CountBooleanAsync(p => p.InStock);

        // Assert
        Assert.Equal(7, trueCount);
        Assert.Equal(3, falseCount);
    }

    [Fact]
    public async Task GetRangeAsync_OnEmptyQuery_ReturnsNulls()
    {
        // Arrange
        var emptyQuery = _fixture.Context.Products.Where(p => p.Brand == "NonExistent");

        // Act
        var (min, max) = await emptyQuery.GetRangeAsync(p => p.Price);

        // Assert
        Assert.Null(min);
        Assert.Null(max);
    }

    [Fact]
    public async Task AggregateFacetAsync_OnFilteredQuery_ReturnsFilteredAggregations()
    {
        // Arrange
        var electronicsQuery = _fixture.Context.Products.Where(p => p.Category == "Electronics");

        // Act
        var brandCounts = await electronicsQuery.AggregateFacetAsync(p => p.Brand);

        // Assert
        // Electronics has: TechCorp (5 products), GameTech (2 products)
        Assert.Equal(2, brandCounts.Count);
        Assert.Equal(5, brandCounts["TechCorp"]);
        Assert.Equal(2, brandCounts["GameTech"]);
        Assert.False(brandCounts.ContainsKey("ComfortPlus"));
        Assert.False(brandCounts.ContainsKey("HomeLight"));
    }

    [Fact]
    public async Task GetRangeAsync_OnFilteredQuery_ReturnsFilteredRange()
    {
        // Arrange
        var inStockQuery = _fixture.Context.Products.Where(p => p.InStock);

        // Act
        var (min, max) = await inStockQuery.GetRangeAsync(p => p.Price);

        // Assert
        Assert.NotNull(min);
        Assert.NotNull(max);
        Assert.Equal(29.99m, min);
        Assert.Equal(1299.99m, max); // Gaming laptop ($1999.99) is out of stock
    }
}
