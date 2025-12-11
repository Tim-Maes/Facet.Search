using Facet.Search.Tests.Models;
using Facet.Search.Tests.Models.Search;

namespace Facet.Search.Tests;

/// <summary>
/// Tests for generated filter classes.
/// </summary>
public class FilterClassGeneratorTests
{
    [Fact]
    public void FilterClass_ShouldBeGenerated()
    {
        // Arrange & Act
        var filter = new TestProductSearchFilter();

        // Assert
        Assert.NotNull(filter);
    }

    [Fact]
    public void FilterClass_ShouldHaveCategoricalProperties()
    {
        // Arrange
        var filter = new TestProductSearchFilter();

        // Act
        filter.Brand = ["Apple", "Samsung"];
        filter.Category = ["Electronics"];

        // Assert
        Assert.Equal(2, filter.Brand?.Length);
        Assert.Single(filter.Category!);
    }

    [Fact]
    public void FilterClass_ShouldHaveRangeProperties()
    {
        // Arrange
        var filter = new TestProductSearchFilter();

        // Act
        filter.MinPrice = 10.0m;
        filter.MaxPrice = 100.0m;

        // Assert
        Assert.Equal(10.0m, filter.MinPrice);
        Assert.Equal(100.0m, filter.MaxPrice);
    }

    [Fact]
    public void FilterClass_ShouldHaveBooleanProperty()
    {
        // Arrange
        var filter = new TestProductSearchFilter();

        // Act
        filter.InStock = true;

        // Assert
        Assert.True(filter.InStock);
    }

    [Fact]
    public void FilterClass_ShouldHaveDateRangeProperties()
    {
        // Arrange
        var filter = new TestProductSearchFilter();
        var fromDate = new DateTime(2024, 1, 1);
        var toDate = new DateTime(2024, 12, 31);

        // Act
        filter.CreatedAtFrom = fromDate;
        filter.CreatedAtTo = toDate;

        // Assert
        Assert.Equal(fromDate, filter.CreatedAtFrom);
        Assert.Equal(toDate, filter.CreatedAtTo);
    }

    [Fact]
    public void FilterClass_ShouldHaveFullTextSearchProperty()
    {
        // Arrange
        var filter = new TestProductSearchFilter();

        // Act
        filter.SearchText = "laptop";

        // Assert
        Assert.Equal("laptop", filter.SearchText);
    }
}
