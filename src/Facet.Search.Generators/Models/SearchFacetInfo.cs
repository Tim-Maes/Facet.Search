using Microsoft.CodeAnalysis;
using System;
using System.Linq;

namespace Facet.Search.Generators.Models;

internal sealed record SearchFacetInfo(
    string PropertyName,
    string PropertyType,
    string FacetType,
    string? DisplayName,
    string OrderBy,
    int Limit,
    string? DependsOn,
    bool IsHierarchical,
    string RangeAggregation,
    string? RangeIntervals
) : IEquatable<SearchFacetInfo>
{
    public static SearchFacetInfo Create(IPropertySymbol property, AttributeData attribute)
    {
        var propertyName = property.Name;
        var propertyType = property.Type.ToDisplayString();
        var facetType = "Categorical";
        string? displayName = null;
        var orderBy = "Count";
        var limit = 0;
        string? dependsOn = null;
        var isHierarchical = false;
        var rangeAggregation = "Auto";
        string? rangeIntervals = null;

        foreach (var namedArg in attribute.NamedArguments)
        {
            switch (namedArg.Key)
            {
                case "Type":
                    facetType = GetEnumName(namedArg.Value) ?? "Categorical";
                    break;
                case "DisplayName":
                    displayName = namedArg.Value.Value?.ToString();
                    break;
                case "OrderBy":
                    orderBy = GetEnumName(namedArg.Value) ?? "Count";
                    break;
                case "Limit":
                    limit = (int)(namedArg.Value.Value ?? 0);
                    break;
                case "DependsOn":
                    dependsOn = namedArg.Value.Value?.ToString();
                    break;
                case "IsHierarchical":
                    isHierarchical = (bool)(namedArg.Value.Value ?? false);
                    break;
                case "RangeAggregation":
                    rangeAggregation = GetEnumName(namedArg.Value) ?? "Auto";
                    break;
                case "RangeIntervals":
                    rangeIntervals = namedArg.Value.Value?.ToString();
                    break;
            }
        }

        return new SearchFacetInfo(
            propertyName, propertyType, facetType, displayName, orderBy,
            limit, dependsOn, isHierarchical, rangeAggregation, rangeIntervals);
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

    public bool IsNumericType() =>
        PropertyType is "int" or "long" or "decimal" or "double" or "float"
            or "System.Int32" or "System.Int64" or "System.Decimal" or "System.Double" or "System.Single";
}
