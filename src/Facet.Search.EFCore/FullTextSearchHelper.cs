using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Facet.Search.EFCore;

/// <summary>
/// Runtime helper for full-text search that is called by generated code.
/// Provides unified search across different database providers.
/// </summary>
public static class FullTextSearchHelper
{
    /// <summary>
    /// Applies full-text search using the specified strategy.
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <param name="query">The queryable to search.</param>
    /// <param name="searchTerm">The search term.</param>
    /// <param name="strategy">The full-text search strategy to use.</param>
    /// <param name="propertySelectors">Properties to search.</param>
    /// <returns>Filtered queryable.</returns>
    public static IQueryable<T> ApplyFullTextSearch<T>(
        this IQueryable<T> query,
        string searchTerm,
        FullTextSearchStrategy strategy,
        params Expression<Func<T, string?>>[] propertySelectors)
        where T : class
    {
        if (string.IsNullOrWhiteSpace(searchTerm) || propertySelectors.Length == 0)
            return query;

        return strategy switch
        {
            FullTextSearchStrategy.SqlServerFreeText => ApplySqlServerFreeText(query, searchTerm, propertySelectors),
            FullTextSearchStrategy.SqlServerContains => ApplySqlServerContains(query, searchTerm, propertySelectors),
            FullTextSearchStrategy.PostgreSqlFullText => ApplyPostgreSqlFullText(query, searchTerm, propertySelectors),
            FullTextSearchStrategy.EfLike => ApplyLikeSearch(query, searchTerm, propertySelectors),
            FullTextSearchStrategy.ClientSide => ApplyClientSideSearch(query, searchTerm, propertySelectors),
            _ => ApplyLikeSearch(query, searchTerm, propertySelectors)
        };
    }

    /// <summary>
    /// Applies full-text search using SQL Server FREETEXT function.
    /// Falls back to LIKE if FREETEXT is not available.
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <param name="query">The queryable to search.</param>
    /// <param name="searchTerm">The search term.</param>
    /// <param name="propertySelectors">Properties to search.</param>
    /// <returns>Filtered queryable.</returns>
    public static IQueryable<T> ApplySqlServerFreeText<T>(
        this IQueryable<T> query,
        string searchTerm,
        params Expression<Func<T, string?>>[] propertySelectors)
        where T : class
    {
        if (string.IsNullOrWhiteSpace(searchTerm) || propertySelectors.Length == 0)
            return query;

        var freeTextMethod = GetFreeTextMethod();
        if (freeTextMethod != null)
        {
            return ApplyDbFunctionSearch(query, searchTerm, propertySelectors, freeTextMethod);
        }

        // Fallback to LIKE search
        return ApplyLikeSearch(query, searchTerm, propertySelectors);
    }

    /// <summary>
    /// Applies full-text search using SQL Server CONTAINS function.
    /// Falls back to LIKE if CONTAINS is not available.
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <param name="query">The queryable to search.</param>
    /// <param name="searchTerm">The search term.</param>
    /// <param name="propertySelectors">Properties to search.</param>
    /// <returns>Filtered queryable.</returns>
    public static IQueryable<T> ApplySqlServerContains<T>(
        this IQueryable<T> query,
        string searchTerm,
        params Expression<Func<T, string?>>[] propertySelectors)
        where T : class
    {
        if (string.IsNullOrWhiteSpace(searchTerm) || propertySelectors.Length == 0)
            return query;

        var containsMethod = GetContainsMethod();
        if (containsMethod != null)
        {
            // Wrap in quotes for CONTAINS syntax if not already
            var containsSearchTerm = searchTerm.Contains("\"") ? searchTerm : $"\"{searchTerm}\"";
            return ApplyDbFunctionSearch(query, containsSearchTerm, propertySelectors, containsMethod);
        }

        // Fallback to LIKE search
        return ApplyLikeSearch(query, searchTerm, propertySelectors);
    }

