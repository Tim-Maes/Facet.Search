using Facet.Search.Tests.Models;
using Facet.Search.Tests.Models.Search;
using Facet.Search.Tests.Utilities;

namespace Facet.Search.Tests;

/// <summary>
/// Tests for generated search extension methods using actual generated code.
/// </summary>
public class SearchExtensionsTests
{
    [Fact]
    public void ApplyFacetedSearch_WithNullFilter_ReturnsAllItems()
    {
        // Arrange
        var products = TestDataFactory.CreateProductList().AsQueryable();

        // Act
        var results = products.ApplyFacetedSearch(null!).ToList();

        // Assert
        Assert.Equal(8, results.Count);
    }

    [Fact]
    public void ApplyFacetedSearch_WithEmptyFilter_ReturnsAllItems()
    {
        // Arrange
        var products = TestDataFactory.CreateProductList().AsQueryable();
        var filter = new TestProductSearchFilter();

        // Act
        var results = products.ApplyFacetedSearch(filter).ToList();

        // Assert
        Assert.Equal(8, results.Count);
    }

    [Fact]
    public void ApplyFacetedSearch_WithSingleBrand_FiltersCorrectly()
    {
        // Arrange
        var products = TestDataFactory.CreateProductList().AsQueryable();
        var filter = new TestProductSearchFilter
        {
            Brand = ["TechCorp"]
        };

        // Act
        var results = products.ApplyFacetedSearch(filter).ToList();

        // Assert
        Assert.Equal(3, results.Count);
        Assert.All(results, p => Assert.Equal("TechCorp", p.Brand));
    }

    [Fact]
    public void ApplyFacetedSearch_WithMultipleBrands_FiltersCorrectly()
    {
        // Arrange
        var products = TestDataFactory.CreateProductList().AsQueryable();
        var filter = new TestProductSearchFilter
        {
            Brand = ["TechCorp", "GameTech"]
        };

        // Act
        var results = products.ApplyFacetedSearch(filter).ToList();

        // Assert
        Assert.Equal(5, results.Count);
        Assert.All(results, p => Assert.True(p.Brand == "TechCorp" || p.Brand == "GameTech"));
    }

    [Fact]
    public void ApplyFacetedSearch_WithMinPrice_FiltersCorrectly()
    {
        // Arrange
        var products = TestDataFactory.CreateProductList().AsQueryable();
        var filter = new TestProductSearchFilter
        {
            MinPrice = 100m
        };

        // Act
        var results = products.ApplyFacetedSearch(filter).ToList();

        // Assert
        Assert.All(results, p => Assert.True(p.Price >= 100m));
    }

    [Fact]
    public void ApplyFacetedSearch_WithMaxPrice_FiltersCorrectly()
    {
        // Arrange
        var products = TestDataFactory.CreateProductList().AsQueryable();
        var filter = new TestProductSearchFilter
        {
            MaxPrice = 100m
        };

        // Act
        var results = products.ApplyFacetedSearch(filter).ToList();

        // Assert
        Assert.All(results, p => Assert.True(p.Price <= 100m));
    }

    [Fact]
    public void ApplyFacetedSearch_WithPriceRange_FiltersCorrectly()
    {
        // Arrange
        var products = TestDataFactory.CreateProductList().AsQueryable();
        var filter = new TestProductSearchFilter
        {
            MinPrice = 100m,
            MaxPrice = 500m
        };

        // Act
        var results = products.ApplyFacetedSearch(filter).ToList();

        // Assert
        Assert.All(results, p => Assert.InRange(p.Price, 100m, 500m));
    }

    [Fact]
    public void ApplyFacetedSearch_WithInStockTrue_FiltersCorrectly()
    {
        // Arrange
        var products = TestDataFactory.CreateProductList().AsQueryable();
        var filter = new TestProductSearchFilter
        {
            InStock = true
        };

        // Act
        var results = products.ApplyFacetedSearch(filter).ToList();

        // Assert
        Assert.All(results, p => Assert.True(p.InStock));
    }

    [Fact]
    public void ApplyFacetedSearch_WithInStockFalse_FiltersCorrectly()
    {
        // Arrange
        var products = TestDataFactory.CreateProductList().AsQueryable();
        var filter = new TestProductSearchFilter
        {
            InStock = false
        };

        // Act
        var results = products.ApplyFacetedSearch(filter).ToList();

        // Assert
        Assert.All(results, p => Assert.False(p.InStock));
    }

    [Fact]
    public void ApplyFacetedSearch_WithCategory_FiltersCorrectly()
    {
        // Arrange
        var products = TestDataFactory.CreateProductList().AsQueryable();
        var filter = new TestProductSearchFilter
        {
            Category = ["Electronics"]
        };

        // Act
        var results = products.ApplyFacetedSearch(filter).ToList();

        // Assert
        Assert.All(results, p => Assert.Equal("Electronics", p.Category));
    }

