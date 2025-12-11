namespace Facet.Search;

/// <summary>
/// Defines the type of facet for search filtering.
/// </summary>
public enum FacetType
{
    /// <summary>
    /// Discrete values (e.g., Brand, Category, Status)
    /// Generates: string[]? PropertyName { get; set; }
    /// </summary>
    Categorical,

    /// <summary>
    /// Numeric or date range (e.g., Price, Date)
    /// Generates: Min/Max properties
    /// </summary>
    Range,

    /// <summary>
    /// Boolean flag (e.g., InStock, IsActive)
    /// Generates: bool? PropertyName { get; set; }
    /// </summary>
    Boolean,

    /// <summary>
    /// Date/DateTime with common presets (Today, Last 7 days, etc.)
    /// Generates: DateTime? From/To + DateRangePreset enum
    /// </summary>
    DateRange,

    /// <summary>
    /// Hierarchical categories (e.g., Electronics > Computers > Laptops)
    /// Generates: string[]? PropertyName { get; set; } with path support
    /// </summary>
    Hierarchical,

    /// <summary>
    /// Geographic location with distance filtering
    /// Generates: GeoPoint + Distance properties
    /// </summary>
    Geo
}
