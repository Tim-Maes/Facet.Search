using Microsoft.EntityFrameworkCore;
using Facet.Search.EFCore.Tests.Data;
using Facet.Search.EFCore.Tests.Models;

namespace Facet.Search.EFCore.Tests.Fixtures;

/// <summary>
/// Test fixture that provides a SQLite in-memory database with seed data.
/// </summary>
public class DatabaseFixture : IDisposable
{
    public TestDbContext Context { get; }

    public DatabaseFixture()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        Context = new TestDbContext(options);
        Context.Database.OpenConnection();
        Context.Database.EnsureCreated();

        SeedData();
    }

    private void SeedData()
    {
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Laptop Pro 15", Description = "High-performance laptop", Brand = "TechCorp", Category = "Electronics", Price = 1299.99m, InStock = true, CreatedAt = DateTime.Now.AddDays(-30), Rating = 5 },
            new() { Id = 2, Name = "Wireless Mouse", Description = "Ergonomic wireless mouse", Brand = "TechCorp", Category = "Electronics", Price = 49.99m, InStock = true, CreatedAt = DateTime.Now.AddDays(-10), Rating = 4 },
            new() { Id = 3, Name = "Office Chair", Description = "Comfortable office chair", Brand = "ComfortPlus", Category = "Furniture", Price = 299.99m, InStock = false, CreatedAt = DateTime.Now.AddDays(-60), Rating = 4 },
            new() { Id = 4, Name = "Standing Desk", Description = "Adjustable standing desk", Brand = "ComfortPlus", Category = "Furniture", Price = 599.99m, InStock = true, CreatedAt = DateTime.Now.AddDays(-5), Rating = 5 },
            new() { Id = 5, Name = "Gaming Laptop", Description = "High-end gaming laptop", Brand = "GameTech", Category = "Electronics", Price = 1999.99m, InStock = false, CreatedAt = DateTime.Now.AddDays(-15), Rating = 5 },
            new() { Id = 6, Name = "Mechanical Keyboard", Description = "RGB mechanical keyboard", Brand = "GameTech", Category = "Electronics", Price = 149.99m, InStock = true, CreatedAt = DateTime.Now.AddDays(-20), Rating = 4 },
            new() { Id = 7, Name = "Monitor 27\"", Description = "4K monitor", Brand = "TechCorp", Category = "Electronics", Price = 449.99m, InStock = true, CreatedAt = DateTime.Now.AddDays(-25), Rating = 5 },
            new() { Id = 8, Name = "Desk Lamp", Description = "LED desk lamp", Brand = "HomeLight", Category = "Furniture", Price = 39.99m, InStock = true, CreatedAt = DateTime.Now.AddDays(-40), Rating = 3 },
            new() { Id = 9, Name = "Webcam HD", Description = "1080p webcam", Brand = "TechCorp", Category = "Electronics", Price = 79.99m, InStock = false, CreatedAt = DateTime.Now.AddDays(-35), Rating = 4 },
            new() { Id = 10, Name = "USB Hub", Description = "7-port USB hub", Brand = "TechCorp", Category = "Electronics", Price = 29.99m, InStock = true, CreatedAt = DateTime.Now.AddDays(-50), Rating = 4 },
        };

        Context.Products.AddRange(products);
        Context.SaveChanges();
    }

    public void Dispose()
    {
        Context.Database.CloseConnection();
        Context.Dispose();
    }
}

/// <summary>
/// Collection definition for sharing the database fixture across tests.
/// </summary>
[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
}
