using Facet.Search.Tests.Models;
using Facet.Search.Tests.Models.Search;
using Facet.Search.Tests.Utilities;

namespace Facet.Search.Tests;

/// <summary>
/// Tests for generated full-text search functionality using actual generated code.
/// </summary>
public class FullTextSearchRuntimeTests
{
    [Fact]
    public void FullTextSearch_WithTitleMatch_FindsResults()
    {
        // Arrange
        var articles = TestDataFactory.CreateArticleList().AsQueryable();
        var filter = new TestArticleSearchFilter
        {
            SearchText = "python"
        };

        // Act
        var results = articles.ApplyFacetedSearch(filter).ToList();

        // Assert
        Assert.Single(results);
        Assert.Contains("Python", results[0].Title, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FullTextSearch_CaseInsensitive_MatchesRegardlessOfCase()
    {
        // Arrange
        var articles = TestDataFactory.CreateArticleList().AsQueryable();
        var filter = new TestArticleSearchFilter
        {
            SearchText = "JAVASCRIPT"
        };

        // Act
        var results = articles.ApplyFacetedSearch(filter).ToList();

        // Assert
        Assert.Single(results);
        Assert.Contains("JavaScript", results[0].Title);
    }

    [Fact]
    public void FullTextSearch_WithNoMatch_ReturnsEmpty()
    {
        // Arrange
        var articles = TestDataFactory.CreateArticleList().AsQueryable();
        var filter = new TestArticleSearchFilter
        {
            SearchText = "nonexistent"
        };

        // Act
        var results = articles.ApplyFacetedSearch(filter).ToList();

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void FullTextSearch_WithEmptySearchText_ReturnsAllItems()
    {
        // Arrange
        var articles = TestDataFactory.CreateArticleList().AsQueryable();
        var filter = new TestArticleSearchFilter
        {
            SearchText = ""
        };

        // Act
        var results = articles.ApplyFacetedSearch(filter).ToList();

        // Assert
        Assert.Equal(5, results.Count);
    }

    [Fact]
    public void FullTextSearch_WithNullSearchText_ReturnsAllItems()
    {
        // Arrange
        var articles = TestDataFactory.CreateArticleList().AsQueryable();
        var filter = new TestArticleSearchFilter
        {
            SearchText = null
        };

        // Act
        var results = articles.ApplyFacetedSearch(filter).ToList();

        // Assert
        Assert.Equal(5, results.Count);
    }

    [Fact]
    public void FullTextSearch_CombinedWithFacetFilter_AppliesBoth()
    {
        // Arrange
        var articles = TestDataFactory.CreateArticleList().AsQueryable();
        var filter = new TestArticleSearchFilter
        {
            Author = ["John"],
            SearchText = "c#"
        };

        // Act
        var results = articles.ApplyFacetedSearch(filter).ToList();

        // Assert
        Assert.All(results, a => Assert.Equal("John", a.Author));
        Assert.All(results, a => Assert.Contains("c#", a.Title.ToLower()));
    }

    [Fact]
    public void FullTextSearch_StartsWithBehavior_MatchesSlugStart()
    {
        // Arrange - Slug uses StartsWith behavior
        var articles = TestDataFactory.CreateArticleList().AsQueryable();
        var filter = new TestArticleSearchFilter
        {
            SearchText = "getting"  // Matches slug "getting-started-csharp"
        };

        // Act
        var results = articles.ApplyFacetedSearch(filter).ToList();

        // Assert - Should match via title (Contains) or slug (StartsWith)
        Assert.True(results.Count >= 1);
    }

    [Fact]
    public void FullTextSearch_MatchesMultipleFields()
    {
        // Arrange
        var articles = TestDataFactory.CreateArticleList().AsQueryable();
        var filter = new TestArticleSearchFilter
        {
            SearchText = "intro"  // Matches slug "intro-fsharp" (StartsWith)
        };

        // Act
        var results = articles.ApplyFacetedSearch(filter).ToList();

        // Assert
        Assert.True(results.Count >= 1);
    }

    [Fact]
    public void FullTextSearch_WithPartialMatch_FindsResults()
    {
        // Arrange
        var articles = TestDataFactory.CreateArticleList().AsQueryable();
        var filter = new TestArticleSearchFilter
        {
            SearchText = "begin"  // Contains match in "Python for Beginners"
        };

        // Act
        var results = articles.ApplyFacetedSearch(filter).ToList();

        // Assert
        Assert.Single(results);
        Assert.Contains("Beginners", results[0].Title);
    }
}
