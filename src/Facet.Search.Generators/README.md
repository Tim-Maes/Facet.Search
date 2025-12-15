# Facet.Search.Generators

[![NuGet](https://img.shields.io/nuget/v/Facet.Search.Generators.svg)](https://www.nuget.org/packages/Facet.Search.Generators/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

**Source generators for Facet.Search** — Automatically generates faceted search infrastructure from your domain models at compile time.

> **Note**: This package is included automatically when you install [`Facet.Search`](https://www.nuget.org/packages/Facet.Search/). You only need to install this package directly if you want to use the generators without the main package.

## What Gets Generated

When you decorate your models with `[FacetedSearch]` and `[SearchFacet]` attributes, this generator creates:

| Generated Class | Description |
|-----------------|-------------|
| `{Model}SearchFilter` | Strongly-typed filter class with properties for each facet |
| `{Model}SearchExtensions` | LINQ extension methods (`ApplyFacetedSearch`, `GetFacetAggregations`) |
| `{Model}FacetAggregations` | Aggregation results class for facet counts and ranges |
| `{Model}SearchMetadata` | Static metadata for building dynamic UIs |

## Generated Code Example

For this model:

```csharp
[FacetedSearch]
public class Product
{
    [FullTextSearch]
    public string Name { get; set; }

    [SearchFacet(Type = FacetType.Categorical)]
    public string Brand { get; set; }

    [SearchFacet(Type = FacetType.Range)]
    public decimal Price { get; set; }

    [SearchFacet(Type = FacetType.Boolean)]
    public bool InStock { get; set; }
}
```

The generator creates:

### ProductSearchFilter.g.cs
```csharp
public class ProductSearchFilter
{
    public string? SearchText { get; set; }
    public string[]? Brand { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? InStock { get; set; }
}
```

### ProductSearchExtensions.g.cs
```csharp
public static class ProductSearchExtensions
{
    public static IQueryable<Product> ApplyFacetedSearch(
        this IQueryable<Product> query,
        ProductSearchFilter filter)
    {
        // All filter logic translates to SQL
        if (filter.Brand?.Any() == true)
            query = query.Where(x => filter.Brand.Contains(x.Brand));
        
        if (filter.MinPrice.HasValue)
            query = query.Where(x => x.Price >= filter.MinPrice.Value);
        
        // ... etc
        return query;
    }
}
```

## How It Works

1. The generator scans your compilation for classes marked with `[FacetedSearch]`
2. It analyzes properties with `[SearchFacet]`, `[FullTextSearch]`, and `[Searchable]` attributes
3. At compile time, it emits `.g.cs` files with all the search infrastructure
4. **All generated LINQ expressions are translated to SQL** by EF Core

## SQL Translation

The generated code uses standard LINQ expressions that EF Core translates to SQL:

| Filter Type | Generated Code | SQL Translation |
|-------------|---------------|-----------------|
| Categorical | `filter.Brand.Contains(x.Brand)` | `WHERE Brand IN (...)` |
| Range | `x.Price >= min` | `WHERE Price >= @min` |
| Boolean | `x.InStock == true` | `WHERE InStock = 1` |
| Full-Text | `x.Name.Contains(term)` | `WHERE Name LIKE '%term%'` |

## Generated Code Location

Generated files appear in your project's `obj` folder:

```
obj/Debug/net8.0/generated/Facet.Search.Generators/
??? ProductSearchFilter.g.cs
??? ProductSearchExtensions.g.cs
??? ProductFacetAggregations.g.cs
??? ProductSearchMetadata.g.cs
```

## Viewing Generated Code

In Visual Studio:
1. Expand your project in Solution Explorer
2. Expand **Dependencies** ? **Analyzers** ? **Facet.Search.Generators**
3. View the generated `.g.cs` files

Or check the `obj` folder directly.

## Requirements

- .NET Standard 2.0+
- C# 9.0+

## Installation

Typically, you should install the main package which bundles this generator:

```bash
dotnet add package Facet.Search
```

For standalone generator installation:

```bash
dotnet add package Facet.Search.Generators
```

## Troubleshooting

### Generator not running?
```bash
dotnet clean
dotnet build --no-incremental
```

### Can't see generated code?
Check `obj/Debug/net8.0/generated/Facet.Search.Generators/`

### IntelliSense not working?
Restart Visual Studio or run `dotnet build`

## Related Packages

- [Facet.Search](https://www.nuget.org/packages/Facet.Search/) — Main package with attributes (includes this generator)
- [Facet.Search.EFCore](https://www.nuget.org/packages/Facet.Search.EFCore/) — Entity Framework Core async extensions

## License

MIT License — see [LICENSE](https://github.com/Tim-Maes/Facet.Search/blob/master/LICENSE.txt) for details.
