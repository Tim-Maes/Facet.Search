# Facet.Search.EFCore

[![NuGet](https://img.shields.io/nuget/v/Facet.Search.EFCore.svg)](https://www.nuget.org/packages/Facet.Search.EFCore/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

**Entity Framework Core integration for Facet.Search** — Async extensions for executing faceted searches and aggregations with EF Core.

## Installation

```bash
dotnet add package Facet.Search.EFCore
```

> **Note**: This package requires [Facet.Search](https://www.nuget.org/packages/Facet.Search/) for the core attributes and source generators.

## Features

- **Async Query Execution** — `ExecuteSearchAsync`, `CountSearchResultsAsync`
- **Pagination** — `ToPagedResultAsync`, `Paginate`
- **Facet Aggregations** — `AggregateFacetAsync`, `GetRangeAsync`, `CountBooleanAsync`
- **Sorting** — `SortBy`, `ThenSortBy`
- **SQL Translated** — All operations execute on the database server

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

## Facet Aggregations

All aggregations are executed as SQL queries on the database:

```csharp
using Facet.Search.EFCore;

// Get categorical facet counts (e.g., brand -> count)
// SQL: SELECT Brand, COUNT(*) FROM Products GROUP BY Brand ORDER BY COUNT(*) DESC
var brandCounts = await dbContext.Products
    .AggregateFacetAsync(p => p.Brand, limit: 10);
// Returns: { "Apple": 42, "Samsung": 38, "Google": 25, ... }

// Get min/max range for numeric properties
// SQL: SELECT MIN(Price), MAX(Price) FROM Products
var (minPrice, maxPrice) = await dbContext.Products
    .GetRangeAsync(p => p.Price);

// Count boolean values
// SQL: SELECT COUNT(*) WHERE InStock = 1, COUNT(*) WHERE InStock = 0
var (inStockCount, outOfStockCount) = await dbContext.Products
    .CountBooleanAsync(p => p.InStock);
```

## Pagination & Sorting

```csharp
using Facet.Search.EFCore;

// Apply pagination (page 2, 25 items per page)
// SQL: SELECT ... ORDER BY Id OFFSET 25 ROWS FETCH NEXT 25 ROWS ONLY
var items = await dbContext.Products
    .ApplyFacetedSearch(filter)
    .Paginate(page: 2, pageSize: 25)
    .ExecuteSearchAsync();

// Apply sorting
// SQL: SELECT ... ORDER BY Price DESC, Name ASC
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
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var filter = new ProductSearchFilter
        {
            Brand = brands,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            InStock = inStock,
            SearchText = search
        };

        var result = await _context.Products
            .ApplyFacetedSearch(filter)
            .SortBy(p => p.Name)
            .ToPagedResultAsync(page, pageSize);

        return Ok(result);
    }

    [HttpGet("facets")]
    public async Task<IActionResult> GetFacets()
    {
        var brands = await _context.Products.AggregateFacetAsync(p => p.Brand, limit: 20);
        var (minPrice, maxPrice) = await _context.Products.GetRangeAsync(p => p.Price);
        var (inStock, outOfStock) = await _context.Products.CountBooleanAsync(p => p.InStock);

        return Ok(new { brands, priceRange = new { min = minPrice, max = maxPrice }, inStock, outOfStock });
    }
}
```

## API Reference

### Query Extensions

| Method | Description | SQL |
|--------|-------------|-----|
| `ExecuteSearchAsync<T>()` | Returns `List<T>` | `SELECT ...` |
| `CountSearchResultsAsync<T>()` | Returns total count | `SELECT COUNT(*)` |
| `ToPagedResultAsync<T>(page, pageSize)` | Returns `PagedResult<T>` | Count + Offset/Fetch |
| `HasResultsAsync<T>()` | Returns `true` if any match | `SELECT TOP 1 ...` |
| `FirstOrDefaultSearchResultAsync<T>()` | Returns first or `null` | `SELECT TOP 1 ...` |

### Aggregation Extensions

| Method | Description | SQL |
|--------|-------------|-----|
| `AggregateFacetAsync<T, TKey>(selector, limit?)` | Groups and counts | `GROUP BY ... ORDER BY COUNT(*)` |
| `GetMinAsync<T, TResult>(selector)` | Gets minimum value | `SELECT MIN(...)` |
| `GetMaxAsync<T, TResult>(selector)` | Gets maximum value | `SELECT MAX(...)` |
| `GetRangeAsync<T, TResult>(selector)` | Gets `(min, max)` tuple | `SELECT MIN(...), MAX(...)` |
| `CountBooleanAsync<T>(selector)` | Returns `(trueCount, falseCount)` | Two COUNT queries |

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

1. **Add indexes** on facet columns for faster filtering
2. **Use `limit` parameter** in `AggregateFacetAsync` to avoid loading all distinct values
3. **Apply filters before pagination** — the generated code handles this correctly
4. **Consider caching aggregations** if they don't change frequently
5. **Use projection** with `.Select()` if you don't need all columns

## Requirements

- .NET 10.0+
- Entity Framework Core 10.0+
- [Facet.Search](https://www.nuget.org/packages/Facet.Search/) package

## Related Packages

- [Facet.Search](https://www.nuget.org/packages/Facet.Search/) — Core package with attributes and source generators

## License

MIT License — see [LICENSE](https://github.com/Tim-Maes/Facet.Search/blob/master/LICENSE.txt) for details.
