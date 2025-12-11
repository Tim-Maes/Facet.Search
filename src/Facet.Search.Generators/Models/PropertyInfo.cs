using Microsoft.CodeAnalysis;

namespace Facet.Search.Generators.Models;

/// <summary>
/// Basic property information.
/// </summary>
internal class PropertyInfo
{
    public string Name { get; set; } = null!;
    public string Type { get; set; } = null!;
    public AttributeData? Attribute { get; set; }
}
