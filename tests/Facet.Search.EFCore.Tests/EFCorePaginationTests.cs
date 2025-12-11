using Facet.Search.EFCore.Extensions;
using Facet.Search.EFCore.Tests.Fixtures;
using Facet.Search.EFCore.Tests.Models;
using Facet.Search.EFCore.Tests.Models.Search;

namespace Facet.Search.EFCore.Tests;

/// <summary>
/// Integration tests for pagination with EF Core.
/// </summary>
[Collection("Database")]
public class EFCorePaginationTests
{
    private readonly DatabaseFixture _fixture;

    public EFCorePaginationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ToPagedResultAsync_ReturnsCorrectPage()
    {
        // Arrange
        var filter = new ProductSearchFilter();

        // Act
        var pagedResult = await _fixture.Context.Products
            .ApplyFacetedSearch(filter)
            .OrderBy(p => p.Id)
            .ToPagedResultAsync(page: 1, pageSize: 3);

        // Assert
        Assert.Equal(3, pagedResult.Items.Count);
        Assert.Equal(1, pagedResult.Page);
        Assert.Equal(3, pagedResult.PageSize);
        Assert.Equal(10, pagedResult.TotalCount);
        Assert.Equal(4, pagedResult.TotalPages);
        Assert.False(pagedResult.HasPreviousPage);
        Assert.True(pagedResult.HasNextPage);
    }

    [Fact]
    public async Task ToPagedResultAsync_ReturnsSecondPage_Correctly()
    {
        // Arrange
        var filter = new ProductSearchFilter();

        // Act
        var pagedResult = await _fixture.Context.Products
            .ApplyFacetedSearch(filter)
            .OrderBy(p => p.Id)
            .ToPagedResultAsync(page: 2, pageSize: 3);

        // Assert
        Assert.Equal(3, pagedResult.Items.Count);
        Assert.Equal(2, pagedResult.Page);
        Assert.True(pagedResult.HasPreviousPage);
        Assert.True(pagedResult.HasNextPage);
        
        // Should be products 4, 5, 6
        Assert.Equal(4, pagedResult.Items[0].Id);
        Assert.Equal(5, pagedResult.Items[1].Id);
        Assert.Equal(6, pagedResult.Items[2].Id);
    }

    [Fact]
    public async Task ToPagedResultAsync_LastPage_HasCorrectItems()
    {
        // Arrange
        var filter = new ProductSearchFilter();

        // Act
        var pagedResult = await _fixture.Context.Products
            .ApplyFacetedSearch(filter)
            .OrderBy(p => p.Id)
            .ToPagedResultAsync(page: 4, pageSize: 3);

        // Assert
        Assert.Single(pagedResult.Items); // Only 1 item on last page (10 total, 3 per page)
        Assert.Equal(4, pagedResult.Page);
        Assert.True(pagedResult.HasPreviousPage);
        Assert.False(pagedResult.HasNextPage);
    }

    [Fact]
    public async Task ToPagedResultAsync_WithFilter_PaginatesFilteredResults()
    {
        // Arrange
        var filter = new ProductSearchFilter
        {
            Brand = ["TechCorp"]
        };

        // Act
        var pagedResult = await _fixture.Context.Products
            .ApplyFacetedSearch(filter)
            .OrderBy(p => p.Id)
            .ToPagedResultAsync(page: 1, pageSize: 2);

        // Assert
        Assert.Equal(2, pagedResult.Items.Count);
        Assert.Equal(5, pagedResult.TotalCount); // 5 TechCorp products
        Assert.Equal(3, pagedResult.TotalPages);
    }

    [Fact]
    public void Paginate_AppliesSkipAndTake()
    {
        // Arrange & Act
        var results = _fixture.Context.Products
            .OrderBy(p => p.Id)
            .Paginate(page: 2, pageSize: 3)
            .ToList();

        // Assert
        Assert.Equal(3, results.Count);
        Assert.Equal(4, results[0].Id);
    }

    [Fact]
    public void SortBy_Ascending_SortsCorrectly()
    {
        // Arrange & Act
        var results = _fixture.Context.Products
            .SortBy(p => p.Price)
            .Take(3)
            .ToList();

        // Assert
        Assert.Equal(29.99m, results[0].Price); // USB Hub
        Assert.Equal(39.99m, results[1].Price); // Desk Lamp
        Assert.Equal(49.99m, results[2].Price); // Wireless Mouse
    }

    [Fact]
    public void SortBy_Descending_SortsCorrectly()
    {
        // Arrange & Act
        var results = _fixture.Context.Products
            .SortBy(p => p.Price, descending: true)
            .Take(3)
            .ToList();

        // Assert
        Assert.Equal(1999.99m, results[0].Price); // Gaming Laptop
        Assert.Equal(1299.99m, results[1].Price); // Laptop Pro
        Assert.Equal(599.99m, results[2].Price);  // Standing Desk
    }
}
