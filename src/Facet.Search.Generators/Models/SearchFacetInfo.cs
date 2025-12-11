using Microsoft.CodeAnalysis;
using System.Linq;

namespace Facet.Search.Generators.Models;

/// <summary>
/// Information about a property marked with [SearchFacet].
/// </summary>
internal class SearchFacetInfo
{
    public string PropertyName { get; set; } = null!;
    public string PropertyType { get; set; } = null!;
    public string FacetType { get; set; } = "Categorical";
    public string? DisplayName { get; set; }
    public string OrderBy { get; set; } = "Count";
    public int Limit { get; set; }
    public string? DependsOn { get; set; }
    public bool IsHierarchical { get; set; }
    public string RangeAggregation { get; set; } = "Auto";
    public string? RangeIntervals { get; set; }

    public static SearchFacetInfo Create(IPropertySymbol property, AttributeData attribute)
    {
        var info = new SearchFacetInfo
        {
            PropertyName = property.Name,
            PropertyType = property.Type.ToDisplayString()
        };

        // Extract attribute values
        foreach (var namedArg in attribute.NamedArguments)
        {
            switch (namedArg.Key)
            {
                case "Type":
                    // Enum values come as integers, get the name from the type
                    info.FacetType = GetEnumName(namedArg.Value) ?? "Categorical";
                    break;
                case "DisplayName":
                    info.DisplayName = namedArg.Value.Value?.ToString();
                    break;
                case "OrderBy":
                    info.OrderBy = GetEnumName(namedArg.Value) ?? "Count";
                    break;
                case "Limit":
                    info.Limit = (int)(namedArg.Value.Value ?? 0);
                    break;
                case "DependsOn":
                    info.DependsOn = namedArg.Value.Value?.ToString();
                    break;
                case "IsHierarchical":
                    info.IsHierarchical = (bool)(namedArg.Value.Value ?? false);
                    break;
                case "RangeAggregation":
                    info.RangeAggregation = GetEnumName(namedArg.Value) ?? "Auto";
                    break;
                case "RangeIntervals":
                    info.RangeIntervals = namedArg.Value.Value?.ToString();
                    break;
            }
        }

        return info;
    }

    /// <summary>
    /// Gets the enum member name from a TypedConstant representing an enum value.
    /// </summary>
    private static string? GetEnumName(TypedConstant constant)
    {
        if (constant.Type is INamedTypeSymbol enumType && enumType.TypeKind == TypeKind.Enum)
        {
            // Get the enum value
            var value = constant.Value;
            if (value != null)
            {
                // Find the member with this value
                foreach (var member in enumType.GetMembers().OfType<IFieldSymbol>())
                {
                    if (member.HasConstantValue && member.ConstantValue?.Equals(value) == true)
                    {
                        return member.Name;
                    }
                }
            }
        }
        return constant.Value?.ToString();
    }

    /// <summary>
    /// Get the C# type for this facet in the filter class.
    /// </summary>
    public string GetFilterPropertyType()
    {
        return FacetType switch
        {
            "Categorical" => $"string[]?",
            "Range" when IsNumericType() => $"({PropertyType}? Min, {PropertyType}? Max)?",
            "Boolean" => "bool?",
            "DateRange" => "(System.DateTime? From, System.DateTime? To)?",
            "Hierarchical" => "string[]?",
            "Geo" => "(double Latitude, double Longitude, double? RadiusKm)?",
            _ => $"{PropertyType}?"
        };
    }

    /// <summary>
    /// Get the filter property name(s).
    /// </summary>
    public string[] GetFilterPropertyNames()
    {
        return FacetType switch
        {
            "Range" => new[] { $"Min{PropertyName}", $"Max{PropertyName}" },
            "DateRange" => new[] { $"{PropertyName}From", $"{PropertyName}To" },
            "Geo" => new[] { $"{PropertyName}Latitude", $"{PropertyName}Longitude", $"{PropertyName}RadiusKm" },
            _ => new[] { PropertyName }
        };
    }

    private bool IsNumericType()
    {
        return PropertyType is "int" or "long" or "decimal" or "double" or "float"
            or "System.Int32" or "System.Int64" or "System.Decimal" or "System.Double" or "System.Single";
    }
}
