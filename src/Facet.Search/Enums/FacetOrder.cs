namespace Facet.Search;

/// <summary>
/// Defines how facet results should be ordered in aggregations.
/// </summary>
public enum FacetOrder
{
    /// <summary>
    /// Order by count (most common first)
    /// </summary>
    Count,

    /// <summary>
    /// Order alphabetically by value
    /// </summary>
    Value,

    /// <summary>
    /// Order by relevance score (for text search)
    /// </summary>
    Relevance,

    /// <summary>
    /// Custom ordering (requires additional configuration)
    /// </summary>
    Custom
}
