using Facet.Search.EFCore.Tests.Fixtures;
using Facet.Search.EFCore.Tests.Models.Search;
using Facet.Search.EFCore;

namespace Facet.Search.EFCore.Tests;

/// <summary>
/// Additional integration tests for EF Core sorting features.
/// </summary>
[Collection("Database")]
public class EFCoreSortingTests
{
    private readonly DatabaseFixture _fixture;

    public EFCoreSortingTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void SortBy_Ascending_SortsCorrectly()
    {
        // Arrange & Act
        var results = _fixture.Context.Products
            .SortBy(p => p.Price)
            .Take(5)
            .ToList();

        // Assert
        for (int i = 1; i < results.Count; i++)
        {
            Assert.True(results[i].Price >= results[i - 1].Price);
        }
    }

    [Fact]
    public void SortBy_Descending_SortsCorrectly()
    {
        // Arrange & Act
        var results = _fixture.Context.Products
            .SortBy(p => p.Price, descending: true)
            .Take(5)
            .ToList();

        // Assert
        for (int i = 1; i < results.Count; i++)
        {
            Assert.True(results[i].Price <= results[i - 1].Price);
        }
    }

    [Fact]
    public void SortBy_ThenSortBy_AppliesSecondarySort()
    {
        // Arrange & Act
        var results = _fixture.Context.Products
            .SortBy(p => p.Category)
            .ThenSortBy(p => p.Price)
            .ToList();

        // Assert - Items should be sorted by category, then by price within category
        var groups = results.GroupBy(p => p.Category);
        foreach (var group in groups)
        {
            var prices = group.Select(p => p.Price).ToList();
            for (int i = 1; i < prices.Count; i++)
            {
                Assert.True(prices[i] >= prices[i - 1]);
            }
        }
    }

    [Fact]
    public void SortBy_WithString_SortsAlphabetically()
    {
        // Arrange & Act
        var results = _fixture.Context.Products
            .SortBy(p => p.Name)
            .Take(5)
            .ToList();

        // Assert
        var sortedNames = results.Select(p => p.Name).ToList();
        var expectedOrder = sortedNames.OrderBy(n => n).ToList();
        Assert.Equal(expectedOrder, sortedNames);
    }

    [Fact]
    public void SortBy_CombinedWithFilter_WorksCorrectly()
    {
        // Arrange
        var filter = new ProductSearchFilter
        {
            InStock = true
        };

        // Act
        var results = _fixture.Context.Products
            .ApplyFacetedSearch(filter)
            .SortBy(p => p.Price, descending: true)
            .ToList();

        // Assert
        Assert.All(results, p => Assert.True(p.InStock));
        for (int i = 1; i < results.Count; i++)
        {
            Assert.True(results[i].Price <= results[i - 1].Price);
        }
    }

    [Fact]
    public async Task SortBy_WithPagination_MaintainsOrder()
    {
        // Arrange
        var filter = new ProductSearchFilter();

        // Act
        var page1 = await _fixture.Context.Products
            .ApplyFacetedSearch(filter)
            .SortBy(p => p.Price)
            .ToPagedResultAsync(page: 1, pageSize: 3);

        var page2 = await _fixture.Context.Products
            .ApplyFacetedSearch(filter)
            .SortBy(p => p.Price)
            .ToPagedResultAsync(page: 2, pageSize: 3);

        // Assert - Last item of page 1 should be <= first item of page 2
        Assert.True(page1.Items.Last().Price <= page2.Items.First().Price);
    }
}
