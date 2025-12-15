using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Facet.Search.EFCore;

/// <summary>
/// Extensions for PostgreSQL full-text search.
/// For simple LIKE-based search, use <see cref="FullTextSearchExtensions.ILikeSearch{T}"/>.
/// For proper tsvector/tsquery search, configure your DbContext with Npgsql's full-text support.
/// </summary>
/// <remarks>
/// PostgreSQL full-text search setup:
/// <code>
/// // 1. Add a tsvector column to your entity
/// public class Product
/// {
///     public string Name { get; set; }
///     public string Description { get; set; }
///     public NpgsqlTsVector SearchVector { get; set; }
/// }
/// 
/// // 2. Configure in OnModelCreating
/// modelBuilder.Entity&lt;Product&gt;()
///     .HasGeneratedTsVectorColumn(
///         p => p.SearchVector,
///         "english",
///         p => new { p.Name, p.Description })
///     .HasIndex(p => p.SearchVector)
///     .HasMethod("GIN");
/// 
/// // 3. Query using Npgsql's built-in methods
/// var results = context.Products
///     .Where(p => p.SearchVector.Matches(EF.Functions.ToTsQuery("english", "laptop")))
///     .ToList();
/// </code>
/// </remarks>
public static class PostgreSqlFullTextExtensions
{
    /// <summary>
    /// Applies PostgreSQL ILike search (case-insensitive LIKE).
    /// This is a convenience wrapper around EF.Functions.ILike().
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <param name="query">The queryable to search.</param>
    /// <param name="propertySelector">Property to search.</param>
    /// <param name="searchTerm">The search term.</param>
    /// <returns>Filtered queryable.</returns>
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

        // Try to find ILike method from Npgsql
        var iLikeMethod = GetNpgsqlILikeMethod();
        if (iLikeMethod == null)
        {
            // Fallback to standard Like if Npgsql is not available
            return FullTextSearchExtensions.LikeSearch(query, propertySelector, searchTerm);
        }

        var call = Expression.Call(iLikeMethod, efFunctions, property, Expression.Constant(pattern));
        var lambda = Expression.Lambda<Func<T, bool>>(call, parameter);

        return query.Where(lambda);
    }

    /// <summary>
    /// Applies PostgreSQL ILike search across multiple properties (OR logic).
    /// </summary>
    public static IQueryable<T> ILikeSearch<T>(
        this IQueryable<T> query,
        string searchTerm,
        params Expression<Func<T, string?>>[] propertySelectors)
        where T : class
    {
        if (string.IsNullOrWhiteSpace(searchTerm) || propertySelectors.Length == 0)
            return query;

        var iLikeMethod = GetNpgsqlILikeMethod();
        if (iLikeMethod == null)
        {
            // Fallback to standard Like search
            return FullTextSearchExtensions.LikeSearch(query, searchTerm, propertySelectors);
        }

        var pattern = $"%{EscapeLikePattern(searchTerm)}%";
        var parameter = Expression.Parameter(typeof(T), "x");
        Expression? combined = null;

        var efFunctions = Expression.Property(null, typeof(EF), "Functions");

        foreach (var selector in propertySelectors)
        {
            var property = ReplaceParameter(selector.Body, selector.Parameters[0], parameter);
            var call = Expression.Call(iLikeMethod, efFunctions, property, Expression.Constant(pattern));

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
    /// Checks if Npgsql full-text search is available.
    /// </summary>
    public static bool IsNpgsqlAvailable()
    {
        return GetNpgsqlILikeMethod() != null;
    }

    private static string EscapeLikePattern(string pattern)
    {
        return pattern
            .Replace("\\", "\\\\")
            .Replace("%", "\\%")
            .Replace("_", "\\_");
    }

    private static MethodInfo? GetNpgsqlILikeMethod()
    {
        // Try to find NpgsqlDbFunctionsExtensions.ILike
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