    /// <summary>
    /// Applies full-text search using PostgreSQL ILike (case-insensitive LIKE).
    /// Falls back to standard LIKE if ILike is not available.
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <param name="query">The queryable to search.</param>
    /// <param name="searchTerm">The search term.</param>
    /// <param name="propertySelectors">Properties to search.</param>
    /// <returns>Filtered queryable.</returns>
    public static IQueryable<T> ApplyPostgreSqlFullText<T>(
        this IQueryable<T> query,
        string searchTerm,
        params Expression<Func<T, string?>>[] propertySelectors)
        where T : class
    {
        if (string.IsNullOrWhiteSpace(searchTerm) || propertySelectors.Length == 0)
            return query;

        var iLikeMethod = GetILikeMethod();
        if (iLikeMethod != null)
        {
            var pattern = $"%{EscapePostgreSqlLikePattern(searchTerm)}%";
            return ApplyDbFunctionSearch(query, pattern, propertySelectors, iLikeMethod);
        }

        // Fallback to standard LIKE search
        return ApplyLikeSearch(query, searchTerm, propertySelectors);
    }

    /// <summary>
    /// Applies EF.Functions.Like search.
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <param name="query">The queryable to search.</param>
    /// <param name="searchTerm">The search term.</param>
    /// <param name="propertySelectors">Properties to search.</param>
    /// <returns>Filtered queryable.</returns>
    public static IQueryable<T> ApplyLikeSearch<T>(
        this IQueryable<T> query,
        string searchTerm,
        params Expression<Func<T, string?>>[] propertySelectors)
        where T : class
    {
        if (string.IsNullOrWhiteSpace(searchTerm) || propertySelectors.Length == 0)
            return query;

        var pattern = $"%{EscapeSqlServerLikePattern(searchTerm)}%";
        var parameter = Expression.Parameter(typeof(T), "x");
        Expression? combined = null;

        var efFunctions = Expression.Property(null, typeof(EF), "Functions");
        var likeMethod = typeof(DbFunctionsExtensions).GetMethod(
            nameof(DbFunctionsExtensions.Like),
            new[] { typeof(DbFunctions), typeof(string), typeof(string) });

        if (likeMethod == null)
        {
            // Ultimate fallback to string.Contains
            return ApplyStringContainsSearch(query, searchTerm, propertySelectors);
        }

        foreach (var selector in propertySelectors)
        {
            var property = ReplaceParameter(selector.Body, selector.Parameters[0], parameter);
            var call = Expression.Call(likeMethod, efFunctions, property, Expression.Constant(pattern));

            combined = combined == null
                ? call
                : Expression.OrElse(combined, call);
        }

        if (combined == null)
            return query;

        var lambda = Expression.Lambda<Func<T, bool>>(combined, parameter);
        return query.Where(lambda);
    }

    /// <summary>
    /// Applies client-side search (loads data into memory).
    /// Warning: Can be slow for large datasets.
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <param name="query">The queryable to search.</param>
    /// <param name="searchTerm">The search term.</param>
    /// <param name="propertySelectors">Properties to search.</param>
    /// <returns>Filtered queryable.</returns>
    public static IQueryable<T> ApplyClientSideSearch<T>(
        this IQueryable<T> query,
        string searchTerm,
        params Expression<Func<T, string?>>[] propertySelectors)
        where T : class
    {
        if (string.IsNullOrWhiteSpace(searchTerm) || propertySelectors.Length == 0)
            return query;

        var compiledSelectors = propertySelectors
            .Select(s => s.Compile())
            .ToList();

        var lowerSearchTerm = searchTerm.ToLower();

        return query.AsEnumerable()
            .Where(item => compiledSelectors.Any(selector =>
                selector(item)?.ToLower().Contains(lowerSearchTerm) ?? false))
            .AsQueryable();
    }

    /// <summary>
    /// Applies standard string.Contains search (works everywhere but slower).
    /// </summary>
    private static IQueryable<T> ApplyStringContainsSearch<T>(
        IQueryable<T> query,
        string searchTerm,
        params Expression<Func<T, string?>>[] propertySelectors)
        where T : class
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        Expression? combined = null;
        var lowerSearchTerm = searchTerm.ToLower();

