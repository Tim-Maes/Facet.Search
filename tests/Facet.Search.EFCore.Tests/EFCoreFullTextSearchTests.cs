using Facet.Search.EFCore.Tests.Fixtures;
using Facet.Search.EFCore.Tests.Models.Search;
using Microsoft.EntityFrameworkCore;

namespace Facet.Search.EFCore.Tests;

[Collection("Database")]
public class EFCoreFullTextSearchTests
{
    private readonly DatabaseFixture _fixture;

    public EFCoreFullTextSearchTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ApplyLikeSearch_WithSingleProperty_FindsResults()
    {
        // Arrange
        var query = _fixture.Context.Products.AsQueryable();

        // Act
        var results = await query
            .ApplyLikeSearch("laptop", x => x.Name)
            .ToListAsync();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.All(results, p => Assert.Contains("Laptop", p.Name, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ApplyLikeSearch_WithMultipleProperties_FindsResultsFromAny()
    {
        // Arrange
        var query = _fixture.Context.Products.AsQueryable();

        // Act - Search for "ergonomic" which is in Description only
        var results = await query
            .ApplyLikeSearch("ergonomic", x => x.Name, x => x.Description)
            .ToListAsync();

        // Assert
        Assert.Single(results);
        Assert.Contains("ergonomic", results[0].Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ApplyLikeSearch_CaseInsensitive_MatchesRegardlessOfCase()
    {
        // Arrange
        var query = _fixture.Context.Products.AsQueryable();

        // Act
        var results = await query
            .ApplyLikeSearch("LAPTOP", x => x.Name)
            .ToListAsync();

        // Assert
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task ApplyLikeSearch_WithNoMatch_ReturnsEmpty()
    {
        // Arrange
        var query = _fixture.Context.Products.AsQueryable();

        // Act
        var results = await query
            .ApplyLikeSearch("nonexistent", x => x.Name)
            .ToListAsync();

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public async Task ApplyLikeSearch_WithEmptySearchTerm_ReturnsAllItems()
    {
        // Arrange
        var query = _fixture.Context.Products.AsQueryable();
        var totalCount = await query.CountAsync();

        // Act
        var results = await query
            .ApplyLikeSearch("", x => x.Name)
            .ToListAsync();

        // Assert
        Assert.Equal(totalCount, results.Count);
    }

    [Fact]
    public async Task ApplyLikeSearch_WithNullSearchTerm_ReturnsAllItems()
    {
        // Arrange
        var query = _fixture.Context.Products.AsQueryable();
        var totalCount = await query.CountAsync();

        // Act
        var results = await query
            .ApplyLikeSearch(null!, x => x.Name)
            .ToListAsync();

        // Assert
        Assert.Equal(totalCount, results.Count);
    }

    [Fact]
    public async Task ApplyLikeSearch_WithPartialMatch_FindsResults()
    {
        // Arrange
        var query = _fixture.Context.Products.AsQueryable();

        // Act - "Pro" should match "Laptop Pro 15"
        var results = await query
            .ApplyLikeSearch("Pro", x => x.Name)
            .ToListAsync();

        // Assert
        Assert.Single(results);
        Assert.Contains("Pro", results[0].Name);
    }

    [Fact]
    public async Task ApplyFullTextSearch_WithLinqContainsStrategy_WorksLikeLikeSearch()
    {
        // Arrange
        var query = _fixture.Context.Products.AsQueryable();

        // Act
        var results = await query
            .ApplyFullTextSearch("laptop", FullTextSearchStrategy.LinqContains, x => x.Name)
            .ToListAsync();

        // Assert
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task ApplyFullTextSearch_WithEfLikeStrategy_WorksLikeLikeSearch()
    {
        // Arrange
        var query = _fixture.Context.Products.AsQueryable();

        // Act
        var results = await query
            .ApplyFullTextSearch("laptop", FullTextSearchStrategy.EfLike, x => x.Name)
            .ToListAsync();

        // Assert
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task ApplyFullTextSearch_SqlServerFreeText_FallsBackToLike()
    {
        // Arrange - SQLite doesn't support FREETEXT, should fallback
        var query = _fixture.Context.Products.AsQueryable();

        // Act - Should fallback to LIKE search since we're using SQLite
        var results = await query
            .ApplyFullTextSearch("laptop", FullTextSearchStrategy.SqlServerFreeText, x => x.Name)
            .ToListAsync();

        // Assert - Should still work via fallback
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task ApplyFullTextSearch_SqlServerContains_FallsBackToLike()
    {
        // Arrange - SQLite doesn't support CONTAINS, should fallback
        var query = _fixture.Context.Products.AsQueryable();

        // Act - Should fallback to LIKE search since we're using SQLite
        var results = await query
            .ApplyFullTextSearch("laptop", FullTextSearchStrategy.SqlServerContains, x => x.Name)
            .ToListAsync();

        // Assert - Should still work via fallback
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task ApplyFullTextSearch_PostgreSqlFullText_FallsBackToLike()
    {
        // Arrange - SQLite doesn't support ILike, should fallback
        var query = _fixture.Context.Products.AsQueryable();

        // Act - Should fallback to LIKE search since we're using SQLite
        var results = await query
            .ApplyFullTextSearch("laptop", FullTextSearchStrategy.PostgreSqlFullText, x => x.Name)
            .ToListAsync();

        // Assert - Should still work via fallback
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public void ApplyClientSideSearch_WithMatch_FindsResults()
    {
        // Arrange
        var query = _fixture.Context.Products.AsQueryable();

        // Act - ClientSide loads data into memory
        var results = query
            .ApplyClientSideSearch("laptop", x => x.Name)
            .ToList();

        // Assert
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public void ApplyClientSideSearch_WithMultipleProperties_FindsResultsFromAny()
    {
        // Arrange
        var query = _fixture.Context.Products.AsQueryable();

        // Act
        var results = query
            .ApplyClientSideSearch("ergonomic", x => x.Name, x => x.Description)
            .ToList();

        // Assert
        Assert.Single(results);
    }

    [Fact]
    public async Task ApplyLikeSearch_CombinedWithFiltering_WorksCorrectly()
    {
        // Arrange
        var filter = new ProductSearchFilter
        {
            Category = ["Electronics"]
        };

        // Act
        var results = await _fixture.Context.Products
            .ApplyFacetedSearch(filter)
            .ApplyLikeSearch("laptop", x => x.Name)
            .ToListAsync();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.All(results, p => Assert.Equal("Electronics", p.Category));
        Assert.All(results, p => Assert.Contains("Laptop", p.Name, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ApplyLikeSearch_WithSpecialCharacters_EscapesCorrectly()
    {
        // Arrange
        var query = _fixture.Context.Products.AsQueryable();

        // Act - Search with characters that need escaping in LIKE patterns
        var results = await query
            .ApplyLikeSearch("100%", x => x.Name)
            .ToListAsync();

        // Assert - Should not crash and should return empty (no match)
        Assert.Empty(results);
    }

    [Fact]
    public async Task GetFullTextPropertySelectors_ReturnsCorrectSelectors()
    {
        // Arrange
        var selectors = ProductSearchExtensions.GetFullTextPropertySelectors();

        // Assert
        Assert.Equal(2, selectors.Length); // Name and Description

        // Act - Use the selectors with ApplyLikeSearch
        var results = await _fixture.Context.Products
            .ApplyLikeSearch("laptop", selectors)
            .ToListAsync();

        // Assert
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task GeneratedFullTextSearch_SearchesAcrossMultipleFields()
    {
        // Arrange - Use generated ApplyFacetedSearch with SearchText
        var filter = new ProductSearchFilter
        {
            SearchText = "ergonomic" // This is in Description, not Name
        };

        // Act
        var results = await _fixture.Context.Products
            .ApplyFacetedSearch(filter)
            .ToListAsync();

        // Assert
        Assert.Single(results);
        Assert.Equal("Wireless Mouse", results[0].Name);
    }

    [Fact]
    public async Task GeneratedFullTextSearch_CaseInsensitive()
    {
        // Arrange
        var filter = new ProductSearchFilter
        {
            SearchText = "GAMING"
        };

        // Act
        var results = await _fixture.Context.Products
            .ApplyFacetedSearch(filter)
            .ToListAsync();

        // Assert
        Assert.True(results.Count >= 1);
        Assert.All(results, p =>
            Assert.True(
                p.Name.Contains("Gaming", StringComparison.OrdinalIgnoreCase) ||
                p.Description!.Contains("Gaming", StringComparison.OrdinalIgnoreCase)));
    }
}
