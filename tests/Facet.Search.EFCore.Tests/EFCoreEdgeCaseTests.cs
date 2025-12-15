using Facet.Search.EFCore.Tests.Fixtures;
using Facet.Search.EFCore.Tests.Models.Search;
using Facet.Search.EFCore;

namespace Facet.Search.EFCore.Tests;

/// <summary>
/// Tests for edge cases and error handling.
/// </summary>
[Collection("Database")]
public class EFCoreEdgeCaseTests
{
    private readonly DatabaseFixture _fixture;

    public EFCoreEdgeCaseTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ExecuteSearchAsync_WithEmptyBrandArray_ReturnsAllProducts()
    {
        // Arrange
        var filter = new ProductSearchFilter
        {
            Brand = [] // Empty array
        };

        // Act
        var results = await _fixture.Context.Products
            .ApplyFacetedSearch(filter)
            .ExecuteSearchAsync();

        // Assert - Empty array should not filter
        Assert.Equal(10, results.Count);
    }

    [Fact]
    public async Task ExecuteSearchAsync_WithNullBrandArray_ReturnsAllProducts()
    {
        // Arrange
        var filter = new ProductSearchFilter
        {
            Brand = null
        };

        // Act
        var results = await _fixture.Context.Products
            .ApplyFacetedSearch(filter)
            .ExecuteSearchAsync();

        // Assert
        Assert.Equal(10, results.Count);
    }

    [Fact]
    public async Task ExecuteSearchAsync_WithZeroPriceRange_ReturnsNoProducts()
    {
        // Arrange
        var filter = new ProductSearchFilter
        {
            MinPrice = 100m,
            MaxPrice = 0m // Max less than min
        };

        // Act
        var results = await _fixture.Context.Products
            .ApplyFacetedSearch(filter)
            .ExecuteSearchAsync();

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public async Task ToPagedResultAsync_WithZeroPageSize_DefaultsToReasonableSize()
    {
        // Arrange
        var filter = new ProductSearchFilter();

        // Act
        var result = await _fixture.Context.Products
            .ApplyFacetedSearch(filter)
            .ToPagedResultAsync(page: 1, pageSize: 0);

        // Assert - Should default to something reasonable
        Assert.True(result.PageSize > 0);
    }

    [Fact]
    public async Task ToPagedResultAsync_WithNegativePage_DefaultsToFirstPage()
    {
        // Arrange
        var filter = new ProductSearchFilter();

        // Act
        var result = await _fixture.Context.Products
            .ApplyFacetedSearch(filter)
            .ToPagedResultAsync(page: -1, pageSize: 10);

        // Assert
        Assert.Equal(1, result.Page);
    }

    [Fact]
    public async Task ToPagedResultAsync_WithPageBeyondResults_ReturnsEmptyItems()
    {
        // Arrange
        var filter = new ProductSearchFilter();

        // Act
        var result = await _fixture.Context.Products
            .ApplyFacetedSearch(filter)
            .ToPagedResultAsync(page: 100, pageSize: 10);

        // Assert
        Assert.Empty(result.Items);
        Assert.Equal(10, result.TotalCount);
        Assert.False(result.HasNextPage);
    }

    [Fact]
    public async Task AggregateFacetAsync_WithNoData_ReturnsEmptyDictionary()
    {
        // Arrange
        var emptyQuery = _fixture.Context.Products.Where(p => p.Brand == "NonExistent");

        // Act
        var result = await emptyQuery.AggregateFacetAsync(p => p.Brand);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task CountBooleanAsync_WithNoData_ReturnsZeros()
    {
        // Arrange
        var emptyQuery = _fixture.Context.Products.Where(p => p.Brand == "NonExistent");

        // Act
        var (trueCount, falseCount) = await emptyQuery.CountBooleanAsync(p => p.InStock);

        // Assert
        Assert.Equal(0, trueCount);
        Assert.Equal(0, falseCount);
    }

    [Fact]
    public async Task ExecuteSearchAsync_WithWhitespaceSearchText_TreatsAsEmpty()
    {
        // Arrange
        var filter = new ProductSearchFilter
        {
            SearchText = "   " // Whitespace only
        };

        // Act
        var results = await _fixture.Context.Products
            .ApplyFacetedSearch(filter)
            .ExecuteSearchAsync();

        // Assert - Should return all products (whitespace treated as no search)
        Assert.Equal(10, results.Count);
    }

    [Fact]
    public async Task ExecuteSearchAsync_WithSpecialCharacters_HandlesCorrectly()
    {
        // Arrange
        var filter = new ProductSearchFilter
        {
            SearchText = "%" // SQL wildcard character
        };

        // Act - Should not throw and should safely handle
        var results = await _fixture.Context.Products
            .ApplyFacetedSearch(filter)
            .ExecuteSearchAsync();

        // Assert - May or may not find results, but shouldn't throw
        Assert.NotNull(results);
    }

    [Fact]
    public async Task FilterByDateRange_WithFromOnly_FiltersCorrectly()
    {
        // Arrange
        var filter = new ProductSearchFilter
        {
            CreatedAtFrom = DateTime.Now.AddDays(-7),
            CreatedAtTo = null
        };

        // Act
        var results = await _fixture.Context.Products
            .ApplyFacetedSearch(filter)
            .ExecuteSearchAsync();

        // Assert
        Assert.All(results, p => Assert.True(p.CreatedAt >= filter.CreatedAtFrom));
    }

    [Fact]
    public async Task FilterByDateRange_WithToOnly_FiltersCorrectly()
    {
        // Arrange
        var filter = new ProductSearchFilter
        {
            CreatedAtFrom = null,
            CreatedAtTo = DateTime.Now.AddDays(-30)
        };

        // Act
        var results = await _fixture.Context.Products
            .ApplyFacetedSearch(filter)
            .ExecuteSearchAsync();

        // Assert
        Assert.All(results, p => Assert.True(p.CreatedAt <= filter.CreatedAtTo));
    }
}
