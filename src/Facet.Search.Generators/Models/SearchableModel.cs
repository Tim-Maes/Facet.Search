using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace Facet.Search.Generators.Models;

/// <summary>
/// Represents a class marked with [FacetedSearch] and its searchable properties.
/// </summary>
internal class SearchableModel
{
    public string ClassName { get; set; } = null!;
    public string Namespace { get; set; } = null!;
    public string FilterClassName { get; set; } = null!;
    public bool GenerateAggregations { get; set; }
    public bool GenerateMetadata { get; set; }
    public List<SearchFacetInfo> Facets { get; set; } = new();
    public List<PropertyInfo> FullTextProperties { get; set; } = new();
    public List<PropertyInfo> SearchableProperties { get; set; } = new();

    /// <summary>
    /// Creates a SearchableModel from the class symbol and its [FacetedSearch] attribute.
    /// Optimized for use with ForAttributeWithMetadataName.
    /// </summary>
    public static SearchableModel Create(INamedTypeSymbol classSymbol, AttributeData facetedSearchAttr)
    {
        var model = new SearchableModel
        {
            ClassName = classSymbol.Name,
            Namespace = classSymbol.ContainingNamespace.ToDisplayString()
        };

        // Extract attribute properties from the provided attribute
        model.FilterClassName = GetNamedArgument<string>(facetedSearchAttr, "FilterClassName")
            ?? $"{model.ClassName}SearchFilter";
        model.GenerateAggregations = GetNamedArgument<bool>(facetedSearchAttr, "GenerateAggregations", true);
        model.GenerateMetadata = GetNamedArgument<bool>(facetedSearchAttr, "GenerateMetadata", true);

        var customNamespace = GetNamedArgument<string>(facetedSearchAttr, "Namespace");
        if (!string.IsNullOrEmpty(customNamespace))
            model.Namespace = customNamespace!;

        // Collect all searchable properties
        foreach (var member in classSymbol.GetMembers().OfType<IPropertySymbol>())
        {
            var searchFacetAttr = member.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.Name == "SearchFacetAttribute");

            var fullTextAttr = member.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.Name == "FullTextSearchAttribute");

            var searchableAttr = member.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.Name == "SearchableAttribute");

            if (searchFacetAttr != null)
            {
                model.Facets.Add(SearchFacetInfo.Create(member, searchFacetAttr));
            }
            else if (fullTextAttr != null)
            {
                model.FullTextProperties.Add(new PropertyInfo
                {
                    Name = member.Name,
                    Type = member.Type.ToDisplayString(),
                    Attribute = fullTextAttr
                });
            }
            else if (searchableAttr != null)
            {
                model.SearchableProperties.Add(new PropertyInfo
                {
                    Name = member.Name,
                    Type = member.Type.ToDisplayString(),
                    Attribute = searchableAttr
                });
            }
        }

        return model;
    }

    private static T? GetNamedArgument<T>(AttributeData attribute, string name, T? defaultValue = default)
    {
        var arg = attribute.NamedArguments.FirstOrDefault(na => na.Key == name);
        return arg.Value.Value is T value ? value : defaultValue;
    }
}
