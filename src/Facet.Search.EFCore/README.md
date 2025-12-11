# Facet.Search.EFCore

[![NuGet](https://img.shields.io/nuget/v/Facet.Search.EFCore.svg)](https://www.nuget.org/packages/Facet.Search.EFCore/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

**Entity Framework Core integration for Facet.Search** - Async extensions for executing faceted searches and aggregations with EF Core.

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

## Pagination & Sorting

```csharp
using Facet.Search.EFCore;

// Apply pagination (page 2, 25 items per page)
var items = await dbContext.Products
    .ApplyFacetedSearch(filter)
    .Paginate(page: 2, pageSize: 25)
    .ExecuteSearchAsync();

// Apply sorting
var sorted = await dbContext.Products
    .ApplyFacetedSearch(filter)
    .SortBy(p => p.Price, descending: true)
    .ThenSortBy(p => p.Name)
    .ExecuteSearchAsync();
```

## API Reference

### Query Extensions

| Method | Description |
|--------|-------------|
| `ExecuteSearchAsync<T>()` | Executes query and returns `List<T>` |
| `CountSearchResultsAsync<T>()` | Returns total count of matching items |
| `ToPagedResultAsync<T>(page, pageSize)` | Returns `PagedResult<T>` with items and metadata |
| `HasResultsAsync<T>()` | Returns `true` if any results match |
| `FirstOrDefaultSearchResultAsync<T>()` | Returns first result or `null` |

### Aggregation Extensions

| Method | Description |
|--------|-------------|
| `AggregateFacetAsync<T, TKey>(selector, limit?)` | Groups and counts by key |
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

The `ToPagedResultAsync` method returns a `PagedResult<T>` with:

```csharp
public class PagedResult<T>
{
    public List<T> Items { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; }      // Calculated
    public bool HasNextPage { get; }    // Calculated
    public bool HasPreviousPage { get; } // Calculated
}
```

## Requirements

- .NET 10.0+
- Entity Framework Core 10.0+
- [Facet.Search](https://www.nuget.org/packages/Facet.Search/) package

## Related Packages

- [Facet.Search](https://www.nuget.org/packages/Facet.Search/) - Core package with attributes and source generators
- [Facet.Search.Generators](https://www.nuget.org/packages/Facet.Search.Generators/) - Standalone source generator

## License

MIT License - see [LICENSE](https://github.com/Tim-Maes/Facet.Search/blob/master/LICENSE) for details.