        foreach (var selector in propertySelectors)
        {
            var property = ReplaceParameter(selector.Body, selector.Parameters[0], parameter);
            
            // Build: property != null && property.ToLower().Contains(searchTerm)
            var notNull = Expression.NotEqual(property, Expression.Constant(null, typeof(string)));
            var toLower = Expression.Call(property, typeof(string).GetMethod("ToLower", Type.EmptyTypes)!);
            var contains = Expression.Call(toLower, typeof(string).GetMethod("Contains", new[] { typeof(string) })!, 
                Expression.Constant(lowerSearchTerm));
            var condition = Expression.AndAlso(notNull, contains);

            combined = combined == null
                ? condition
                : Expression.OrElse(combined, condition);
        }

        if (combined == null)
            return query;

        var lambda = Expression.Lambda<Func<T, bool>>(combined, parameter);
        return query.Where(lambda);
    }

    private static IQueryable<T> ApplyDbFunctionSearch<T>(
        IQueryable<T> query,
        string searchPattern,
        Expression<Func<T, string?>>[] propertySelectors,
        MethodInfo dbMethod)
        where T : class
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        Expression? combined = null;

        var efFunctions = Expression.Property(null, typeof(EF), "Functions");

        foreach (var selector in propertySelectors)
        {
            var property = ReplaceParameter(selector.Body, selector.Parameters[0], parameter);
            var call = Expression.Call(dbMethod, efFunctions, property, Expression.Constant(searchPattern));

            combined = combined == null
                ? call
                : Expression.OrElse(combined, call);
        }

        if (combined == null)
            return query;

        var lambda = Expression.Lambda<Func<T, bool>>(combined, parameter);
        return query.Where(lambda);
    }

    private static MethodInfo? GetFreeTextMethod()
    {
        // Try to find EF.Functions.FreeText from SQL Server provider
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.GetName().Name == "Microsoft.EntityFrameworkCore.SqlServer")
            {
                var extensionsType = assembly.GetType("Microsoft.EntityFrameworkCore.SqlServerDbFunctionsExtensions");
                if (extensionsType != null)
                {
                    return extensionsType.GetMethod("FreeText",
                        new[] { typeof(DbFunctions), typeof(string), typeof(string) });
                }
            }
        }
        return null;
    }

    private static MethodInfo? GetContainsMethod()
    {
        // Try to find EF.Functions.Contains from SQL Server provider
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.GetName().Name == "Microsoft.EntityFrameworkCore.SqlServer")
            {
                var extensionsType = assembly.GetType("Microsoft.EntityFrameworkCore.SqlServerDbFunctionsExtensions");
                if (extensionsType != null)
                {
                    return extensionsType.GetMethod("Contains",
                        new[] { typeof(DbFunctions), typeof(string), typeof(string) });
                }
            }
        }
        return null;
    }

    private static MethodInfo? GetILikeMethod()
    {
        // Try to find EF.Functions.ILike from Npgsql provider
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.GetName().Name == "Npgsql.EntityFrameworkCore.PostgreSQL")
            {
                var extensionsType = assembly.GetType("Microsoft.EntityFrameworkCore.NpgsqlDbFunctionsExtensions");
                if (extensionsType != null)
                {
                    return extensionsType.GetMethod("ILike",
                        new[] { typeof(DbFunctions), typeof(string), typeof(string) });
                }
            }
        }
        return null;
    }

    private static string EscapeSqlServerLikePattern(string pattern)
    {
        return pattern
            .Replace("[", "[[]")
            .Replace("%", "[%]")
            .Replace("_", "[_]");
    }

    private static string EscapePostgreSqlLikePattern(string pattern)
    {
        return pattern
            .Replace("\\", "\\\\")
            .Replace("%", "\\%")
            .Replace("_", "\\_");
    }

    private static Expression ReplaceParameter(Expression expression, ParameterExpression oldParam, ParameterExpression newParam)
    {
        return new ParameterReplacer(oldParam, newParam).Visit(expression);
    }

    private class ParameterReplacer : ExpressionVisitor
    {
        private readonly ParameterExpression _oldParam;
        private readonly ParameterExpression _newParam;

        public ParameterReplacer(ParameterExpression oldParam, ParameterExpression newParam)
        {
            _oldParam = oldParam;
            _newParam = newParam;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == _oldParam ? _newParam : base.VisitParameter(node);
        }
    }
}
