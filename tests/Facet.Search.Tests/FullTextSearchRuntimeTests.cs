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
    public void FullTextSearch_ExactBehavior_MatchesExactValue()
    {
        // Arrange - TestArticle has Code with Exact behavior
        var articles = TestDataFactory.CreateArticleList().AsQueryable();
        var filter = new TestArticleSearchFilter
        {
            SearchText = "cs001"  // Exact match for code (case-insensitive due to ToLower)
        };

        // Act
        var results = articles.ApplyFacetedSearch(filter).ToList();

        // Assert - Should match because Code has Exact behavior and "CS001" exists
        Assert.True(results.Count >= 1);
    }

    [Fact]
    public void FullTextSearch_StartsWithBehavior_MatchesPrefix()
    {
        // Arrange - TestArticle has Slug with StartsWith behavior
        var articles = TestDataFactory.CreateArticleList().AsQueryable();
        var filter = new TestArticleSearchFilter
        {
            SearchText = "getting"  // Should match "getting-started-csharp"
        };

        // Act
        var results = articles.ApplyFacetedSearch(filter).ToList();

        // Assert - Should find the article with slug starting with "getting"
        Assert.Contains(results, a => a.Slug?.StartsWith("getting", StringComparison.OrdinalIgnoreCase) ?? false);
    }

    [Fact]
    public void FullTextSearch_MultipleFields_ReturnsUnionOfMatches()
    {
        // Arrange
        var articles = TestDataFactory.CreateArticleList().AsQueryable();
        var filter = new TestArticleSearchFilter
        {
            SearchText = "c#"  // Should match in Title
        };

        // Act
        var results = articles.ApplyFacetedSearch(filter).ToList();

        // Assert - Should find articles with "C#" in title
        Assert.True(results.Count >= 1);
        Assert.All(results, a => 
            Assert.True(
                a.Title.Contains("C#", StringComparison.OrdinalIgnoreCase) ||
                (a.Slug?.Contains("c#", StringComparison.OrdinalIgnoreCase) ?? false) ||
                (a.Code?.ToLower() == "c#")));
    }

    [Fact]
    public void FullTextSearch_WithSpecialCharacters_HandlesCorrectly()
    {
        // Arrange
        var articles = TestDataFactory.CreateArticleList().AsQueryable();
        var filter = new TestArticleSearchFilter
        {
            SearchText = "f#"  // F# is a valid language name with special char
        };

        // Act
        var results = articles.ApplyFacetedSearch(filter).ToList();

        // Assert - Should find the F# article
        Assert.Contains(results, a => a.Title.Contains("F#", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void FullTextSearch_ContainsBehavior_MatchesAnywhere()
    {
        // Arrange - Title uses Contains behavior (default)
        var articles = TestDataFactory.CreateArticleList().AsQueryable();
        var filter = new TestArticleSearchFilter
        {
            SearchText = "Pattern"  // Should match "Advanced C# Patterns"
        };

        // Act
        var results = articles.ApplyFacetedSearch(filter).ToList();

        // Assert
        Assert.Single(results);
        Assert.Contains("Patterns", results[0].Title);
    }

    [Fact]
    public void FullTextSearch_NullPropertyValue_DoesNotThrow()
    {
        // Arrange - Create articles where some have null optional fields
        var articles = new List<TestArticle>
        {
            TestDataFactory.CreateArticle(1, "Test Article", null, null, "Author1"),  // null Slug and Code
            TestDataFactory.CreateArticle(2, "Another Article", "slug-value", "CODE", "Author2"),
        }.AsQueryable();

        var filter = new TestArticleSearchFilter
        {
            SearchText = "test"
        };

        // Act - Should not throw even with null values
        var results = articles.ApplyFacetedSearch(filter).ToList();

        // Assert
        Assert.Single(results);
        Assert.Equal("Test Article", results[0].Title);
    }

    [Fact]
    public void FullTextSearch_WhitespaceOnlySearchText_ReturnsAllItems()
    {
        // Arrange
        var articles = TestDataFactory.CreateArticleList().AsQueryable();
        var filter = new TestArticleSearchFilter
        {
            SearchText = "   \t\n  "  // Only whitespace
        };

        // Act
        var results = articles.ApplyFacetedSearch(filter).ToList();

        // Assert - Should return all items since search is effectively empty
        Assert.Equal(5, results.Count);
    }

    [Fact]
    public void FullTextSearch_SingleCharacterSearch_FindsResults()
    {
        // Arrange
        var articles = TestDataFactory.CreateArticleList().AsQueryable();
        var filter = new TestArticleSearchFilter
        {
            SearchText = "c"  // Single character
        };

        // Act
        var results = articles.ApplyFacetedSearch(filter).ToList();

        // Assert - Should find articles containing 'c'
        Assert.True(results.Count >= 1);
    }
}
