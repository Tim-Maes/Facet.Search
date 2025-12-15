using System;

namespace Facet.Search.Generators.Models;

internal sealed record PropertyInfo(
    string Name,
    string Type,
    float Weight = 1.0f,
    bool CaseSensitive = false,
    string Behavior = "Contains",
    bool Sortable = true
) : IEquatable<PropertyInfo>;
