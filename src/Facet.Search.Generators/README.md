# Facet.Search.Generators

[![NuGet](https://img.shields.io/nuget/v/Facet.Search.Generators.svg)](https://www.nuget.org/packages/Facet.Search.Generators/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

**Source generators for Facet.Search** - Automatically generates faceted search infrastructure from your domain models at compile time.

> ?? **Note**: This package is included automatically when you install [`Facet.Search`](https://www.nuget.org/packages/Facet.Search/). You only need to install this package directly if you want to use the generators without the main package.

## What Gets Generated

When you decorate your models with `[FacetedSearch]` and `[SearchFacet]` attributes, this generator creates:

| Generated Class | Description |
|-----------------|-------------|
| `{Model}SearchFilter` | Strongly-typed filter class with properties for each facet |
| `{Model}SearchExtensions` | LINQ extension methods (`ApplyFacetedSearch`, etc.) |
| `{Model}FacetResults` | Aggregation results class for facet counts and ranges |
| `{Model}SearchMetadata` | Static metadata for building dynamic UIs |

## How It Works

1. The generator scans your compilation for classes marked with `[FacetedSearch]`
2. It analyzes properties with `[SearchFacet]`, `[FullTextSearch]`, and `[Searchable]` attributes
3. At compile time, it emits `.g.cs` files with all the search infrastructure

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

## Related Packages

- [Facet.Search](https://www.nuget.org/packages/Facet.Search/) - Main package with attributes (includes this generator)
- [Facet.Search.EFCore](https://www.nuget.org/packages/Facet.Search.EFCore/) - Entity Framework Core async extensions

## License

MIT License - see [LICENSE](https://github.com/Tim-Maes/Facet.Search/blob/master/LICENSE) for details.
