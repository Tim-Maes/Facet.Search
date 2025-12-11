namespace Facet.Search;

/// <summary>
/// Defines full-text search behavior.
/// </summary>
public enum TextSearchBehavior
{
    /// <summary>
    /// Contains substring (default)
    /// </summary>
    Contains,

    /// <summary>
    /// Starts with prefix
    /// </summary>
    StartsWith,

    /// <summary>
    /// Ends with suffix
    /// </summary>
    EndsWith,

    /// <summary>
    /// Exact match
    /// </summary>
    Exact
}
