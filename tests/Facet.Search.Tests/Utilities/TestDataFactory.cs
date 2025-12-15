namespace Facet.Search.Tests.Utilities;

/// <summary>
/// Factory for creating test data.
/// </summary>
public static class TestDataFactory
{
    public static Models.TestProduct CreateProduct(
        int id = 0,
        string name = "Test Product",
        string? description = null,
        string brand = "TestBrand",
        string category = "Electronics",
        decimal price = 99.99m,
        bool inStock = true,
        DateTime? createdAt = null,
        int rating = 4)
    {
        return new Models.TestProduct
        {
            Id = id == 0 ? Random.Shared.Next(1, 10000) : id,
            Name = name,
            Description = description ?? $"Description for {name}",
            Brand = brand,
            Category = category,
            Price = price,
            InStock = inStock,
            CreatedAt = createdAt ?? DateTime.Now.AddDays(-Random.Shared.Next(1, 60)),
            Rating = rating
        };
    }

    public static List<Models.TestProduct> CreateProductList()
    {
        return
        [
            CreateProduct(1, "Laptop Pro", "High-end laptop", "TechCorp", "Electronics", 1299.99m, true, DateTime.Now.AddDays(-30), 5),
            CreateProduct(2, "Wireless Mouse", "Ergonomic mouse", "TechCorp", "Electronics", 49.99m, true, DateTime.Now.AddDays(-10), 4),
            CreateProduct(3, "Office Chair", "Comfortable office chair", "ComfortPlus", "Furniture", 299.99m, false, DateTime.Now.AddDays(-60), 4),
            CreateProduct(4, "Standing Desk", "Adjustable desk", "ComfortPlus", "Furniture", 599.99m, true, DateTime.Now.AddDays(-5), 5),
            CreateProduct(5, "Gaming Laptop", "Gaming powerhouse", "GameTech", "Electronics", 1999.99m, false, DateTime.Now.AddDays(-15), 5),
            CreateProduct(6, "USB Hub", "4-port USB hub", "TechCorp", "Electronics", 29.99m, true, DateTime.Now.AddDays(-3), 4),
            CreateProduct(7, "Monitor Stand", "Adjustable monitor stand", "ComfortPlus", "Furniture", 89.99m, true, DateTime.Now.AddDays(-20), 3),
            CreateProduct(8, "Mechanical Keyboard", "RGB mechanical keyboard", "GameTech", "Electronics", 149.99m, true, DateTime.Now.AddDays(-7), 5),
        ];
    }

    public static Models.TestArticle CreateArticle(
        int id = 0,
        string title = "Test Article",
        string? slug = null,
        string? code = null,
        string author = "John Doe")
    {
        return new Models.TestArticle
        {
            Id = id == 0 ? Random.Shared.Next(1, 10000) : id,
            Title = title,
            Slug = slug ?? title.ToLower().Replace(" ", "-"),
            Code = code ?? $"ART{Random.Shared.Next(100, 999)}",
            Author = author
        };
    }

    public static List<Models.TestArticle> CreateArticleList()
    {
        return
        [
            CreateArticle(1, "Getting Started with C#", "getting-started-csharp", "CS001", "John"),
            CreateArticle(2, "Advanced C# Patterns", "advanced-csharp-patterns", "CS002", "Jane"),
            CreateArticle(3, "Introduction to F#", "intro-fsharp", "FS001", "John"),
            CreateArticle(4, "Python for Beginners", "python-beginners", "PY001", "Bob"),
            CreateArticle(5, "JavaScript Essentials", "javascript-essentials", "JS001", "Jane"),
        ];
    }

    public static Models.TestCategory CreateCategory(
        int id = 0,
        string path = "Electronics",
        string? subCategory = null)
    {
        return new Models.TestCategory
        {
            Id = id == 0 ? Random.Shared.Next(1, 10000) : id,
            Path = path,
            SubCategory = subCategory
        };
    }

    public static List<Models.TestCategory> CreateCategoryList()
    {
        return
        [
            CreateCategory(1, "Electronics", "Computers"),
            CreateCategory(2, "Electronics", "Mobile"),
            CreateCategory(3, "Electronics", "Audio"),
            CreateCategory(4, "Furniture", "Office"),
            CreateCategory(5, "Furniture", "Home"),
            CreateCategory(6, "Clothing", null),
        ];
    }

    public static Models.TestItem CreateItem(
        int id = 0,
        decimal weight = 1.5m,
        string tag = "General")
    {
        return new Models.TestItem
        {
            Id = id == 0 ? Random.Shared.Next(1, 10000) : id,
            Weight = weight,
            Tag = tag
        };
    }

    public static List<Models.TestItem> CreateItemList()
    {
        return
        [
            CreateItem(1, 0.5m, "Light"),
            CreateItem(2, 1.2m, "Medium"),
            CreateItem(3, 3.5m, "Heavy"),
            CreateItem(4, 7.0m, "VeryHeavy"),
            CreateItem(5, 0.8m, "Light"),
            CreateItem(6, 2.1m, "Medium"),
        ];
    }
}
