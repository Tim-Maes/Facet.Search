namespace Facet.Search;

/// <summary>
/// Defines how full-text search is executed.
/// </summary>
public enum FullTextSearchStrategy
{
    /// <summary>
    /// Uses LINQ Contains() which translates to SQL LIKE '%term%'.
    /// Works with all databases but doesn't use full-text indexes.
    /// </summary>
    LinqContains,

    /// <summary>
    /// Uses EF.Functions.Like() for SQL LIKE pattern matching.
    /// Same as LinqContains but more explicit.
    /// </summary>
    EfLike,

    /// <summary>
    /// Uses EF.Functions.FreeText() for SQL Server full-text search.
    /// Requires a FULLTEXT index on the column.
    /// </summary>
    SqlServerFreeText,

    /// <summary>
    /// Uses EF.Functions.Contains() for SQL Server full-text search with CONTAINS predicate.
    /// Requires a FULLTEXT index on the column.
    /// </summary>
    SqlServerContains,

    /// <summary>
    /// Uses PostgreSQL full-text search with to_tsvector/to_tsquery.
    /// Requires appropriate indexes.
    /// </summary>
    PostgreSqlFullText,

    /// <summary>
    /// Evaluates the search in-memory after fetching data.
    /// Use with caution on large datasets.
    /// </summary>
    ClientSide
}
