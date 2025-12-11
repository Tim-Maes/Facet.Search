using Facet.Search.TestConsumer.Models;
using Facet.Search.TestConsumer.Models.Search;

// Sample data
var products = new List<Product>
{
    new() { Id = 1, Name = "Laptop Pro", Brand = "TechCorp", Category = "Electronics", Price = 1299.99m, InStock = true, CreatedAt = DateTime.Now.AddDays(-30), Rating = 5 },
    new() { Id = 2, Name = "Wireless Mouse", Brand = "TechCorp", Category = "Electronics", Price = 49.99m, InStock = true, CreatedAt = DateTime.Now.AddDays(-10), Rating = 4 },
    new() { Id = 3, Name = "Office Chair", Brand = "ComfortPlus", Category = "Furniture", Price = 299.99m, InStock = false, CreatedAt = DateTime.Now.AddDays(-60), Rating = 4 },
    new() { Id = 4, Name = "Standing Desk", Brand = "ComfortPlus", Category = "Furniture", Price = 599.99m, InStock = true, CreatedAt = DateTime.Now.AddDays(-5), Rating = 5 },
};

// Create a filter
var filter = new ProductSearchFilter
{
    Brand = ["TechCorp"],
    InStock = true,
    MinPrice = 10m,
    MaxPrice = 1500m,
    SearchText = "laptop"
};

// Apply faceted search
var results = products.AsQueryable()
    .ApplyFacetedSearch(filter)
    .ToList();

Console.WriteLine($"Found {results.Count} products matching filters:");
foreach (var product in results)
{
    Console.WriteLine($"  - {product.Name} ({product.Brand}) - ${product.Price}");
}

// Get facet aggregations
Console.WriteLine("\n--- Facet Aggregations ---");
var aggregations = products.AsQueryable().GetFacetAggregations();

Console.WriteLine("Brands:");
foreach (var kvp in aggregations.Brand)
{
    Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
}

Console.WriteLine($"Price Range: ${aggregations.PriceMin} - ${aggregations.PriceMax}");

// Get metadata
Console.WriteLine("\n--- Facet Metadata ---");
foreach (var facet in ProductSearchMetadata.Facets)
{
    Console.WriteLine($"  {facet.DisplayName} ({facet.Type})");
}

Console.WriteLine("\nFacet.Search is working!");
