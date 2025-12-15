using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Facet.Search.EFCore;

/// <summary>
/// Extensions for SQL Server full-text search (FREETEXT, CONTAINS).
/// Requires a FULLTEXT index on the searched columns.
/// </summary>
public static class SqlServerFullTextExtensions
{
    /// <summary>
    /// Applies SQL Server FREETEXT search to a queryable.
    /// FREETEXT provides natural language search with word stemming.
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <param name="query">The queryable to search.</param>
    /// <param name="propertySelector">Property to search.</param>
    /// <param name="searchTerm">The search term.</param>
    /// <returns>Filtered queryable.</returns>
    /// <remarks>
    /// Requires a FULLTEXT index on the column:
    /// <code>
    /// CREATE FULLTEXT INDEX ON Products(Name) KEY INDEX PK_Products;
    /// </code>
    /// </remarks>
    public static IQueryable<T> FreeTextSearch<T>(
        this IQueryable<T> query,
        Expression<Func<T, string?>> propertySelector,
        string searchTerm)
        where T : class
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return query;

        var parameter = propertySelector.Parameters[0];
        var property = propertySelector.Body;

        // Build expression for: EF.Functions.FreeText(property, searchTerm)
        var efFunctions = Expression.Property(null, typeof(EF), "Functions");

        var freeTextMethod = typeof(DbFunctionsExtensions).GetMethod(
            "FreeText",
            new[] { typeof(DbFunctions), typeof(string), typeof(string) });

        if (freeTextMethod == null)
            throw new InvalidOperationException(
                "EF.Functions.FreeText not available. Ensure Microsoft.EntityFrameworkCore.SqlServer is referenced.");

        var call = Expression.Call(freeTextMethod, efFunctions, property, Expression.Constant(searchTerm));
        var lambda = Expression.Lambda<Func<T, bool>>(call, parameter);

        return query.Where(lambda);
    }

    /// <summary>
    /// Applies SQL Server FREETEXT search across multiple properties (OR logic).
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <param name="query">The queryable to search.</param>
    /// <param name="searchTerm">The search term.</param>
    /// <param name="propertySelectors">Properties to search.</param>
    /// <returns>Filtered queryable.</returns>
    public static IQueryable<T> FreeTextSearch<T>(
        this IQueryable<T> query,
        string searchTerm,
        params Expression<Func<T, string?>>[] propertySelectors)
        where T : class
    {
        if (string.IsNullOrWhiteSpace(searchTerm) || propertySelectors.Length == 0)
            return query;

        var parameter = Expression.Parameter(typeof(T), "x");
        Expression? combined = null;

        var efFunctions = Expression.Property(null, typeof(EF), "Functions");

        var freeTextMethod = typeof(DbFunctionsExtensions).GetMethod(
            "FreeText",
            new[] { typeof(DbFunctions), typeof(string), typeof(string) });

        if (freeTextMethod == null)
            throw new InvalidOperationException(
                "EF.Functions.FreeText not available. Ensure Microsoft.EntityFrameworkCore.SqlServer is referenced.");

        foreach (var selector in propertySelectors)
        {
            var property = ReplaceParameter(selector.Body, selector.Parameters[0], parameter);
            var call = Expression.Call(freeTextMethod, efFunctions, property, Expression.Constant(searchTerm));

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
    /// Applies SQL Server CONTAINS search to a queryable.
    /// CONTAINS provides more precise search with boolean operators.
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <param name="query">The queryable to search.</param>
    /// <param name="propertySelector">Property to search.</param>
    /// <param name="searchTerm">The search term (can include CONTAINS operators like AND, OR, NEAR).</param>
    /// <returns>Filtered queryable.</returns>
    /// <remarks>
    /// Search term examples:
    /// - "laptop" - Simple search
    /// - "laptop AND gaming" - Both words
    /// - "laptop OR desktop" - Either word
    /// - '"laptop computer"' - Exact phrase
    /// - "laptop*" - Prefix search
    /// </remarks>
    public static IQueryable<T> ContainsSearch<T>(
        this IQueryable<T> query,
        Expression<Func<T, string?>> propertySelector,
        string searchTerm)
        where T : class
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return query;

        var parameter = propertySelector.Parameters[0];
        var property = propertySelector.Body;

        var efFunctions = Expression.Property(null, typeof(EF), "Functions");

        var containsMethod = typeof(DbFunctionsExtensions).GetMethod(
            "Contains",
            new[] { typeof(DbFunctions), typeof(string), typeof(string) });

        if (containsMethod == null)
            throw new InvalidOperationException(
                "EF.Functions.Contains not available. Ensure Microsoft.EntityFrameworkCore.SqlServer is referenced.");

        var call = Expression.Call(containsMethod, efFunctions, property, Expression.Constant(searchTerm));
        var lambda = Expression.Lambda<Func<T, bool>>(call, parameter);

        return query.Where(lambda);
    }

    /// <summary>
    /// Applies SQL Server CONTAINS search across multiple properties (OR logic).
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <param name="query">The queryable to search.</param>
    /// <param name="searchTerm">The search term.</param>
    /// <param name="propertySelectors">Properties to search.</param>
    /// <returns>Filtered queryable.</returns>
    public static IQueryable<T> ContainsSearch<T>(
        this IQueryable<T> query,
        string searchTerm,
        params Expression<Func<T, string?>>[] propertySelectors)
        where T : class
    {
        if (string.IsNullOrWhiteSpace(searchTerm) || propertySelectors.Length == 0)
            return query;

        var parameter = Expression.Parameter(typeof(T), "x");
        Expression? combined = null;

        var efFunctions = Expression.Property(null, typeof(EF), "Functions");

        var containsMethod = typeof(DbFunctionsExtensions).GetMethod(
            "Contains",
            new[] { typeof(DbFunctions), typeof(string), typeof(string) });

        if (containsMethod == null)
            throw new InvalidOperationException(
                "EF.Functions.Contains not available. Ensure Microsoft.EntityFrameworkCore.SqlServer is referenced.");

        foreach (var selector in propertySelectors)
        {
            var property = ReplaceParameter(selector.Body, selector.Parameters[0], parameter);
            var call = Expression.Call(containsMethod, efFunctions, property, Expression.Constant(searchTerm));

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
    /// Replaces a parameter in an expression tree.
    /// </summary>
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
