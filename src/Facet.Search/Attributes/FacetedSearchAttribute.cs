using System;

namespace Facet.Search;

/// <summary>
/// Marks a class as searchable, triggering generation of filter classes and search extensions.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class FacetedSearchAttribute : Attribute
{
    /// <summary>
    /// Name for the generated filter class. Defaults to {ClassName}SearchFilter.
    /// </summary>
    public string? FilterClassName { get; set; }

    /// <summary>
    /// Whether to generate facet aggregation methods.
    /// </summary>
    public bool GenerateAggregations { get; set; } = true;

    /// <summary>
    /// Whether to generate metadata for frontend consumption.
    /// </summary>
    public bool GenerateMetadata { get; set; } = true;

    /// <summary>
    /// Namespace for generated code. Defaults to source class namespace + ".Search".
    /// </summary>
    public string? Namespace { get; set; }
}
