using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Facet.Search.EFCore.Extensions;

/// <summary>
/// Extensions for pagination and sorting of search results.
/// </summary>
public static class PaginationExtensions
{
    /// <summary>
    /// Applies pagination to a query.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="query">The queryable to paginate.</param>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>The paginated query.</returns>
    public static IQueryable<T> Paginate<T>(
        this IQueryable<T> query,
        int page,
        int pageSize)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;

        return query
            .Skip((page - 1) * pageSize)
            .Take(pageSize);
    }

    /// <summary>
    /// Applies sorting to a query.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <typeparam name="TKey">The sort key type.</typeparam>
    /// <param name="query">The queryable to sort.</param>
    /// <param name="keySelector">Expression to select the sort key.</param>
    /// <param name="descending">Whether to sort in descending order.</param>
    /// <returns>The sorted query.</returns>
    public static IOrderedQueryable<T> SortBy<T, TKey>(
        this IQueryable<T> query,
        Expression<Func<T, TKey>> keySelector,
        bool descending = false)
    {
        return descending
            ? query.OrderByDescending(keySelector)
            : query.OrderBy(keySelector);
    }

    /// <summary>
    /// Applies secondary sorting to an already sorted query.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <typeparam name="TKey">The sort key type.</typeparam>
    /// <param name="query">The ordered queryable.</param>
    /// <param name="keySelector">Expression to select the secondary sort key.</param>
    /// <param name="descending">Whether to sort in descending order.</param>
    /// <returns>The sorted query.</returns>
    public static IOrderedQueryable<T> ThenSortBy<T, TKey>(
        this IOrderedQueryable<T> query,
        Expression<Func<T, TKey>> keySelector,
        bool descending = false)
    {
        return descending
            ? query.ThenByDescending(keySelector)
            : query.ThenBy(keySelector);
    }
}
