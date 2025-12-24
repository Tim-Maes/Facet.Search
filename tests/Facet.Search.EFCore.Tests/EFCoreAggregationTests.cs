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

    [Fact]
    public async Task GetFacetAggregationsAsync_ReturnsAllAggregations()
    {
        // Arrange & Act
        var results = await _fixture.Context.Products
            .GetFacetAggregationsAsync<Models.Product, Models.Search.ProductFacetResults>();

        // Assert - Brand facet
        Assert.NotNull(results.Brand);
        Assert.Equal(4, results.Brand.Count);
        Assert.Equal(5, results.Brand["TechCorp"]);
        Assert.Equal(2, results.Brand["ComfortPlus"]);
        Assert.Equal(2, results.Brand["GameTech"]);
        Assert.Equal(1, results.Brand["HomeLight"]);

        // Assert - Category facet
        Assert.NotNull(results.Category);
        Assert.Equal(2, results.Category.Count);
        Assert.Equal(7, results.Category["Electronics"]);
        Assert.Equal(3, results.Category["Furniture"]);

        // Assert - Price range
        Assert.Equal(29.99m, results.PriceMin);
        Assert.Equal(1999.99m, results.PriceMax);

        // Assert - InStock boolean
        Assert.Equal(7, results.InStockTrueCount);
        Assert.Equal(3, results.InStockFalseCount);
    }

    [Fact]
    public async Task GetFacetAggregationsAsync_OnFilteredQuery_ReturnsFilteredAggregations()
    {
        // Arrange
        var electronicsQuery = _fixture.Context.Products.Where(p => p.Category == "Electronics");

        // Act
        var results = await electronicsQuery
            .GetFacetAggregationsAsync<Models.Product, Models.Search.ProductFacetResults>();

        // Assert - Only electronics brands
        Assert.NotNull(results.Brand);
        Assert.Equal(2, results.Brand.Count);
        Assert.Equal(5, results.Brand["TechCorp"]);
        Assert.Equal(2, results.Brand["GameTech"]);
        Assert.False(results.Brand.ContainsKey("ComfortPlus"));
        Assert.False(results.Brand.ContainsKey("HomeLight"));

        // Assert - Category should only have Electronics
        Assert.NotNull(results.Category);
        Assert.Single(results.Category);
        Assert.Equal(7, results.Category["Electronics"]);
    }

    [Fact]
    public async Task GetFacetAggregationsAsync_OnEmptyQuery_ReturnsEmptyAggregations()
    {
        // Arrange
        var emptyQuery = _fixture.Context.Products.Where(p => p.Brand == "NonExistent");

        // Act
        var results = await emptyQuery
            .GetFacetAggregationsAsync<Models.Product, Models.Search.ProductFacetResults>();

        // Assert
        Assert.NotNull(results.Brand);
        Assert.Empty(results.Brand);
        Assert.NotNull(results.Category);
        Assert.Empty(results.Category);
        Assert.Null(results.PriceMin);
        Assert.Null(results.PriceMax);
        Assert.Equal(0, results.InStockTrueCount);
        Assert.Equal(0, results.InStockFalseCount);
    }
}
