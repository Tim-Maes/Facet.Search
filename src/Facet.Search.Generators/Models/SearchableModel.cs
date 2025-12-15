using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Facet.Search.Generators.Models;

internal sealed record SearchableModel(
    string ClassName,
    string Namespace,
    string FilterClassName,
    bool GenerateAggregations,
    bool GenerateMetadata,
    string FullTextStrategy,
    EquatableArray<SearchFacetInfo> Facets,
    EquatableArray<PropertyInfo> FullTextProperties,
    EquatableArray<PropertyInfo> SearchableProperties
) : IEquatable<SearchableModel>
{
    /// <summary>
    /// Gets all unique navigation property paths that require Include() calls.
    /// </summary>
    public IEnumerable<string> GetRequiredIncludes()
    {
        var includes = new HashSet<string>();

        // From facets with NavigationPath
        foreach (var facet in Facets)
        {
            if (facet.RequiresInclude && facet.RootNavigationProperty != null)
            {
                includes.Add(facet.RootNavigationProperty);
            }
        }

        return includes;
    }

    public static SearchableModel Create(INamedTypeSymbol classSymbol, AttributeData facetedSearchAttr)
    {
        var className = classSymbol.Name;
        var ns = classSymbol.ContainingNamespace.ToDisplayString();

        var filterClassName = GetNamedArgument<string>(facetedSearchAttr, "FilterClassName")
            ?? $"{className}SearchFilter";
        var generateAggregations = GetNamedArgument<bool>(facetedSearchAttr, "GenerateAggregations", true);
        var generateMetadata = GetNamedArgument<bool>(facetedSearchAttr, "GenerateMetadata", true);
        var fullTextStrategy = GetEnumArgument(facetedSearchAttr, "FullTextStrategy") ?? "LinqContains";

        var customNamespace = GetNamedArgument<string>(facetedSearchAttr, "Namespace");
        if (!string.IsNullOrEmpty(customNamespace))
            ns = customNamespace!;

        var facetsBuilder = ImmutableArray.CreateBuilder<SearchFacetInfo>();
        var fullTextBuilder = ImmutableArray.CreateBuilder<PropertyInfo>();
        var searchableBuilder = ImmutableArray.CreateBuilder<PropertyInfo>();

        foreach (var member in classSymbol.GetMembers().OfType<IPropertySymbol>())
        {
            var searchFacetAttr = member.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.Name == "SearchFacetAttribute");
            var fullTextAttr = member.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.Name == "FullTextSearchAttribute");
            var searchableAttr = member.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.Name == "SearchableAttribute");

            if (searchFacetAttr != null)
                facetsBuilder.Add(SearchFacetInfo.Create(member, searchFacetAttr));
            
            if (fullTextAttr != null)
                fullTextBuilder.Add(CreateFullTextPropertyInfo(member, fullTextAttr));
            else if (searchableAttr != null)
                searchableBuilder.Add(CreateSearchablePropertyInfo(member, searchableAttr));
        }

        return new SearchableModel(
            className, ns, filterClassName, generateAggregations, generateMetadata, fullTextStrategy,
            facetsBuilder.ToImmutable().ToEquatableArray(),
            fullTextBuilder.ToImmutable().ToEquatableArray(),
            searchableBuilder.ToImmutable().ToEquatableArray());
    }

    private static PropertyInfo CreateFullTextPropertyInfo(IPropertySymbol property, AttributeData attribute)
    {
        var weight = 1.0f;
        var caseSensitive = false;
        var behavior = "Contains";

        foreach (var namedArg in attribute.NamedArguments)
        {
            switch (namedArg.Key)
            {
                case "Weight":
                    weight = (float)(namedArg.Value.Value ?? 1.0f);
                    break;
                case "CaseSensitive":
                    caseSensitive = (bool)(namedArg.Value.Value ?? false);
                    break;
                case "Behavior":
                    behavior = GetEnumName(namedArg.Value) ?? "Contains";
                    break;
            }
        }

        return new PropertyInfo(property.Name, property.Type.ToDisplayString(), weight, caseSensitive, behavior, false);
    }

    private static PropertyInfo CreateSearchablePropertyInfo(IPropertySymbol property, AttributeData attribute)
    {
        var sortable = true;
        foreach (var namedArg in attribute.NamedArguments)
        {
            if (namedArg.Key == "Sortable")
                sortable = (bool)(namedArg.Value.Value ?? true);
        }

        return new PropertyInfo(property.Name, property.Type.ToDisplayString(), 1.0f, false, "Contains", sortable);
    }

    private static T? GetNamedArgument<T>(AttributeData attribute, string name, T? defaultValue = default)
    {
        var arg = attribute.NamedArguments.FirstOrDefault(na => na.Key == name);
        return arg.Value.Value is T value ? value : defaultValue;
    }

    private static string? GetEnumArgument(AttributeData attribute, string name)
    {
        var arg = attribute.NamedArguments.FirstOrDefault(na => na.Key == name);
        if (arg.Value.Value == null) return null;
        return GetEnumName(arg.Value);
    }

    private static string? GetEnumName(TypedConstant constant)
    {
        if (constant.Type is INamedTypeSymbol enumType && enumType.TypeKind == TypeKind.Enum)
        {
            var value = constant.Value;
            if (value != null)
            {
                foreach (var member in enumType.GetMembers().OfType<IFieldSymbol>())
                {
                    if (member.HasConstantValue && member.ConstantValue?.Equals(value) == true)
                        return member.Name;
                }
            }
        }
        return constant.Value?.ToString();
    }
}
