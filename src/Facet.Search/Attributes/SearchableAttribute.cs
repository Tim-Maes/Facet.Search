using System;

namespace Facet.Search;

/// <summary>
/// Marks a property as searchable but not a facet (e.g., sortable fields, included in results).
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class SearchableAttribute : Attribute
{
    /// <summary>
    /// Whether this property can be used for sorting.
    /// </summary>
    public bool Sortable { get; set; } = true;
}
