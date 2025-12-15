using Facet.Search;

namespace Facet.Search.Tests.Models;

/// <summary>
/// Test model for full-text search behaviors.
/// </summary>
[FacetedSearch]
public class TestArticle
{
    public int Id { get; set; }

    [FullTextSearch(Behavior = TextSearchBehavior.Contains)]
    public string Title { get; set; } = null!;

    [FullTextSearch(Behavior = TextSearchBehavior.StartsWith)]
    public string? Slug { get; set; }

    [FullTextSearch(Behavior = TextSearchBehavior.Exact, CaseSensitive = true)]
    public string? Code { get; set; }

    [SearchFacet(Type = FacetType.Categorical)]
    public string Author { get; set; } = null!;
}

/// <summary>
/// Test model with hierarchical facet.
/// </summary>
[FacetedSearch]
public class TestCategory
{
    public int Id { get; set; }

    [SearchFacet(Type = FacetType.Hierarchical, DisplayName = "Category Path", IsHierarchical = true)]
    public string Path { get; set; } = null!;

    [SearchFacet(Type = FacetType.Categorical, DependsOn = "Path")]
    public string? SubCategory { get; set; }
}

/// <summary>
/// Test model with range aggregation settings.
/// </summary>
[FacetedSearch]
public class TestItem
{
    public int Id { get; set; }

    [SearchFacet(
        Type = FacetType.Range,
        DisplayName = "Weight",
        RangeAggregation = RangeAggregation.Auto,
        RangeIntervals = "0-1,1-5,5-10,10+"
    )]
    public decimal Weight { get; set; }

    [SearchFacet(
        Type = FacetType.Categorical,
        OrderBy = FacetOrder.Count,
        Limit = 5
    )]
    public string Tag { get; set; } = null!;
}
