# Facet.Search.EFCore

[![NuGet](https://img.shields.io/nuget/v/Facet.Search.EFCore.svg)](https://www.nuget.org/packages/Facet.Search.EFCore/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

**Entity Framework Core integration for Facet.Search** � Async extensions for executing faceted searches and aggregations with EF Core.

## Installation

```bash
dotnet add package Facet.Search.EFCore
```

> **Note**: This package requires [Facet.Search](https://www.nuget.org/packages/Facet.Search/) for the core attributes and source generators.

## Features

- **Async Query Execution** - `ExecuteSearchAsync`, `CountSearchResultsAsync`
- **Pagination** - `ToPagedResultAsync`, `Paginate`
- **Facet Aggregations** - `AggregateFacetAsync`, `GetRangeAsync`, `CountBooleanAsync`
- **Sorting** - `SortBy`, `ThenSortBy`
- **Full-Text Search** - SQL Server FREETEXT/CONTAINS, PostgreSQL ILike, EF.Functions.Like
- **SQL Translated** - All operations execute on the database server

## How It Works

All Facet.Search filters are **translated to SQL** and executed on the database server. No client-side evaluation is performed for facet filtering.

```csharp
// This filter...
var filter = new ProductSearchFilter
{
    Brand = ["Apple", "Samsung"],
    MinPrice = 100m,
    MaxPrice = 1000m,
    InStock = true
};

// Generates SQL like:
// SELECT * FROM Products 
// WHERE Brand IN ('Apple', 'Samsung')
//   AND Price >= 100 AND Price <= 1000
//   AND InStock = 1
```

## Quick Start

```csharp
using Facet.Search.EFCore;

// Execute search asynchronously
var results = await dbContext.Products
    .ApplyFacetedSearch(filter)
    .ExecuteSearchAsync();

// Get total count
var count = await dbContext.Products
    .ApplyFacetedSearch(filter)
    .CountSearchResultsAsync();

// Paginated results with metadata
var pagedResult = await dbContext.Products
    .ApplyFacetedSearch(filter)
    .ToPagedResultAsync(page: 1, pageSize: 20);

// pagedResult.Items - the items for the current page
// pagedResult.TotalCount - total matching items
// pagedResult.TotalPages - total number of pages
// pagedResult.HasNextPage / HasPreviousPage
```

## Full-Text Search

### Universal LIKE Search

Works with all databases:

```csharp
using Facet.Search.EFCore;

// Single property
var results = await dbContext.Products
    .LikeSearch(p => p.Name, "laptop")
    .ToListAsync();

// Multiple properties (OR)
var results = await dbContext.Products
    .LikeSearch("laptop", p => p.Name, p => p.Description)
    .ToListAsync();
```

### SQL Server Full-Text Search

Requires a FULLTEXT index on the column(s):

```sql
-- Create full-text index
CREATE FULLTEXT CATALOG ProductsCatalog AS DEFAULT;
CREATE FULLTEXT INDEX ON Products(Name, Description) KEY INDEX PK_Products;
```

```csharp
using Facet.Search.EFCore;

// FREETEXT - Natural language search with word stemming
var results = await dbContext.Products
    .FreeTextSearch(p => p.Name, "laptop computer")
    .ToListAsync();

// Multiple properties
var results = await dbContext.Products
    .FreeTextSearch("laptop", p => p.Name, p => p.Description)
    .ToListAsync();

// CONTAINS - Precise search with boolean operators
var results = await dbContext.Products
    .ContainsSearch(p => p.Name, "laptop AND gaming")
    .ToListAsync();

// CONTAINS supports:
// - "laptop AND gaming" - Both words
// - "laptop OR desktop" - Either word  
// - '"laptop computer"' - Exact phrase
// - "laptop*" - Prefix search
// - "laptop NEAR gaming" - Words near each other
```

### PostgreSQL Full-Text Search

For case-insensitive LIKE (ILike):

```csharp
using Facet.Search.EFCore;

// Case-insensitive search (PostgreSQL only, falls back to Like on other DBs)
var results = await dbContext.Products
    .ILikeSearch(p => p.Name, "LAPTOP")  // Matches "laptop", "Laptop", "LAPTOP"
    .ToListAsync();

// Multiple properties
var results = await dbContext.Products
    .ILikeSearch("laptop", p => p.Name, p => p.Description)
    .ToListAsync();
```

For proper tsvector/tsquery search, configure your DbContext:

```csharp
// 1. Add a tsvector column to your entity
public class Product
{
    public string Name { get; set; }
    public string Description { get; set; }
    public NpgsqlTsVector SearchVector { get; set; }  // From NpgsqlTypes
}

// 2. Configure in OnModelCreating
modelBuilder.Entity<Product>()
    .HasGeneratedTsVectorColumn(
        p => p.SearchVector,
        "english",
        p => new { p.Name, p.Description })
    .HasIndex(p => p.SearchVector)
    .HasMethod("GIN");

// 3. Query using Npgsql's built-in methods
var results = await context.Products
    .Where(p => p.SearchVector.Matches(EF.Functions.ToTsQuery("english", "laptop")))
    .OrderByDescending(p => p.SearchVector.Rank(EF.Functions.ToTsQuery("english", "laptop")))
    .ToListAsync();
```

## Facet Aggregations

All aggregations are executed as SQL queries on the database:

```csharp
using Facet.Search.EFCore;

// Get categorical facet counts (e.g., brand -> count)
var brandCounts = await dbContext.Products
    .AggregateFacetAsync(p => p.Brand, limit: 10);
// Returns: { "Apple": 42, "Samsung": 38, "Google": 25, ... }

// Get min/max range for numeric properties
var (minPrice, maxPrice) = await dbContext.Products
    .GetRangeAsync(p => p.Price);

// Count boolean values
var (inStockCount, outOfStockCount) = await dbContext.Products
    .CountBooleanAsync(p => p.InStock);
```

### All-in-One Async Aggregations

Use `GetFacetAggregationsAsync` to execute **all facet aggregations at once** and populate the generated `*FacetResults` class:

```csharp
using Facet.Search.EFCore;
using YourNamespace.Search;

// Execute all aggregations asynchronously
var aggregations = await dbContext.Products
    .GetFacetAggregationsAsync<Product, ProductFacetResults>();

// Access all facet data from a single call:
// - Categorical facets
Console.WriteLine($"Brands: {string.Join(", ", aggregations.Brand.Keys)}");
// Output: Brands: Apple, Samsung, Google

// - Range facets
Console.WriteLine($"Price range: ${aggregations.PriceMin} - ${aggregations.PriceMax}");
// Output: Price range: $99.99 - $2499.99

// - Boolean facets
Console.WriteLine($"In stock: {aggregations.InStockTrueCount}, Out of stock: {aggregations.InStockFalseCount}");
// Output: In stock: 42, Out of stock: 8
```

This is the async equivalent of the generated `GetFacetAggregations()` method, providing the same results with async execution.

**Benefits:**
- ✅ Single method call for all facets
- ✅ Type-safe with generated `*FacetResults` class
- ✅ Works with filtered queries
- ✅ Executes asynchronously

**Example with filtering:**
```csharp
// Get aggregations for filtered results (e.g., only electronics)
var filter = new ProductSearchFilter { Category = ["Electronics"] };

var aggregations = await dbContext.Products
    .ApplyFacetedSearch(filter)
    .GetFacetAggregationsAsync<Product, ProductFacetResults>();

// Now aggregations only reflect electronics products
```

## Pagination & Sorting

### Built-in Sorting via Filter

Sorting is built into the generated filter class:

```csharp
using Facet.Search.EFCore;

// Use filter properties for sorting
var filter = new ProductSearchFilter
{
    Category = ["Electronics"],
    SortBy = "Price",           // Property name to sort by
    SortDescending = true       // Sort direction
};

var results = await dbContext.Products
    .ApplyFacetedSearch(filter)
    .ExecuteSearchAsync();

// Results are filtered and sorted
```

### Manual Sorting with Extension Methods

You can also use EF Core extension methods for more control:

```csharp
// Apply pagination (page 2, 25 items per page)
var items = await dbContext.Products
    .ApplyFacetedSearch(filter)
    .Paginate(page: 2, pageSize: 25)
    .ExecuteSearchAsync();

// Apply sorting manually
var sorted = await dbContext.Products
    .ApplyFacetedSearch(filter)
    .SortBy(p => p.Price, descending: true)
    .ThenSortBy(p => p.Name)
    .ExecuteSearchAsync();
```

## Complete Example

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductDbContext _context;

    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] string[]? brands,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] bool? inStock,
        [FromQuery] string? search,
        [FromQuery] string? sortBy,
        [FromQuery] bool sortDescending = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var filter = new ProductSearchFilter
        {
            Brand = brands,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            InStock = inStock,
            SearchText = search,
            SortBy = sortBy,
            SortDescending = sortDescending
        };

        var result = await _context.Products
            .ApplyFacetedSearch(filter)    // Applies filtering and sorting
            .ToPagedResultAsync(page, pageSize);

        return Ok(result);
    }

    [HttpGet("facets")]
    public async Task<IActionResult> GetFacets()
    {
        // Get all facet aggregations with a single call
        var aggregations = await _context.Products
            .GetFacetAggregationsAsync<Product, ProductFacetResults>();

        return Ok(new
        {
            brands = aggregations.Brand,
            categories = aggregations.Category,
            priceRange = new { min = aggregations.PriceMin, max = aggregations.PriceMax },
            inStock = aggregations.InStockTrueCount,
            outOfStock = aggregations.InStockFalseCount
        });
    }

    [HttpGet("facets/filtered")]
    public async Task<IActionResult> GetFacetsForCategory([FromQuery] string category)
    {
        // Get aggregations for a specific category
        var filter = new ProductSearchFilter { Category = [category] };

        var aggregations = await _context.Products
            .ApplyFacetedSearch(filter)
            .GetFacetAggregationsAsync<Product, ProductFacetResults>();

        return Ok(aggregations);
    }
}
```

## API Reference

### Full-Text Search Extensions

| Method | Database | Description |
|--------|----------|-------------|
| `LikeSearch<T>(property, term)` | All | EF.Functions.Like with wildcards |
| `LikeSearch<T>(term, properties...)` | All | Multi-property OR search |
| `ILikeSearch<T>(property, term)` | PostgreSQL | Case-insensitive LIKE |
| `FreeTextSearch<T>(property, term)` | SQL Server | FREETEXT with word stemming |
| `FreeTextSearch<T>(term, properties...)` | SQL Server | Multi-property FREETEXT |
| `ContainsSearch<T>(property, term)` | SQL Server | CONTAINS with boolean operators |

### Query Extensions

| Method | Description |
|--------|-------------|
| `ExecuteSearchAsync<T>()` | Returns `List<T>` |
| `CountSearchResultsAsync<T>()` | Returns total count |
| `ToPagedResultAsync<T>(page, pageSize)` | Returns `PagedResult<T>` |
| `HasResultsAsync<T>()` | Returns `true` if any match |
| `FirstOrDefaultSearchResultAsync<T>()` | Returns first or `null` |

### Aggregation Extensions

| Method | Description |
|--------|-------------|
| `GetFacetAggregationsAsync<TEntity, TResults>()` | **Executes all facet aggregations** and returns populated `*FacetResults` |
| `AggregateFacetAsync<T, TKey>(selector, limit?)` | Groups and counts a single facet |
| `GetMinAsync<T, TResult>(selector)` | Gets minimum value |
| `GetMaxAsync<T, TResult>(selector)` | Gets maximum value |
| `GetRangeAsync<T, TResult>(selector)` | Gets `(min, max)` tuple |
| `CountBooleanAsync<T>(selector)` | Returns `(trueCount, falseCount)` |

### Pagination Extensions

| Method | Description |
|--------|-------------|
| `Paginate<T>(page, pageSize)` | Applies Skip/Take pagination |
| `SortBy<T, TKey>(selector, descending?)` | Primary sort |
| `ThenSortBy<T, TKey>(selector, descending?)` | Secondary sort |

## PagedResult&lt;T&gt;

```csharp
public class PagedResult<T>
{
    public List<T> Items { get; set; }      // Items for current page
    public int Page { get; set; }            // Current page number (1-based)
    public int PageSize { get; set; }        // Items per page
    public int TotalCount { get; set; }      // Total matching items
    public int TotalPages { get; }           // Calculated: ceil(TotalCount / PageSize)
    public bool HasNextPage { get; }         // Page < TotalPages
    public bool HasPreviousPage { get; }     // Page > 1
}
```

## Performance Tips

1. **Use full-text indexes** for FREETEXT/CONTAINS on large datasets
2. **Add indexes** on facet columns for faster filtering
3. **Use `limit` parameter** in `AggregateFacetAsync` to avoid loading all distinct values
4. **Consider caching aggregations** if they don't change frequently
5. **Use projection** with `.Select()` if you don't need all columns

## Requirements

- .NET 10.0+
- Entity Framework Core 10.0+
- [Facet.Search](https://www.nuget.org/packages/Facet.Search/) package
- For SQL Server FTS: Microsoft.EntityFrameworkCore.SqlServer
- For PostgreSQL FTS: Npgsql.EntityFrameworkCore.PostgreSQL

## Related Packages

- [Facet.Search](https://www.nuget.org/packages/Facet.Search/) � Core package with attributes and source generators

## License

MIT License � see [LICENSE](https://github.com/Tim-Maes/Facet.Search/blob/master/LICENSE.txt) for details.
