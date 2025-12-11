# Facet.Search

[![NuGet](https://img.shields.io/nuget/v/Facet.Search.svg)](https://www.nuget.org/packages/Facet.Search/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

**Compile-time faceted search generation for .NET** - Zero boilerplate, type-safe, and performant.

Facet.Search uses source generators to automatically create search filter classes, LINQ extension methods, facet aggregations, and metadata from your domain models - all at compile time with no runtime overhead.

## Features

? **Zero Boilerplate** - Just add attributes to your models  
? **Type-Safe** - All filters are compile-time checked  
? **Performant** - Generated code is as efficient as hand-written  
? **Clean Architecture** - No Roslyn dependencies in your runtime  
? **EF Core Integration** - Async extensions for Entity Framework Core  
? **Full-Text Search** - Built-in text search support  
? **Facet Aggregations** - Automatic counting and range detection  
? **Frontend Metadata** - Generate facet metadata for UI consumption  

## Installation

```bash
dotnet add package Facet.Search
```

For Entity Framework Core integration:
```bash
dotnet add package Facet.Search.EFCore
```

## Quick Start

### 1. Define Your Model

```csharp
using Facet.Search;

[FacetedSearch]
public class Product
{
    public int Id { get; set; }

    [FullTextSearch]
    public string Name { get; set; } = null!;

    [SearchFacet(Type = FacetType.Categorical, DisplayName = "Brand")]
    public string Brand { get; set; } = null!;

    [SearchFacet(Type = FacetType.Range, DisplayName = "Price")]
    public decimal Price { get; set; }

    [SearchFacet(Type = FacetType.Boolean, DisplayName = "In Stock")]
    public bool InStock { get; set; }

    [SearchFacet(Type = FacetType.DateRange, DisplayName = "Created Date")]
    public DateTime CreatedAt { get; set; }
}
```

### 2. Use Generated Code

The source generator automatically creates:
- `ProductSearchFilter` - Filter class with all facet properties
- `ProductSearchExtensions` - LINQ extension methods
- `ProductFacetResults` - Aggregation results
- `ProductSearchMetadata` - Facet metadata for frontends

```csharp
using YourNamespace.Search;

// Create a filter
var filter = new ProductSearchFilter
{
    Brand = ["Apple", "Samsung"],
    MinPrice = 100m,
    MaxPrice = 1000m,
    InStock = true,
    SearchText = "laptop"
};

// Apply to any IQueryable<Product>
var results = products.AsQueryable()
    .ApplyFacetedSearch(filter)
    .ToList();

// Get facet aggregations
var aggregations = products.AsQueryable().GetFacetAggregations();
// aggregations.Brand = { "Apple": 5, "Samsung": 3, ... }
// aggregations.PriceMin = 99.99m
// aggregations.PriceMax = 2499.99m

// Access metadata for UI
foreach (var facet in ProductSearchMetadata.Facets)
{
    Console.WriteLine($"{facet.DisplayName} ({facet.Type})");
}
```

## Facet Types

| Type | Description | Generated Properties |
|------|-------------|---------------------|
| `Categorical` | Discrete values (e.g., Brand, Category) | `string[]? PropertyName` |
| `Range` | Numeric ranges (e.g., Price, Rating) | `decimal? MinPropertyName`, `decimal? MaxPropertyName` |
| `Boolean` | True/false filters (e.g., InStock) | `bool? PropertyName` |
| `DateRange` | Date/time ranges | `DateTime? PropertyNameFrom`, `DateTime? PropertyNameTo` |
| `Hierarchical` | Nested categories | `string[]? PropertyName` |

## Attributes

### `[FacetedSearch]`

Marks a class for search generation.

```csharp
[FacetedSearch(
    FilterClassName = "CustomFilter",      // Custom filter class name
    GenerateAggregations = true,           // Generate aggregation methods
    GenerateMetadata = true,               // Generate metadata class
    Namespace = "Custom.Namespace"         // Custom namespace for generated code
)]
public class Product { }
```

### `[SearchFacet]`

Marks a property as a filterable facet.

```csharp
[SearchFacet(
    Type = FacetType.Categorical,          // Facet type
    DisplayName = "Product Brand",         // UI display name
    OrderBy = FacetOrder.Count,            // Aggregation ordering
    Limit = 10,                            // Max aggregation values
    DependsOn = "Category",                // Dependent facet
    IsHierarchical = false                 // Hierarchical category
)]
public string Brand { get; set; }
```

### `[FullTextSearch]`

Marks a property for full-text search.

```csharp
[FullTextSearch(
    Weight = 1.0f,                         // Search relevance weight
    CaseSensitive = false,                 // Case sensitivity
    Behavior = TextSearchBehavior.Contains // Match behavior
)]
public string Name { get; set; }
```

### `[Searchable]`

Marks a property as searchable but not a facet.

```csharp
[Searchable(Sortable = true)]
public int Rating { get; set; }
```

## EF Core Integration

Use the `Facet.Search.EFCore` package for async operations:

```csharp
using Facet.Search.EFCore.Extensions;

// Async search execution
var results = await dbContext.Products
    .ApplyFacetedSearch(filter)
    .ExecuteSearchAsync();

// Async count
var count = await dbContext.Products
    .ApplyFacetedSearch(filter)
    .CountSearchResultsAsync();

// Async facet aggregation
var brandCounts = await dbContext.Products
    .AggregateFacetAsync(p => p.Brand, limit: 10);
```

## Generated Code Location

Generated files appear in your project's `obj` folder:
```
obj/Debug/net8.0/generated/Facet.Search.Generators/
??? ProductSearchFilter.g.cs
??? ProductSearchExtensions.g.cs
??? ProductFacetAggregations.g.cs
??? ProductSearchMetadata.g.cs
```

## Requirements

- .NET Standard 2.0+ (for Facet.Search)
- .NET 6.0+ (for Facet.Search.EFCore)
- C# 9.0+

## License

MIT License - see [LICENSE](LICENSE) for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Related Projects

- [Facet](https://github.com/Tim-Maes/Facet) - The Facet ecosystem
