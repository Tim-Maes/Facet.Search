using Facet.Search.Tests.Models.Search;

namespace Facet.Search.Tests;

/// <summary>
/// Tests for generated filter classes using actual generated code.
/// </summary>
public class FilterClassGeneratorTests
{
    [Fact]
    public void FilterClass_HasSearchTextProperty()
    {
        // Arrange
        var filter = new TestProductSearchFilter();

        // Act & Assert
        Assert.Null(filter.SearchText);
        filter.SearchText = "test";
        Assert.Equal("test", filter.SearchText);
    }

    [Fact]
    public void FilterClass_HasCategoricalArrayProperties()
    {
        // Arrange
        var filter = new TestProductSearchFilter();

        // Act & Assert
        Assert.Null(filter.Brand);
        filter.Brand = ["Apple", "Samsung"];
        Assert.Equal(2, filter.Brand.Length);
    }

    [Fact]
    public void FilterClass_HasRangeMinMaxProperties()
    {
        // Arrange
        var filter = new TestProductSearchFilter();

        // Act & Assert
        Assert.Null(filter.MinPrice);
        Assert.Null(filter.MaxPrice);
        
        filter.MinPrice = 100m;
        filter.MaxPrice = 500m;
        
        Assert.Equal(100m, filter.MinPrice);
        Assert.Equal(500m, filter.MaxPrice);
    }

    [Fact]
    public void FilterClass_HasNullableBooleanProperty()
    {
        // Arrange
        var filter = new TestProductSearchFilter();

        // Act & Assert
        Assert.Null(filter.InStock);
        
        filter.InStock = true;
        Assert.True(filter.InStock);
        
        filter.InStock = false;
        Assert.False(filter.InStock);
    }

    [Fact]
    public void FilterClass_HasDateRangeFromToProperties()
    {
        // Arrange
        var filter = new TestProductSearchFilter();
        var from = DateTime.Now.AddDays(-30);
        var to = DateTime.Now;

        // Act & Assert
        Assert.Null(filter.CreatedAtFrom);
        Assert.Null(filter.CreatedAtTo);
        
        filter.CreatedAtFrom = from;
        filter.CreatedAtTo = to;
        
        Assert.Equal(from, filter.CreatedAtFrom);
        Assert.Equal(to, filter.CreatedAtTo);
    }

    [Fact]
    public void FilterClass_CanBeInitializedWithObjectInitializer()
    {
        // Arrange & Act
        var filter = new TestProductSearchFilter
        {
            Brand = ["Apple"],
            Category = ["Electronics"],
            MinPrice = 100m,
            MaxPrice = 1000m,
            InStock = true,
            SearchText = "phone",
            CreatedAtFrom = DateTime.Now.AddDays(-30),
            CreatedAtTo = DateTime.Now
        };

        // Assert
        Assert.Equal(["Apple"], filter.Brand);
        Assert.Equal(["Electronics"], filter.Category);
        Assert.Equal(100m, filter.MinPrice);
        Assert.Equal(1000m, filter.MaxPrice);
        Assert.True(filter.InStock);
        Assert.Equal("phone", filter.SearchText);
        Assert.NotNull(filter.CreatedAtFrom);
        Assert.NotNull(filter.CreatedAtTo);
    }

    [Fact]
    public void FilterClass_AllPropertiesAreNullable()
    {
        // Arrange
        var filter = new TestProductSearchFilter();

        // Assert - all properties should be nullable and default to null
        Assert.Null(filter.Brand);
        Assert.Null(filter.Category);
        Assert.Null(filter.MinPrice);
        Assert.Null(filter.MaxPrice);
        Assert.Null(filter.InStock);
        Assert.Null(filter.SearchText);
        Assert.Null(filter.CreatedAtFrom);
        Assert.Null(filter.CreatedAtTo);
    }

    [Fact]
    public void FilterClass_ArticleFilter_HasAuthorProperty()
    {
        // Arrange
        var filter = new TestArticleSearchFilter();

        // Act & Assert
        Assert.Null(filter.Author);
        filter.Author = ["John", "Jane"];
        Assert.Equal(2, filter.Author.Length);
    }

    [Fact]
    public void FilterClass_CategoryFilter_HasHierarchicalPathProperty()
    {
        // Arrange
        var filter = new TestCategorySearchFilter();

        // Act & Assert
        Assert.Null(filter.Path);
        filter.Path = ["Electronics", "Computers"];
        Assert.Equal(2, filter.Path.Length);
    }

    [Fact]
    public void FilterClass_ItemFilter_HasRangeProperties()
    {
        // Arrange
        var filter = new TestItemSearchFilter();

        // Act & Assert
        Assert.Null(filter.MinWeight);
        Assert.Null(filter.MaxWeight);
        
        filter.MinWeight = 1.0m;
        filter.MaxWeight = 10.0m;
        
        Assert.Equal(1.0m, filter.MinWeight);
        Assert.Equal(10.0m, filter.MaxWeight);
    }
}
