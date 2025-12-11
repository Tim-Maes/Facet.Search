using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Facet.Search.EFCore;

/// <summary>
/// EF Core-specific extensions for faceted search.
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Executes the query and returns results as a list asynchronously.
    /// This is just a convenience wrapper around EF Core's ToListAsync.
    /// </summary>
    public static Task<List<T>> ExecuteSearchAsync<T>(
        this IQueryable<T> query,
        CancellationToken cancellationToken = default)
    {
        return query.ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets the total count of results matching the current filters.
    /// </summary>
    public static Task<int> CountSearchResultsAsync<T>(
        this IQueryable<T> query,
        CancellationToken cancellationToken = default)
    {
        return query.CountAsync(cancellationToken);
    }

    /// <summary>
    /// Executes the query with pagination and returns a paged result.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="query">The queryable to paginate.</param>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paged result containing items and pagination metadata.</returns>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    /// <summary>
    /// Checks if any results exist matching the current filters.
    /// </summary>
    public static Task<bool> HasResultsAsync<T>(
        this IQueryable<T> query,
        CancellationToken cancellationToken = default)
    {
        return query.AnyAsync(cancellationToken);
    }

    /// <summary>
    /// Gets the first result or null if no results match.
    /// </summary>
    public static Task<T?> FirstOrDefaultSearchResultAsync<T>(
        this IQueryable<T> query,
        CancellationToken cancellationToken = default)
        where T : class
    {
        return query.FirstOrDefaultAsync(cancellationToken);
    }
}

/// <summary>
/// Represents a paginated result set.
/// </summary>
/// <typeparam name="T">The item type.</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// Gets or sets the items for the current page.
    /// </summary>
    public List<T> Items { get; set; } = null!;

    /// <summary>
    /// Gets or sets the current page number.
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Gets or sets the size of the page (number of items per page).
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Gets or sets the total count of items across all pages.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Gets the total number of pages.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// Determines if there are any more pages after the current one.
    /// </summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>
    /// Determines if there are any pages before the current one.
    /// </summary>
    public bool HasPreviousPage => Page > 1;
}
