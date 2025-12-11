using System;

namespace Facet.Search;

/// <summary>
/// Marks a property for full-text search.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class FullTextSearchAttribute : Attribute
{
    /// <summary>
    /// Weight/boost for this field in search relevance (higher = more important).
    /// </summary>
    public float Weight { get; set; } = 1.0f;

    /// <summary>
    /// Whether to use case-sensitive matching.
    /// </summary>
    public bool CaseSensitive { get; set; }

    /// <summary>
    /// Search behavior: Contains, StartsWith, EndsWith, Exact
    /// </summary>
    public TextSearchBehavior Behavior { get; set; } = TextSearchBehavior.Contains;
}
