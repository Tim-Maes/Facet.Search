using Facet.Search;

namespace Facet.Search.EFCore.Tests.Models;

/// <summary>
/// Test entity for EF Core integration tests.
/// </summary>
[FacetedSearch]
public class Product
{
    public int Id { get; set; }

    [FullTextSearch]
    public string Name { get; set; } = null!;

    [FullTextSearch(Weight = 0.5f)]
    public string? Description { get; set; }

    [SearchFacet(Type = FacetType.Categorical, DisplayName = "Brand")]
    public string Brand { get; set; } = null!;

    [SearchFacet(Type = FacetType.Categorical, DisplayName = "Category")]
    public string Category { get; set; } = null!;

    [SearchFacet(Type = FacetType.Range, DisplayName = "Price")]
    public decimal Price { get; set; }

    [SearchFacet(Type = FacetType.Boolean, DisplayName = "In Stock")]
    public bool InStock { get; set; }

    [SearchFacet(Type = FacetType.DateRange, DisplayName = "Created Date")]
    public DateTime CreatedAt { get; set; }

    [Searchable(Sortable = true)]
    public int Rating { get; set; }
}
