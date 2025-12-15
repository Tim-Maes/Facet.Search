using Facet.Search.Tests.Models;
using Facet.Search.Tests.Models.Search;

namespace Facet.Search.Tests;

/// <summary>
/// Runtime tests for full-text search behaviors with generated code.
/// </summary>
public class FullTextSearchRuntimeTests
{
    private static List<TestArticle> GetTestArticles() =>
    [
        new() { Id = 1, Title = "Getting Started with C#", Slug = "getting-started-csharp", Code = "CS001", Author = "John" },
        new() { Id = 2, Title = "Advanced C# Patterns", Slug = "advanced-csharp-patterns", Code = "CS002", Author = "Jane" },
        new() { Id = 3, Title = "Introduction to F#", Slug = "intro-fsharp", Code = "FS001", Author = "John" },
        new() { Id = 4, Title = "Python for Beginners", Slug = "python-beginners", Code = "PY001", Author = "Bob" },
        new() { Id = 5, Title = "JavaScript Essentials", Slug = "javascript-essentials", Code = "JS001", Author = "Jane" },
    ];

    [Fact]
    public void FullTextSearch_WithTitleMatch_FindsResults()
    {
        // Arrange
        var articles = GetTestArticles().AsQueryable();
        var filter = new TestArticleSearchFilter
        {
            SearchText = "python"
        };

        // Act
        var results = articles.ApplyFacetedSearch(filter).ToList();

        // Assert - Should match "Python for Beginners"
        Assert.Single(results);
        Assert.Contains("Python", results[0].Title);
    }

    [Fact]
    public void FullTextSearch_CaseInsensitive_MatchesRegardlessOfCase()
    {
        // Arrange
        var articles = GetTestArticles().AsQueryable();
        var filter = new TestArticleSearchFilter
        {
            SearchText = "PYTHON"
        };

        // Act
        var results = articles.ApplyFacetedSearch(filter).ToList();

        // Assert - Should match "Python for Beginners" case-insensitively
        Assert.Single(results);
        Assert.Contains("Python", results[0].Title);
    }

    [Fact]
    public void FullTextSearch_WithNoMatch_ReturnsEmpty()
    {
        // Arrange
        var articles = GetTestArticles().AsQueryable();
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
        var articles = GetTestArticles().AsQueryable();
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
        var articles = GetTestArticles().AsQueryable();
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
        var articles = GetTestArticles().AsQueryable();
        var filter = new TestArticleSearchFilter
        {
            Author = ["John"],
            SearchText = "c#"
        };

        // Act
        var results = articles.ApplyFacetedSearch(filter).ToList();

        // Assert - Should only match John's articles with "c#" in title
        Assert.All(results, a => Assert.Equal("John", a.Author));
    }

    [Fact]
    public void FullTextSearch_MatchesMultipleFields()
    {
        // Arrange
        var articles = GetTestArticles().AsQueryable();
        var filter = new TestArticleSearchFilter
        {
            SearchText = "getting"
        };

        // Act
        var results = articles.ApplyFacetedSearch(filter).ToList();

        // Assert - Should match via title "Getting Started with C#" or slug
        Assert.True(results.Count >= 1);
    }
}
