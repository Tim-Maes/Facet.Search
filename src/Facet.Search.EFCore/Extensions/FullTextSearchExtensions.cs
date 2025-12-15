using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Facet.Search.EFCore;

/// <summary>
/// Unified full-text search extensions that work with SQL Server, PostgreSQL, and LIKE fallback.
/// </summary>
public static class FullTextSearchExtensions
{
    /// <summary>
    /// Applies EF.Functions.Like() search which translates to SQL LIKE.
    /// Works with all databases.
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <param name="query">The queryable to search.</param>
    /// <param name="propertySelector">Property to search.</param>
    /// <param name="searchTerm">The search term (wildcards are added automatically).</param>
    /// <returns>Filtered queryable.</returns>
    public static IQueryable<T> LikeSearch<T>(
        this IQueryable<T> query,
        Expression<Func<T, string?>> propertySelector,
        string searchTerm)
        where T : class
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return query;

        var pattern = $"%{EscapeLikePattern(searchTerm)}%";

        var parameter = propertySelector.Parameters[0];
        var property = propertySelector.Body;

        var efFunctions = Expression.Property(null, typeof(EF), "Functions");
        var likeMethod = typeof(DbFunctionsExtensions).GetMethod(
            nameof(DbFunctionsExtensions.Like),
            new[] { typeof(DbFunctions), typeof(string), typeof(string) });

        if (likeMethod == null)
            throw new InvalidOperationException("EF.Functions.Like not available.");

        var call = Expression.Call(likeMethod, efFunctions, property, Expression.Constant(pattern));
        var lambda = Expression.Lambda<Func<T, bool>>(call, parameter);

        return query.Where(lambda);
    }

    /// <summary>
    /// Applies EF.Functions.Like() search across multiple properties (OR logic).
    /// </summary>
    public static IQueryable<T> LikeSearch<T>(
        this IQueryable<T> query,
        string searchTerm,
        params Expression<Func<T, string?>>[] propertySelectors)
        where T : class
    {
        if (string.IsNullOrWhiteSpace(searchTerm) || propertySelectors.Length == 0)
            return query;

        var pattern = $"%{EscapeLikePattern(searchTerm)}%";
        var parameter = Expression.Parameter(typeof(T), "x");
        Expression? combined = null;

        var efFunctions = Expression.Property(null, typeof(EF), "Functions");
        var likeMethod = typeof(DbFunctionsExtensions).GetMethod(
            nameof(DbFunctionsExtensions.Like),
            new[] { typeof(DbFunctions), typeof(string), typeof(string) });

        if (likeMethod == null)
            throw new InvalidOperationException("EF.Functions.Like not available.");

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
    /// Applies case-insensitive LIKE search using EF.Functions.ILike() (PostgreSQL only).
    /// Falls back to regular Like for other databases.
    /// </summary>
    public static IQueryable<T> ILikeSearch<T>(
        this IQueryable<T> query,
        Expression<Func<T, string?>> propertySelector,
        string searchTerm)
        where T : class
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return query;

        var pattern = $"%{EscapeLikePattern(searchTerm)}%";
        var parameter = propertySelector.Parameters[0];
        var property = propertySelector.Body;

        var efFunctions = Expression.Property(null, typeof(EF), "Functions");

        // Try ILike first (PostgreSQL)
        var iLikeMethod = GetILikeMethod();
        if (iLikeMethod != null)
        {
            var call = Expression.Call(iLikeMethod, efFunctions, property, Expression.Constant(pattern));
            var lambda = Expression.Lambda<Func<T, bool>>(call, parameter);
            return query.Where(lambda);
        }

        // Fallback to standard Like (case sensitivity depends on database collation)
        return LikeSearch(query, propertySelector, searchTerm);
    }

    /// <summary>
    /// Escapes special characters in LIKE patterns.
    /// </summary>
    private static string EscapeLikePattern(string pattern)
    {
        return pattern
            .Replace("[", "[[]")
            .Replace("%", "[%]")
            .Replace("_", "[_]");
    }

    /// <summary>
    /// Replaces a parameter in an expression tree.
    /// </summary>
    private static Expression ReplaceParameter(Expression expression, ParameterExpression oldParam, ParameterExpression newParam)
    {
        return new ParameterReplacer(oldParam, newParam).Visit(expression);
    }

    private static System.Reflection.MethodInfo? GetILikeMethod()
    {
        // Try to find NpgsqlDbFunctionsExtensions.ILike
        var npgsqlAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == "Npgsql.EntityFrameworkCore.PostgreSQL");

        if (npgsqlAssembly == null)
            return null;

        var extensionsType = npgsqlAssembly.GetType("Microsoft.EntityFrameworkCore.NpgsqlDbFunctionsExtensions");
        return extensionsType?.GetMethod("ILike", new[] { typeof(DbFunctions), typeof(string), typeof(string) });
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
