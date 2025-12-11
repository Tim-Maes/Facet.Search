using System;

namespace Facet.Search;

/// <summary>
/// Marks a property as a searchable facet.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class SearchFacetAttribute : Attribute
{
    /// <summary>
    /// The type of facet (Categorical, Range, Boolean, Date, etc.)
    /// </summary>
    public FacetType Type { get; set; } = FacetType.Categorical;

    /// <summary>
    /// Display name for the facet in UI/metadata.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Order in which facet results should be returned.
    /// </summary>
    public FacetOrder OrderBy { get; set; } = FacetOrder.Count;

    /// <summary>
    /// Maximum number of facet values to return in aggregations.
    /// </summary>
    public int Limit { get; set; } = 0; // 0 = no limit

    /// <summary>
    /// Property name this facet depends on. Only applicable when the dependent property has a value.
    /// </summary>
    public string? DependsOn { get; set; }

    /// <summary>
    /// Whether this facet is hierarchical (e.g., Category > Subcategory).
    /// </summary>
    public bool IsHierarchical { get; set; }

    /// <summary>
    /// For range facets, how to aggregate (Auto, Custom intervals, etc.)
    /// </summary>
    public RangeAggregation RangeAggregation { get; set; } = RangeAggregation.Auto;

    /// <summary>
    /// Custom range intervals for range facets (e.g., "0-50,50-100,100-500,500+")
    /// </summary>
    public string? RangeIntervals { get; set; }
}