    [Fact]
    public void ApplyFacetedSearch_WithDateRangeFrom_FiltersCorrectly()
    {
        // Arrange
        var products = TestDataFactory.CreateProductList().AsQueryable();
        var fromDate = DateTime.Now.AddDays(-15);
        var filter = new TestProductSearchFilter
        {
            CreatedAtFrom = fromDate
        };

        // Act
        var results = products.ApplyFacetedSearch(filter).ToList();

        // Assert
        Assert.All(results, p => Assert.True(p.CreatedAt >= fromDate));
    }

    [Fact]
    public void ApplyFacetedSearch_WithDateRangeTo_FiltersCorrectly()
    {
        // Arrange
        var products = TestDataFactory.CreateProductList().AsQueryable();
        var toDate = DateTime.Now.AddDays(-10);
        var filter = new TestProductSearchFilter
        {
            CreatedAtTo = toDate
        };

        // Act
        var results = products.ApplyFacetedSearch(filter).ToList();

        // Assert
        Assert.All(results, p => Assert.True(p.CreatedAt <= toDate));
    }

    [Fact]
    public void ApplyFacetedSearch_WithFullTextSearch_MatchesName()
    {
        // Arrange
        var products = TestDataFactory.CreateProductList().AsQueryable();
        var filter = new TestProductSearchFilter
        {
            SearchText = "laptop"
        };

        // Act
        var results = products.ApplyFacetedSearch(filter).ToList();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.All(results, p => Assert.Contains("Laptop", p.Name, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ApplyFacetedSearch_WithFullTextSearch_MatchesDescription()
    {
        // Arrange
        var products = TestDataFactory.CreateProductList().AsQueryable();
        var filter = new TestProductSearchFilter
        {
            SearchText = "ergonomic"
        };

        // Act
        var results = products.ApplyFacetedSearch(filter).ToList();

        // Assert
        Assert.Single(results);
        Assert.Contains("ergonomic", results[0].Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ApplyFacetedSearch_WithFullTextSearch_CaseInsensitive()
    {
        // Arrange
        var products = TestDataFactory.CreateProductList().AsQueryable();
        var filter = new TestProductSearchFilter
        {
            SearchText = "LAPTOP"
        };

        // Act
        var results = products.ApplyFacetedSearch(filter).ToList();

        // Assert
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public void ApplyFacetedSearch_WithCombinedFilters_AppliesAll()
    {
        // Arrange
        var products = TestDataFactory.CreateProductList().AsQueryable();
        var filter = new TestProductSearchFilter
        {
            Category = ["Electronics"],
            InStock = true,
            MaxPrice = 200m
        };

        // Act
        var results = products.ApplyFacetedSearch(filter).ToList();

        // Assert
        Assert.All(results, p =>
        {
            Assert.Equal("Electronics", p.Category);
            Assert.True(p.InStock);
            Assert.True(p.Price <= 200m);
        });
    }

    [Fact]
    public void ApplyFacetedSearch_WithNoMatchingResults_ReturnsEmpty()
    {
        // Arrange
        var products = TestDataFactory.CreateProductList().AsQueryable();
        var filter = new TestProductSearchFilter
        {
            Brand = ["NonExistentBrand"]
        };

        // Act
        var results = products.ApplyFacetedSearch(filter).ToList();

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void ApplyFacetedSearch_WithEmptyBrandArray_ReturnsAllItems()
    {
        // Arrange
        var products = TestDataFactory.CreateProductList().AsQueryable();
        var filter = new TestProductSearchFilter
        {
            Brand = []
        };

        // Act
        var results = products.ApplyFacetedSearch(filter).ToList();

        // Assert
        Assert.Equal(8, results.Count);
    }

    [Fact]
    public void ApplyFacetedSearch_WithWhitespaceSearchText_ReturnsAllItems()
    {
        // Arrange
        var products = TestDataFactory.CreateProductList().AsQueryable();
        var filter = new TestProductSearchFilter
        {
            SearchText = "   "
        };

        // Act
        var results = products.ApplyFacetedSearch(filter).ToList();

        // Assert
        Assert.Equal(8, results.Count);
    }

    [Fact]
    public void ApplyFacetedSearch_WithSortByPrice_SortsAscending()
    {
        // Arrange
        var products = TestDataFactory.CreateProductList().AsQueryable();
        var filter = new TestProductSearchFilter
        {
            SortBy = "Price",
            SortDescending = false
        };

        // Act
        var results = products.ApplyFacetedSearch(filter).ToList();

        // Assert
        Assert.Equal(8, results.Count);
        for (int i = 0; i < results.Count - 1; i++)
        {
            Assert.True(results[i].Price <= results[i + 1].Price,
                $"Expected ascending order but {results[i].Price} > {results[i + 1].Price}");
        }
    }

    [Fact]
    public void ApplyFacetedSearch_WithSortByPriceDescending_SortsDescending()
    {
        // Arrange
        var products = TestDataFactory.CreateProductList().AsQueryable();
        var filter = new TestProductSearchFilter
        {
            SortBy = "Price",
            SortDescending = true
        };

        // Act
        var results = products.ApplyFacetedSearch(filter).ToList();

        // Assert
        Assert.Equal(8, results.Count);
        for (int i = 0; i < results.Count - 1; i++)
        {
            Assert.True(results[i].Price >= results[i + 1].Price,
                $"Expected descending order but {results[i].Price} < {results[i + 1].Price}");
        }
    }

    [Fact]
    public void ApplyFacetedSearch_WithSortByBrand_SortsAlphabetically()
    {
        // Arrange
        var products = TestDataFactory.CreateProductList().AsQueryable();
        var filter = new TestProductSearchFilter
        {
            SortBy = "Brand",
            SortDescending = false
        };

        // Act
        var results = products.ApplyFacetedSearch(filter).ToList();

        // Assert
        Assert.Equal(8, results.Count);
        for (int i = 0; i < results.Count - 1; i++)
        {
            Assert.True(string.Compare(results[i].Brand, results[i + 1].Brand, StringComparison.Ordinal) <= 0,
                $"Expected alphabetical order but '{results[i].Brand}' > '{results[i + 1].Brand}'");
        }
    }

    [Fact]
    public void ApplyFacetedSearch_WithSortByRating_SortsCorrectly()
    {
        // Arrange
        var products = TestDataFactory.CreateProductList().AsQueryable();
        var filter = new TestProductSearchFilter
        {
            SortBy = "Rating",
            SortDescending = false
        };

        // Act
        var results = products.ApplyFacetedSearch(filter).ToList();

        // Assert
        Assert.Equal(8, results.Count);
        for (int i = 0; i < results.Count - 1; i++)
        {
            Assert.True(results[i].Rating <= results[i + 1].Rating);
        }
    }

    [Fact]
    public void ApplyFacetedSearch_WithInvalidSortProperty_IgnoresSorting()
    {
        // Arrange
        var products = TestDataFactory.CreateProductList().AsQueryable();
        var originalOrder = products.ToList();
        var filter = new TestProductSearchFilter
        {
            SortBy = "InvalidProperty",
            SortDescending = false
        };

        // Act
        var results = products.ApplyFacetedSearch(filter).ToList();

        // Assert - Should return all items in original order (lenient validation)
        Assert.Equal(8, results.Count);
        Assert.Equal(originalOrder, results);
    }

    [Fact]
    public void ApplyFacetedSearch_WithNullSortBy_DoesNotSort()
    {
        // Arrange
        var products = TestDataFactory.CreateProductList().AsQueryable();
        var originalOrder = products.ToList();
        var filter = new TestProductSearchFilter
        {
            SortBy = null,
            SortDescending = false
        };

        // Act
        var results = products.ApplyFacetedSearch(filter).ToList();

        // Assert
        Assert.Equal(8, results.Count);
        Assert.Equal(originalOrder, results);
    }

    [Fact]
    public void ApplyFacetedSearch_WithEmptySortBy_DoesNotSort()
    {
        // Arrange
        var products = TestDataFactory.CreateProductList().AsQueryable();
        var originalOrder = products.ToList();
        var filter = new TestProductSearchFilter
        {
            SortBy = "",
            SortDescending = false
        };

        // Act
        var results = products.ApplyFacetedSearch(filter).ToList();

        // Assert
        Assert.Equal(8, results.Count);
        Assert.Equal(originalOrder, results);
    }

    [Fact]
    public void ApplyFacetedSearch_WithSortingAndFilters_AppliesBoth()
    {
        // Arrange
        var products = TestDataFactory.CreateProductList().AsQueryable();
        var filter = new TestProductSearchFilter
        {
            Category = ["Electronics"],
            SortBy = "Price",
            SortDescending = false
        };

        // Act
        var results = products.ApplyFacetedSearch(filter).ToList();

        // Assert - All should be Electronics and sorted by price
        Assert.All(results, p => Assert.Equal("Electronics", p.Category));
        for (int i = 0; i < results.Count - 1; i++)
        {
            Assert.True(results[i].Price <= results[i + 1].Price);
        }
    }
}
