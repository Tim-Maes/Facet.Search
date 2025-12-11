using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Facet.Search.EFCore;

/// <summary>
/// EF Core extensions for executing facet aggregations efficiently.
/// </summary>
public static class FacetAggregationExtensions
{
    /// <summary>
    /// Executes a categorical facet aggregation and returns value -> count dictionary.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <typeparam name="TKey">The facet key type.</typeparam>
    /// <param name="query">The queryable to aggregate.</param>
    /// <param name="keySelector">Expression to select the facet key.</param>
    /// <param name="limit">Maximum number of facet values to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Dictionary of facet values to counts.</returns>
    public static async Task<Dictionary<TKey, int>> AggregateFacetAsync<T, TKey>(
        this IQueryable<T> query,
        Expression<Func<T, TKey>> keySelector,
        int? limit = null,
        CancellationToken cancellationToken = default)
        where TKey : notnull
    {
        var grouped = query
            .GroupBy(keySelector)
            .Select(g => new { Key = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count);

        if (limit.HasValue)
        {
            var results = await grouped.Take(limit.Value).ToListAsync(cancellationToken);
            return results.ToDictionary(x => x.Key, x => x.Count);
        }
        else
        {
            var results = await grouped.ToListAsync(cancellationToken);
            return results.ToDictionary(x => x.Key, x => x.Count);
        }
    }

    /// <summary>
    /// Gets the minimum value for a numeric property asynchronously.
    /// </summary>
    public static async Task<TResult?> GetMinAsync<T, TResult>(
        this IQueryable<T> query,
        Expression<Func<T, TResult>> selector,
        CancellationToken cancellationToken = default)
        where TResult : struct
    {
        if (!await query.AnyAsync(cancellationToken))
            return null;

        return await query.MinAsync(selector, cancellationToken);
    }

    /// <summary>
    /// Gets the maximum value for a numeric property asynchronously.
    /// </summary>
    public static async Task<TResult?> GetMaxAsync<T, TResult>(
        this IQueryable<T> query,
        Expression<Func<T, TResult>> selector,
        CancellationToken cancellationToken = default)
        where TResult : struct
    {
        if (!await query.AnyAsync(cancellationToken))
            return null;

        return await query.MaxAsync(selector, cancellationToken);
    }

    /// <summary>
    /// Gets min and max values for a numeric property asynchronously.
    /// </summary>
    public static async Task<(TResult? Min, TResult? Max)> GetRangeAsync<T, TResult>(
        this IQueryable<T> query,
        Expression<Func<T, TResult>> selector,
        CancellationToken cancellationToken = default)
        where TResult : struct
    {
        if (!await query.AnyAsync(cancellationToken))
            return (null, null);

        var min = await query.MinAsync(selector, cancellationToken);
        var max = await query.MaxAsync(selector, cancellationToken);
        return (min, max);
    }

    /// <summary>
    /// Counts true and false values for a boolean property asynchronously.
    /// </summary>
    public static async Task<(int TrueCount, int FalseCount)> CountBooleanAsync<T>(
        this IQueryable<T> query,
        Expression<Func<T, bool>> selector,
        CancellationToken cancellationToken = default)
    {
        // Compile the expression to use in the Where clause
        var parameter = selector.Parameters[0];
        var notExpression = Expression.Not(selector.Body);
        var notSelector = Expression.Lambda<Func<T, bool>>(notExpression, parameter);

        var trueCount = await query.CountAsync(selector, cancellationToken);
        var falseCount = await query.CountAsync(notSelector, cancellationToken);

        return (trueCount, falseCount);
    }
}
