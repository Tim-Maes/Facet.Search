namespace Facet.Search;

/// <summary>
/// Defines how range facets should be aggregated.
/// </summary>
public enum RangeAggregation
{
    /// <summary>
    /// Automatically determine reasonable intervals
    /// </summary>
    Auto,

    /// <summary>
    /// Use custom intervals defined in attribute
    /// </summary>
    Custom,

    /// <summary>
    /// Fixed-size intervals (e.g., 0-10, 10-20, 20-30)
    /// </summary>
    Fixed,

    /// <summary>
    /// No aggregation, just min/max filtering
    /// </summary>
    None
}
