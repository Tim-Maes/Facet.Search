# Facet.Search - Build Instructions

## Overview

**Facet.Search** is a source generator-powered faceted search library for .NET that automatically generates type-safe search filters, LINQ queries, and aggregation methods from domain models. It follows the same philosophy as the Facet ecosystem: eliminating boilerplate through compile-time code generation with zero runtime cost.

### Philosophy Alignment with Facet

- **Compile-time over runtime**: All code generation happens during compilation using Roslyn source generators
- **Zero reflection cost**: Everything is strongly typed and generated
- **Single source of truth**: Domain models are the only place you define your search structure
- **Type safety**: Compiler catches errors before runtime
- **Minimal boilerplate**: Declare what you want with attributes; everything else is generated
- **Progressive enhancement**: Start simple, add complexity only when needed

### Package Name

**Facet.Search** - Perfectly aligned with the Facet naming convention and the concept of faceted search/filtering.

---

## Solution Structure

Create the following project structure (following Facet's pattern of separating attributes):

```
Facet.Search.sln
│
├── src/
│   ├── Facet.Search/
│   │   ├── Facet.Search.csproj
│   │   ├── Attributes/
│   │   │   ├── FacetedSearchAttribute.cs
│   │   │   ├── SearchFacetAttribute.cs
│   │   │   ├── FullTextSearchAttribute.cs
│   │   │   └── SearchableAttribute.cs
│   │   └── Enums/
│   │       ├── FacetType.cs
│   │       ├── FacetOrder.cs
│   │       └── RangeAggregation.cs
│   │
│   ├── Facet.Search.Generators/
│   │   ├── Facet.Search.Generators.csproj
│   │   ├── FacetSearchGenerator.cs
│   │   ├── Generators/
│   │   │   ├── FilterClassGenerator.cs
│   │   │   ├── ExtensionMethodsGenerator.cs
│   │   │   ├── AggregationGenerator.cs
│   │   │   └── MetadataGenerator.cs
│   │   ├── Models/
│   │   │   ├── SearchableModel.cs
│   │   │   ├── SearchFacetInfo.cs
│   │   │   └── GenerationContext.cs
│   │   └── Helpers/
│   │       ├── CodeBuilder.cs
│   │       └── NamingHelper.cs
│   │
│   └── Facet.Search.EFCore/
│       ├── Facet.Search.EFCore.csproj
│       └── Extensions/
│           ├── QueryableExtensions.cs
│           └── FacetAggregationExtensions.cs
│
└── tests/
    ├── Facet.Search.Tests/
    │   └── Facet.Search.Tests.csproj
    └── Facet.Search.IntegrationTests/
        └── Facet.Search.IntegrationTests.csproj
```

---

## Project Configurations

### CRITICAL: Package Architecture

**The architecture ensures zero runtime dependencies on Roslyn/analyzers for consumers:**

1. **Facet.Search** (Runtime Package)
   - Contains only attributes, enums, and interfaces
   - NO source generator code
   - NO Roslyn dependencies
   - Consumers reference this directly

2. **Facet.Search.Generators** (Analyzer Package)
   - Contains the source generator
   - References Roslyn packages with `PrivateAssets="all"`
   - Marked as `DevelopmentDependency="true"`
   - Packed as analyzer (not regular dependency)
   - Automatically included when Facet.Search is referenced

3. **Facet.Search.EFCore** (Optional Runtime Package)
   - Contains EF Core extensions
   - References only Facet.Search and EF Core
   - NO generator/Roslyn dependencies

**Consumer's project references:**
```xml
<!-- Consumer only adds these -->
<PackageReference Include="Facet.Search" Version="1.0.0" />
<PackageReference Include="Facet.Search.EFCore" Version="1.0.0" />
<!-- Generator is automatically included, no Roslyn in runtime! -->
```

### Facet.Search.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    
    <!-- NuGet Package Metadata -->
    <PackageId>Facet.Search</PackageId>
    <Version>1.0.0</Version>
    <Authors>Tim Maes</Authors>
    <Description>Attributes and core interfaces for Facet.Search - compile-time faceted search generation</Description>
    <PackageTags>facet;search;filtering;source-generator;dto;linq</PackageTags>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <PackageProjectUrl>https://github.com/Tim-Maes/Facet</PackageProjectUrl>
    <RepositoryUrl>https://github.com/Tim-Maes/Facet</RepositoryUrl>
  </PropertyGroup>

  <!-- NO dependencies - this is intentional! -->
  <!-- Generator will be included automatically via NuGet metadata below -->
  
  <ItemGroup>
    <!-- Include the generator as a development dependency -->
    <!-- This ensures it's available at compile-time but not runtime -->
    <None Include="..\Facet.Search.Generators\bin\$(Configuration)\netstandard2.0\Facet.Search.Generators.dll" 
          Pack="true" 
          PackagePath="analyzers/dotnet/cs" 
          Visible="false" />
  </ItemGroup>
</Project>
```

### Facet.Search.Generators.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <EnforceExtendedAnalyzerRules>true</EnforceExtendedAnalyzerRules>
    <IsRoslynComponent>true</IsRoslynComponent>
    
    <!-- Prevent this from being a regular dependency -->
    <IncludeBuildOutput>false</IncludeBuildOutput>
    <SuppressDependenciesWhenPacking>true</SuppressDependenciesWhenPacking>
    
    <!-- NuGet Package Metadata -->
    <PackageId>Facet.Search.Generators</PackageId>
    <Version>1.0.0</Version>
    <Authors>Tim Maes</Authors>
    <Description>Source generators for Facet.Search - automatic faceted search filter generation (development dependency only)</Description>
    <DevelopmentDependency>true</DevelopmentDependency>
    <PackageTags>facet;search;source-generator;analyzer;roslyn</PackageTags>
  </PropertyGroup>

  <ItemGroup>
    <!-- All Roslyn packages marked as PrivateAssets="all" -->
    <!-- This prevents them from flowing to consumers -->
    <PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.5.0" PrivateAssets="all" />
    <PackageReference Include="Microsoft.CodeAnalysis.Analyzers" Version="3.3.4" PrivateAssets="all" />
  </ItemGroup>

  <ItemGroup>
    <!-- Reference to Facet.Search for attribute types -->
    <!-- IMPORTANT: This is compile-time only, not packed as dependency -->
    <ProjectReference Include="..\Facet.Search\Facet.Search.csproj" PrivateAssets="all" />
  </ItemGroup>

  <!-- Pack the generator DLL in the analyzers folder -->
  <ItemGroup>
    <None Include="$(OutputPath)\$(AssemblyName).dll" 
          Pack="true" 
          PackagePath="analyzers/dotnet/cs" 
          Visible="false" />
    
    <!-- Also include Facet.Search.dll in analyzers folder -->
    <!-- So generator can reference attribute types at compile-time -->
    <None Include="$(OutputPath)\Facet.Search.dll" 
          Pack="true" 
          PackagePath="analyzers/dotnet/cs" 
          Visible="false" />
  </ItemGroup>
</Project>
```

### Facet.Search.EFCore.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    
    <!-- NuGet Package Metadata -->
    <PackageId>Facet.Search.EFCore</PackageId>
    <Version>1.0.0</Version>
    <Authors>Tim Maes</Authors>
    <Description>Entity Framework Core integration for Facet.Search</Description>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="6.0.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Facet.Search\Facet.Search.csproj" />
  </ItemGroup>
</Project>
```

---

## Verifying Package Architecture

### How to Verify No Runtime Pollution

After building and packing, verify the packages are structured correctly:

**1. Check Facet.Search.nupkg:**
```bash
# Extract and inspect
unzip Facet.Search.1.0.0.nupkg -d temp
tree temp

# Should see:
# lib/netstandard2.0/Facet.Search.dll
# analyzers/dotnet/cs/Facet.Search.Generators.dll
# analyzers/dotnet/cs/Facet.Search.dll
# NO Roslyn DLLs anywhere!
```

**2. Check Consumer Project Dependencies:**
```bash
# After referencing Facet.Search in a test project
dotnet build TestProject.csproj
dotnet list TestProject.csproj package

# Should show:
# Facet.Search - 1.0.0
# (Generator is invisible to dotnet list - this is correct!)

# Check bin output
ls TestProject/bin/Debug/net8.0

# Should NOT contain:
# - Microsoft.CodeAnalysis.*.dll
# - Any analyzer/Roslyn dependencies
# Should ONLY contain:
# - Facet.Search.dll (if referenced)
```

**3. Test Consumer Project:**
```csharp
// TestProject/Program.cs
using Facet.Search;

[FacetedSearch]
public class Product
{
    [SearchFacet]
    public string Name { get; set; } = null!;
}

// Build should succeed
// Generated code should appear in obj/Debug/net8.0/generated/
// No Roslyn DLLs in bin/Debug/net8.0/
```

**4. Verify Generated Files Location:**
```bash
# Generated source files appear in obj folder (never deployed)
ls obj/Debug/net8.0/generated/Facet.Search.Generators/

# Should see:
# ProductSearchFilter.g.cs
# ProductSearchExtensions.g.cs
# ProductFacetAggregations.g.cs
# ProductSearchMetadata.g.cs
```

### Common Packaging Mistakes to Avoid

❌ **WRONG - Including Generators as Regular Dependency:**
```xml
<!-- Consumer sees this - BAD! -->
<PackageReference Include="Facet.Search.Generators" Version="1.0.0" />
```

✅ **CORRECT - Generators Hidden as Development Dependency:**
```xml
<!-- Consumer only sees this - GOOD! -->
<PackageReference Include="Facet.Search" Version="1.0.0" />
<!-- Generator automatically included in analyzers/ folder -->
```

❌ **WRONG - Roslyn in Consumer's bin Folder:**
```
bin/Debug/net8.0/
  ├── Facet.Search.dll
  ├── Microsoft.CodeAnalysis.dll  ❌ Should NOT be here!
  ├── Microsoft.CodeAnalysis.CSharp.dll  ❌ Should NOT be here!
```

✅ **CORRECT - Only Runtime Dependencies:**
```
bin/Debug/net8.0/
  ├── MyApp.dll
  ├── Facet.Search.dll  ✓ Only if needed at runtime (attributes)
  └── [other app dependencies]
```

### Alternative Architecture: Single Package (Recommended)

To match Facet's existing pattern and simplify consumer usage, use a single-package approach:

**Single Package Structure:**
```
Facet.Search.nupkg/
  ├── lib/netstandard2.0/
  │   └── Facet.Search.dll (attributes only)
  └── analyzers/dotnet/cs/
      ├── Facet.Search.Generators.dll (generator)
      └── Facet.Search.dll (copy for generator to reference)
```

**Modified Facet.Search.csproj (Single Package):**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    
    <!-- NuGet Package Metadata -->
    <PackageId>Facet.Search</PackageId>
    <Version>1.0.0</Version>
    <Authors>Tim Maes</Authors>
    <Description>Compile-time faceted search generation - attributes and source generators</Description>
    <PackageTags>facet;search;filtering;source-generator;dto;linq</PackageTags>
  </PropertyGroup>

  <!-- Include the generator as analyzer in the same package -->
  <ItemGroup>
    <ProjectReference Include="..\Facet.Search.Generators\Facet.Search.Generators.csproj" 
                      OutputItemType="Analyzer" 
                      ReferenceOutputAssembly="false" />
  </ItemGroup>
</Project>
```

**This approach:**
- ✅ Consumers only reference one package: `Facet.Search`
- ✅ Generator automatically included in analyzers folder
- ✅ Zero Roslyn pollution in consumer's runtime
- ✅ Matches Facet's existing architecture
- ✅ Simpler for users

**Recommendation:** Use the single-package approach - it's cleaner and matches how Facet itself works.

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                         Consumer Project                         │
│                                                                  │
│  [Product.cs]                                                    │
│  [FacetedSearch] ────────┐                                       │
│  public class Product    │                                       │
│  {                       │                                       │
│      [SearchFacet]       │                                       │
│      public string Name  │ At Compile-Time                       │
│  }                       │                                       │
│                          ▼                                       │
│  ┌──────────────────────────────────────┐                       │
│  │  Facet.Search.Generators (Analyzer)  │                       │
│  │  ├─ Reads attributes                 │                       │
│  │  ├─ Generates code                   │                       │
│  │  └─ NOT in bin folder!               │                       │
│  └──────────────────────────────────────┘                       │
│                          │                                       │
│                          ▼                                       │
│  [Generated: ProductSearchFilter.g.cs]                           │
│  [Generated: ProductSearchExtensions.g.cs]                       │
│                                                                  │
│  Only references:                                                │
│  ✅ Facet.Search.dll (attributes - lightweight)                 │
│  ❌ NO Roslyn DLLs                                               │
│  ❌ NO Analyzer DLLs                                             │
└─────────────────────────────────────────────────────────────────┘

Package Structure on Disk:

Facet.Search.nupkg
├── lib/netstandard2.0/
│   └── Facet.Search.dll              ← Runtime: Attributes only (~10KB)
└── analyzers/dotnet/cs/
    ├── Facet.Search.Generators.dll   ← Compile-time only
    ├── Facet.Search.dll              ← Copy for generator reference
    └── (NO Roslyn - PrivateAssets!)  ← Roslyn stays hidden

Consumer's bin folder (Runtime):
├── ConsumerApp.dll
├── Facet.Search.dll                  ← Only this! (~10KB)
└── (Other app dependencies)
    ❌ NO Microsoft.CodeAnalysis.dll
    ❌ NO analyzer assemblies
```

### Key Principles

1. **Separation of Concerns**
   - `Facet.Search` = Runtime attributes (small, always needed)
   - `Facet.Search.Generators` = Compile-time code generation (never deployed)

2. **Zero Runtime Cost**
   - Generated code is plain C#, no magic
   - No reflection, no dynamic proxies
   - Consumer's runtime only has lightweight attribute DLL

3. **Transparent to Consumer**
   - Consumer adds one package: `Facet.Search`
   - Generator works automatically
   - No extra configuration needed

4. **Clean Dependency Tree**
   - Roslyn packages marked `PrivateAssets="all"`
   - Generator marked `DevelopmentDependency="true"`
   - Consumer never sees analyzer infrastructure

---

## Implementation Details

### 1. Facet.Search - Attributes and Enums

#### FacetedSearchAttribute.cs

```csharp
using System;

namespace Facet.Search;

/// <summary>
/// Marks a class as searchable, triggering generation of filter classes and search extensions.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class FacetedSearchAttribute : Attribute
{
    /// <summary>
    /// Name for the generated filter class. Defaults to {ClassName}SearchFilter.
    /// </summary>
    public string? FilterClassName { get; set; }

    /// <summary>
    /// Whether to generate facet aggregation methods.
    /// </summary>
    public bool GenerateAggregations { get; set; } = true;

    /// <summary>
    /// Whether to generate metadata for frontend consumption.
    /// </summary>
    public bool GenerateMetadata { get; set; } = true;

    /// <summary>
    /// Namespace for generated code. Defaults to source class namespace + ".Search".
    /// </summary>
    public string? Namespace { get; set; }
}
```

#### SearchFacetAttribute.cs

```csharp
using System;

namespace Facet.Search;

/// <summary>
/// Marks a property as a searchable facet.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class SearchFacetAttribute : Attribute
{
    /// <summary>
    /// The type of facet (Categorical, Range, Boolean, Date, etc.)
    /// </summary>
    public FacetType Type { get; set; } = FacetType.Categorical;

    /// <summary>
    /// Display name for the facet in UI/metadata.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Order in which facet results should be returned.
    /// </summary>
    public FacetOrder OrderBy { get; set; } = FacetOrder.Count;

    /// <summary>
    /// Maximum number of facet values to return in aggregations.
    /// </summary>
    public int Limit { get; set; } = 0; // 0 = no limit

    /// <summary>
    /// Property name this facet depends on. Only applicable when the dependent property has a value.
    /// </summary>
    public string? DependsOn { get; set; }

    /// <summary>
    /// Whether this facet is hierarchical (e.g., Category > Subcategory).
    /// </summary>
    public bool IsHierarchical { get; set; }

    /// <summary>
    /// For range facets, how to aggregate (Auto, Custom intervals, etc.)
    /// </summary>
    public RangeAggregation RangeAggregation { get; set; } = RangeAggregation.Auto;

    /// <summary>
    /// Custom range intervals for range facets (e.g., "0-50,50-100,100-500,500+")
    /// </summary>
    public string? RangeIntervals { get; set; }
}
```

#### FullTextSearchAttribute.cs

```csharp
using System;

namespace Facet.Search;

/// <summary>
/// Marks a property for full-text search.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class FullTextSearchAttribute : Attribute
{
    /// <summary>
    /// Weight/boost for this field in search relevance (higher = more important).
    /// </summary>
    public float Weight { get; set; } = 1.0f;

    /// <summary>
    /// Whether to use case-sensitive matching.
    /// </summary>
    public bool CaseSensitive { get; set; }

    /// <summary>
    /// Search behavior: Contains, StartsWith, EndsWith, Exact
    /// </summary>
    public TextSearchBehavior Behavior { get; set; } = TextSearchBehavior.Contains;
}
```

#### SearchableAttribute.cs

```csharp
using System;

namespace Facet.Search;

/// <summary>
/// Marks a property as searchable but not a facet (e.g., sortable fields, included in results).
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class SearchableAttribute : Attribute
{
    /// <summary>
    /// Whether this property can be used for sorting.
    /// </summary>
    public bool Sortable { get; set; } = true;
}
```

#### Enums/FacetType.cs

```csharp
namespace Facet.Search;

/// <summary>
/// Defines the type of facet for search filtering.
/// </summary>
public enum FacetType
{
    /// <summary>
    /// Discrete values (e.g., Brand, Category, Status)
    /// Generates: string[]? PropertyName { get; set; }
    /// </summary>
    Categorical,

    /// <summary>
    /// Numeric or date range (e.g., Price, Date)
    /// Generates: Min/Max properties
    /// </summary>
    Range,

    /// <summary>
    /// Boolean flag (e.g., InStock, IsActive)
    /// Generates: bool? PropertyName { get; set; }
    /// </summary>
    Boolean,

    /// <summary>
    /// Date/DateTime with common presets (Today, Last 7 days, etc.)
    /// Generates: DateTime? From/To + DateRangePreset enum
    /// </summary>
    DateRange,

    /// <summary>
    /// Hierarchical categories (e.g., Electronics > Computers > Laptops)
    /// Generates: string[]? PropertyName { get; set; } with path support
    /// </summary>
    Hierarchical,

    /// <summary>
    /// Geographic location with distance filtering
    /// Generates: GeoPoint + Distance properties
    /// </summary>
    Geo
}
```

#### Enums/FacetOrder.cs

```csharp
namespace Facet.Search;

/// <summary>
/// Defines how facet results should be ordered in aggregations.
/// </summary>
public enum FacetOrder
{
    /// <summary>
    /// Order by count (most common first)
    /// </summary>
    Count,

    /// <summary>
    /// Order alphabetically by value
    /// </summary>
    Value,

    /// <summary>
    /// Order by relevance score (for text search)
    /// </summary>
    Relevance,

    /// <summary>
    /// Custom ordering (requires additional configuration)
    /// </summary>
    Custom
}
```

#### Enums/RangeAggregation.cs

```csharp
namespace Facet.Search;

/// <summary>
/// Defines how range facets should be aggregated.
/// </summary>
public enum RangeAggregation
{
    /// <summary>
    /// Automatically determine reasonable intervals
    /// </summary>
    Auto,

    /// <summary>
    /// Use custom intervals defined in attribute
    /// </summary>
    Custom,

    /// <summary>
    /// Fixed-size intervals (e.g., 0-10, 10-20, 20-30)
    /// </summary>
    Fixed,

    /// <summary>
    /// No aggregation, just min/max filtering
    /// </summary>
    None
}
```

#### Enums/TextSearchBehavior.cs

```csharp
namespace Facet.Search;

/// <summary>
/// Defines full-text search behavior.
/// </summary>
public enum TextSearchBehavior
{
    /// <summary>
    /// Contains substring (default)
    /// </summary>
    Contains,

    /// <summary>
    /// Starts with prefix
    /// </summary>
    StartsWith,

    /// <summary>
    /// Ends with suffix
    /// </summary>
    EndsWith,

    /// <summary>
    /// Exact match
    /// </summary>
    Exact
}
```

---

### 2. Facet.Search.Generators - Source Generator Implementation

#### FacetSearchGenerator.cs (Main Entry Point)

```csharp
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Facet.Search.Generators;

[Generator]
public class FacetSearchGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Find all classes with [FacetedSearch] attribute
        var searchableClasses = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null);

        // Combine with compilation
        var compilationAndClasses = context.CompilationProvider.Combine(searchableClasses.Collect());

        // Generate code
        context.RegisterSourceOutput(compilationAndClasses,
            static (spc, source) => Execute(source.Left, source.Right!, spc));
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
    {
        return node is ClassDeclaration classDeclaration 
            && classDeclaration.AttributeLists.Count > 0;
    }

    private static INamedTypeSymbol? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;

        foreach (var attributeList in classDeclaration.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                var symbol = context.SemanticModel.GetSymbolInfo(attribute).Symbol;
                if (symbol is not IMethodSymbol attributeSymbol)
                    continue;

                var attributeClass = attributeSymbol.ContainingType;
                var fullName = attributeClass.ToDisplayString();

                if (fullName == "Facet.Search.FacetedSearchAttribute")
                {
                    return context.SemanticModel.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;
                }
            }
        }

        return null;
    }

    private static void Execute(
        Compilation compilation,
        ImmutableArray<INamedTypeSymbol?> classes,
        SourceProductionContext context)
    {
        if (classes.IsDefaultOrEmpty)
            return;

        foreach (var classSymbol in classes)
        {
            if (classSymbol is null)
                continue;

            try
            {
                var searchableModel = SearchableModel.Create(classSymbol, context);
                
                // Generate filter class
                var filterCode = FilterClassGenerator.Generate(searchableModel);
                context.AddSource($"{searchableModel.FilterClassName}.g.cs", filterCode);

                // Generate extension methods for applying search
                var extensionCode = ExtensionMethodsGenerator.Generate(searchableModel);
                context.AddSource($"{searchableModel.ClassName}SearchExtensions.g.cs", extensionCode);

                // Generate aggregation methods if enabled
                if (searchableModel.GenerateAggregations)
                {
                    var aggregationCode = AggregationGenerator.Generate(searchableModel);
                    context.AddSource($"{searchableModel.ClassName}FacetAggregations.g.cs", aggregationCode);
                }

                // Generate metadata if enabled
                if (searchableModel.GenerateMetadata)
                {
                    var metadataCode = MetadataGenerator.Generate(searchableModel);
                    context.AddSource($"{searchableModel.ClassName}SearchMetadata.g.cs", metadataCode);
                }
            }
            catch
            {
                // Log errors appropriately
                continue;
            }
        }
    }
}
```

#### Models/SearchableModel.cs

```csharp
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

    public static SearchableModel Create(INamedTypeSymbol classSymbol, SourceProductionContext context)
    {
        var model = new SearchableModel
        {
            ClassName = classSymbol.Name,
            Namespace = classSymbol.ContainingNamespace.ToDisplayString()
        };

        // Parse [FacetedSearch] attribute
        var facetedSearchAttr = classSymbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.Name == "FacetedSearchAttribute");

        if (facetedSearchAttr != null)
        {
            // Extract attribute properties
            model.FilterClassName = GetNamedArgument<string>(facetedSearchAttr, "FilterClassName") 
                ?? $"{model.ClassName}SearchFilter";
            model.GenerateAggregations = GetNamedArgument<bool>(facetedSearchAttr, "GenerateAggregations", true);
            model.GenerateMetadata = GetNamedArgument<bool>(facetedSearchAttr, "GenerateMetadata", true);
            
            var customNamespace = GetNamedArgument<string>(facetedSearchAttr, "Namespace");
            if (!string.IsNullOrEmpty(customNamespace))
                model.Namespace = customNamespace!;
        }
        else
        {
            model.FilterClassName = $"{model.ClassName}SearchFilter";
            model.GenerateAggregations = true;
            model.GenerateMetadata = true;
        }

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
```

#### Models/SearchFacetInfo.cs

```csharp
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
                    info.FacetType = namedArg.Value.Value?.ToString() ?? "Categorical";
                    break;
                case "DisplayName":
                    info.DisplayName = namedArg.Value.Value?.ToString();
                    break;
                case "OrderBy":
                    info.OrderBy = namedArg.Value.Value?.ToString() ?? "Count";
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
                    info.RangeAggregation = namedArg.Value.Value?.ToString() ?? "Auto";
                    break;
                case "RangeIntervals":
                    info.RangeIntervals = namedArg.Value.Value?.ToString();
                    break;
            }
        }

        return info;
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
            "DateRange" => "(DateTime? From, DateTime? To)?",
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
```

#### Generators/FilterClassGenerator.cs

```csharp
using Facet.Search.Generators.Models;
using System.Text;

namespace Facet.Search.Generators;

/// <summary>
/// Generates the filter class from a SearchableModel.
/// </summary>
internal static class FilterClassGenerator
{
    public static string Generate(SearchableModel model)
    {
        var sb = new StringBuilder();

        // File header
        sb.AppendLine("// <auto-generated />");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine($"namespace {model.Namespace}.Search;");
        sb.AppendLine();

        // XML documentation
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// Generated search filter for {model.ClassName}.");
        sb.AppendLine("/// </summary>");

        // Class declaration
        sb.AppendLine($"public partial class {model.FilterClassName}");
        sb.AppendLine("{");

        // Generate properties for each facet
        foreach (var facet in model.Facets)
        {
            GenerateFacetProperty(sb, facet);
        }

        // Generate full-text search property if any text properties exist
        if (model.FullTextProperties.Count > 0)
        {
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// Full-text search query.");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    public string? SearchText { get; set; }");
            sb.AppendLine();
        }

        // Close class
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static void GenerateFacetProperty(StringBuilder sb, SearchFacetInfo facet)
    {
        sb.AppendLine("    /// <summary>");
        sb.AppendLine($"    /// Filter by {facet.DisplayName ?? facet.PropertyName}.");
        sb.AppendLine("    /// </summary>");

        switch (facet.FacetType)
        {
            case "Range":
                sb.AppendLine($"    public {facet.PropertyType}? Min{facet.PropertyName} {{ get; set; }}");
                sb.AppendLine();
                sb.AppendLine("    /// <summary>");
                sb.AppendLine($"    /// Maximum {facet.PropertyName} value.");
                sb.AppendLine("    /// </summary>");
                sb.AppendLine($"    public {facet.PropertyType}? Max{facet.PropertyName} {{ get; set; }}");
                break;

            case "DateRange":
                sb.AppendLine($"    public System.DateTime? {facet.PropertyName}From {{ get; set; }}");
                sb.AppendLine();
                sb.AppendLine("    /// <summary>");
                sb.AppendLine($"    /// End date for {facet.PropertyName} filter.");
                sb.AppendLine("    /// </summary>");
                sb.AppendLine($"    public System.DateTime? {facet.PropertyName}To {{ get; set; }}");
                break;

            case "Geo":
                sb.AppendLine($"    public double? {facet.PropertyName}Latitude {{ get; set; }}");
                sb.AppendLine();
                sb.AppendLine($"    public double? {facet.PropertyName}Longitude {{ get; set; }}");
                sb.AppendLine();
                sb.AppendLine($"    public double? {facet.PropertyName}RadiusKm {{ get; set; }}");
                break;

            case "Categorical":
            case "Hierarchical":
                sb.AppendLine($"    public string[]? {facet.PropertyName} {{ get; set; }}");
                break;

            case "Boolean":
                sb.AppendLine($"    public bool? {facet.PropertyName} {{ get; set; }}");
                break;

            default:
                sb.AppendLine($"    public {facet.PropertyType}? {facet.PropertyName} {{ get; set; }}");
                break;
        }

        sb.AppendLine();
    }
}
```

#### Generators/ExtensionMethodsGenerator.cs

```csharp
using Facet.Search.Generators.Models;
using System.Linq;
using System.Text;

namespace Facet.Search.Generators;

/// <summary>
/// Generates extension methods for applying search filters to IQueryable.
/// </summary>
internal static class ExtensionMethodsGenerator
{
    public static string Generate(SearchableModel model)
    {
        var sb = new StringBuilder();

        // File header
        sb.AppendLine("// <auto-generated />");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Linq;");
        sb.AppendLine();
        sb.AppendLine($"namespace {model.Namespace}.Search;");
        sb.AppendLine();

        // Extension class
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// Extension methods for searching {model.ClassName}.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"public static class {model.ClassName}SearchExtensions");
        sb.AppendLine("{");

        // Main search method
        GenerateApplySearchMethod(sb, model);

        // Close class
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static void GenerateApplySearchMethod(StringBuilder sb, SearchableModel model)
    {
        sb.AppendLine("    /// <summary>");
        sb.AppendLine($"    /// Applies faceted search filtering to a queryable of {model.ClassName}.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine($"    public static IQueryable<{model.Namespace}.{model.ClassName}> ApplyFacetedSearch(");
        sb.AppendLine($"        this IQueryable<{model.Namespace}.{model.ClassName}> query,");
        sb.AppendLine($"        {model.FilterClassName} filter)");
        sb.AppendLine("    {");
        sb.AppendLine("        if (filter == null)");
        sb.AppendLine("            return query;");
        sb.AppendLine();

        // Generate filter logic for each facet
        foreach (var facet in model.Facets)
        {
            GenerateFacetFilter(sb, facet, model);
        }

        // Full-text search
        if (model.FullTextProperties.Any())
        {
            sb.AppendLine("        // Full-text search");
            sb.AppendLine("        if (!string.IsNullOrWhiteSpace(filter.SearchText))");
            sb.AppendLine("        {");
            sb.AppendLine("            var searchTerm = filter.SearchText.ToLower();");
            sb.Append("            query = query.Where(x => ");

            var conditions = model.FullTextProperties
                .Select(p => $"x.{p.Name}.ToLower().Contains(searchTerm)")
                .ToArray();

            sb.Append(string.Join(" || ", conditions));
            sb.AppendLine(");");
            sb.AppendLine("        }");
            sb.AppendLine();
        }

        sb.AppendLine("        return query;");
        sb.AppendLine("    }");
    }

    private static void GenerateFacetFilter(StringBuilder sb, SearchFacetInfo facet, SearchableModel model)
    {
        sb.AppendLine($"        // Filter: {facet.PropertyName}");

        switch (facet.FacetType)
        {
            case "Categorical":
            case "Hierarchical":
                sb.AppendLine($"        if (filter.{facet.PropertyName}?.Any() == true)");
                sb.AppendLine($"            query = query.Where(x => filter.{facet.PropertyName}.Contains(x.{facet.PropertyName}));");
                break;

            case "Range":
                sb.AppendLine($"        if (filter.Min{facet.PropertyName}.HasValue)");
                sb.AppendLine($"            query = query.Where(x => x.{facet.PropertyName} >= filter.Min{facet.PropertyName}.Value);");
                sb.AppendLine();
                sb.AppendLine($"        if (filter.Max{facet.PropertyName}.HasValue)");
                sb.AppendLine($"            query = query.Where(x => x.{facet.PropertyName} <= filter.Max{facet.PropertyName}.Value);");
                break;

            case "Boolean":
                sb.AppendLine($"        if (filter.{facet.PropertyName}.HasValue)");
                sb.AppendLine($"            query = query.Where(x => x.{facet.PropertyName} == filter.{facet.PropertyName}.Value);");
                break;

            case "DateRange":
                sb.AppendLine($"        if (filter.{facet.PropertyName}From.HasValue)");
                sb.AppendLine($"            query = query.Where(x => x.{facet.PropertyName} >= filter.{facet.PropertyName}From.Value);");
                sb.AppendLine();
                sb.AppendLine($"        if (filter.{facet.PropertyName}To.HasValue)");
                sb.AppendLine($"            query = query.Where(x => x.{facet.PropertyName} <= filter.{facet.PropertyName}To.Value);");
                break;
        }

        sb.AppendLine();
    }
}
```

#### Generators/AggregationGenerator.cs

```csharp
using Facet.Search.Generators.Models;
using System.Text;

namespace Facet.Search.Generators;

/// <summary>
/// Generates facet aggregation methods (counts per facet value).
/// </summary>
internal static class AggregationGenerator
{
    public static string Generate(SearchableModel model)
    {
        var sb = new StringBuilder();

        // File header
        sb.AppendLine("// <auto-generated />");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using System.Linq;");
        sb.AppendLine("using System.Threading.Tasks;");
        sb.AppendLine();
        sb.AppendLine($"namespace {model.Namespace}.Search;");
        sb.AppendLine();

        // Result classes
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// Aggregated facet results for {model.ClassName}.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"public class {model.ClassName}FacetResults");
        sb.AppendLine("{");

        foreach (var facet in model.Facets)
        {
            if (facet.FacetType == "Categorical" || facet.FacetType == "Hierarchical")
            {
                sb.AppendLine($"    public Dictionary<string, int> {facet.PropertyName} {{ get; set; }} = new();");
            }
            else if (facet.FacetType == "Range")
            {
                sb.AppendLine($"    public {facet.PropertyType}? {facet.PropertyName}Min {{ get; set; }}");
                sb.AppendLine($"    public {facet.PropertyType}? {facet.PropertyName}Max {{ get; set; }}");
            }
        }

        sb.AppendLine("}");
        sb.AppendLine();

        // Extension class
        sb.AppendLine($"public static class {model.ClassName}FacetAggregationExtensions");
        sb.AppendLine("{");

        // Generate aggregation method
        GenerateAggregationMethod(sb, model);

        sb.AppendLine("}");

        return sb.ToString();
    }

    private static void GenerateAggregationMethod(StringBuilder sb, SearchableModel model)
    {
        sb.AppendLine("    /// <summary>");
        sb.AppendLine($"    /// Gets facet aggregations (counts per value) for {model.ClassName}.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine($"    public static async Task<{model.ClassName}FacetResults> GetFacetAggregationsAsync(");
        sb.AppendLine($"        this IQueryable<{model.Namespace}.{model.ClassName}> query)");
        sb.AppendLine("    {");
        sb.AppendLine($"        var results = new {model.ClassName}FacetResults();");
        sb.AppendLine();

        foreach (var facet in model.Facets)
        {
            if (facet.FacetType == "Categorical" || facet.FacetType == "Hierarchical")
            {
                sb.AppendLine($"        // Aggregate {facet.PropertyName}");
                sb.AppendLine($"        results.{facet.PropertyName} = query");
                sb.AppendLine($"            .GroupBy(x => x.{facet.PropertyName})");
                sb.AppendLine("            .Select(g => new { Value = g.Key, Count = g.Count() })");

                if (facet.Limit > 0)
                {
                    var orderByClause = facet.OrderBy == "Value"
                        ? "OrderBy(x => x.Value)"
                        : "OrderByDescending(x => x.Count)";
                    sb.AppendLine($"            .{orderByClause}");
                    sb.AppendLine($"            .Take({facet.Limit})");
                }

                sb.AppendLine("            .ToDictionary(x => x.Value, x => x.Count);");
                sb.AppendLine();
            }
            else if (facet.FacetType == "Range")
            {
                sb.AppendLine($"        // Get min/max for {facet.PropertyName}");
                sb.AppendLine($"        if (query.Any())");
                sb.AppendLine("        {");
                sb.AppendLine($"            results.{facet.PropertyName}Min = query.Min(x => x.{facet.PropertyName});");
                sb.AppendLine($"            results.{facet.PropertyName}Max = query.Max(x => x.{facet.PropertyName});");
                sb.AppendLine("        }");
                sb.AppendLine();
            }
        }

        sb.AppendLine("        return results;");
        sb.AppendLine("    }");
    }
}
```

#### Generators/MetadataGenerator.cs

```csharp
using Facet.Search.Generators.Models;
using System.Text;

namespace Facet.Search.Generators;

/// <summary>
/// Generates metadata about searchable facets for frontend consumption.
/// </summary>
internal static class MetadataGenerator
{
    public static string Generate(SearchableModel model)
    {
        var sb = new StringBuilder();

        // File header
        sb.AppendLine("// <auto-generated />");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine();
        sb.AppendLine($"namespace {model.Namespace}.Search;");
        sb.AppendLine();

        // Metadata class
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// Metadata about searchable facets for {model.ClassName}.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"public static class {model.ClassName}SearchMetadata");
        sb.AppendLine("{");

        // Generate facet metadata
        sb.AppendLine("    public static IReadOnlyList<FacetMetadata> Facets { get; } = new[]");
        sb.AppendLine("    {");

        foreach (var facet in model.Facets)
        {
            sb.AppendLine("        new FacetMetadata");
            sb.AppendLine("        {");
            sb.AppendLine($"            Name = \"{facet.PropertyName}\",");
            sb.AppendLine($"            DisplayName = \"{facet.DisplayName ?? facet.PropertyName}\",");
            sb.AppendLine($"            Type = \"{facet.FacetType}\",");
            sb.AppendLine($"            IsHierarchical = {facet.IsHierarchical.ToString().ToLower()},");

            if (!string.IsNullOrEmpty(facet.DependsOn))
            {
                sb.AppendLine($"            DependsOn = \"{facet.DependsOn}\",");
            }

            sb.AppendLine("        },");
        }

        sb.AppendLine("    };");
        sb.AppendLine("}");
        sb.AppendLine();

        // Metadata model
        sb.AppendLine("public class FacetMetadata");
        sb.AppendLine("{");
        sb.AppendLine("    public string Name { get; set; } = null!;");
        sb.AppendLine("    public string DisplayName { get; set; } = null!;");
        sb.AppendLine("    public string Type { get; set; } = null!;");
        sb.AppendLine("    public bool IsHierarchical { get; set; }");
        sb.AppendLine("    public string? DependsOn { get; set; }");
        sb.AppendLine("}");

        return sb.ToString();
    }
}
```

---

### 3. Facet.Search.EFCore - Entity Framework Integration

#### Extensions/QueryableExtensions.cs

```csharp
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Facet.Search.EFCore.Extensions;

/// <summary>
/// EF Core-specific extensions for faceted search.
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Executes the query and returns results as a list asynchronously.
    /// This is just a convenience wrapper around EF Core's ToListAsync.
    /// </summary>
    public static Task<List<T>> ExecuteSearchAsync<T>(
        this IQueryable<T> query,
        CancellationToken cancellationToken = default)
    {
        return query.ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets the total count of results matching the current filters.
    /// </summary>
    public static Task<int> CountSearchResultsAsync<T>(
        this IQueryable<T> query,
        CancellationToken cancellationToken = default)
    {
        return query.CountAsync(cancellationToken);
    }
}
```

#### Extensions/FacetAggregationExtensions.cs

```csharp
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Facet.Search.EFCore.Extensions;

/// <summary>
/// EF Core extensions for executing facet aggregations efficiently.
/// </summary>
public static class FacetAggregationExtensions
{
    /// <summary>
    /// Executes a categorical facet aggregation and returns value -> count dictionary.
    /// </summary>
    public static async Task<Dictionary<TKey, int>> AggregateFacetAsync<T, TKey>(
        this IQueryable<T> query,
        System.Func<T, TKey> keySelector,
        int? limit = null,
        CancellationToken cancellationToken = default)
        where TKey : notnull
    {
        var grouped = query
            .GroupBy(keySelector)
            .Select(g => new { Key = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count);

        if (limit.HasValue)
            grouped = (IQueryable<dynamic>)grouped.Take(limit.Value);

        var results = await grouped.ToListAsync(cancellationToken);
        return results.ToDictionary(x => (TKey)x.Key, x => (int)x.Count);
    }
}
```

---

## Usage Examples

### Example 1: E-commerce Product Search

```csharp
using Facet.Search;

namespace MyStore.Models;

[FacetedSearch]
public class Product
{
    public int Id { get; set; }
    
    [FullTextSearch(Weight = 2.0f)]
    public string Name { get; set; } = null!;
    
    [FullTextSearch]
    public string Description { get; set; } = null!;
    
    [SearchFacet]
    public string Brand { get; set; } = null!;
    
    [SearchFacet]
    public string Category { get; set; } = null!;
    
    [SearchFacet(Type = FacetType.Range)]
    public decimal Price { get; set; }
    
    [SearchFacet(Type = FacetType.Range, DisplayName = "Customer Rating")]
    public double Rating { get; set; }
    
    [SearchFacet]
    public string[] Tags { get; set; } = Array.Empty<string>();
    
    [SearchFacet(Type = FacetType.Boolean, DisplayName = "In Stock")]
    public bool InStock { get; set; }
    
    [Searchable]
    public DateTime CreatedAt { get; set; }
}

// Usage
public class ProductService
{
    private readonly AppDbContext _context;
    
    public async Task<List<Product>> SearchProducts(ProductSearchFilter filter)
    {
        return await _context.Products
            .ApplyFacetedSearch(filter)
            .ExecuteSearchAsync();
    }
    
    public async Task<ProductFacetResults> GetFacets()
    {
        return await _context.Products
            .GetFacetAggregationsAsync();
    }
}
```

### Example 2: Job Listings with Date Ranges

```csharp
[FacetedSearch]
public class JobListing
{
    public int Id { get; set; }
    
    [FullTextSearch(Weight = 2.0f)]
    public string Title { get; set; } = null!;
    
    [FullTextSearch]
    public string Description { get; set; } = null!;
    
    [SearchFacet(DisplayName = "Job Type")]
    public string JobType { get; set; } = null!; // Full-time, Part-time, Contract
    
    [SearchFacet]
    public string Location { get; set; } = null!;
    
    [SearchFacet(Type = FacetType.Range, DisplayName = "Salary")]
    public decimal SalaryMin { get; set; }
    
    [SearchFacet(Type = FacetType.DateRange, DisplayName = "Posted Date")]
    public DateTime PostedDate { get; set; }
    
    [SearchFacet(Type = FacetType.Boolean)]
    public bool RemoteAvailable { get; set; }
    
    [SearchFacet]
    public string[] RequiredSkills { get; set; } = Array.Empty<string>();
}
```

### Example 3: ASP.NET Core Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;
    
    [HttpGet("search")]
    public async Task<ActionResult<SearchResult>> Search([FromQuery] ProductSearchFilter filter)
    {
        // Apply filters
        var query = _context.Products.ApplyFacetedSearch(filter);
        
        // Get results
        var products = await query
            .Skip(filter.Skip ?? 0)
            .Take(filter.Take ?? 20)
            .ExecuteSearchAsync();
        
        // Get total count
        var totalCount = await query.CountSearchResultsAsync();
        
        // Get facet aggregations
        var facets = await _context.Products
            .ApplyFacetedSearch(filter) // Apply same filters
            .GetFacetAggregationsAsync();
        
        return Ok(new SearchResult
        {
            Products = products,
            TotalCount = totalCount,
            Facets = facets
        });
    }
    
    [HttpGet("metadata")]
    public ActionResult<object> GetSearchMetadata()
    {
        return Ok(ProductSearchMetadata.Facets);
    }
}
```

---

## Testing Strategy

### Unit Tests (Facet.Search.Tests)

1. **Attribute Tests**: Verify attributes are correctly defined
2. **Generator Tests**: Test source generation with various configurations
3. **Filter Generation Tests**: Ensure correct filter classes are generated
4. **Extension Method Tests**: Verify LINQ query building logic

### Integration Tests (Facet.Search.IntegrationTests)

1. **EF Core In-Memory Tests**: Test against in-memory database
2. **SQL Server Tests**: Test with real database
3. **Complex Scenarios**: Nested filters, combinations, edge cases
4. **Performance Tests**: Benchmark query generation and execution

---

## Documentation Requirements

1. **README.md**: Overview, installation, quick start
2. **API Documentation**: XML docs for all public members
3. **Examples**: Multiple real-world scenarios
4. **Migration Guide**: How to adopt in existing projects
5. **Performance Guide**: Best practices and optimization tips
6. **Troubleshooting**: Common issues and solutions

---

## IMPORTANT: Test Package Architecture First!

Before implementing all features, validate the package architecture with a minimal test:

### Step 1: Create Minimal Generator

Create a simple "Hello World" generator that proves the packaging works:

```csharp
// Facet.Search.Generators/HelloGenerator.cs
[Generator]
public class HelloGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource("Hello.g.cs", @"
namespace Facet.Search.Test
{
    public static class Hello
    {
        public static string SayHello() => ""Hello from Facet.Search Generator!"";
    }
}");
        });
    }
}
```

### Step 2: Pack and Test

```bash
# Build and pack
cd src/Facet.Search
dotnet pack -c Release

# Create test project
cd ../../
mkdir TestConsumer
cd TestConsumer
dotnet new console
dotnet add package Facet.Search --source ../src/Facet.Search/bin/Release

# Test it works
# Program.cs:
// Console.WriteLine(Facet.Search.Test.Hello.SayHello());

dotnet build
dotnet run
```

### Step 3: Verify No Roslyn Pollution

```bash
# Check dependencies
dotnet list package
# Should show ONLY: Facet.Search

# Check bin folder
ls bin/Debug/net8.0
# Should NOT contain Microsoft.CodeAnalysis.*.dll

# Success criteria:
# ✅ Code generates
# ✅ Build succeeds
# ✅ No Roslyn DLLs in output
# ✅ App runs successfully
```

**Only proceed with full implementation after this validation passes!**

---

## Implementation Phases

### Phase 1: Core Foundation
- ✅ Project structure
- ✅ Attributes and enums
- ✅ Basic source generator skeleton
- ✅ Filter class generation for categorical facets

### Phase 2: Advanced Facets
- Range facets (numeric, date)
- Boolean facets
- Full-text search
- Hierarchical facets

### Phase 3: Query Generation
- LINQ expression building
- Extension methods
- Complex filtering logic
- Query optimization

### Phase 4: Aggregations
- Facet counting
- Min/max calculations
- Order and limit support
- Dependent facets

### Phase 5: EF Core Integration
- Async methods
- Efficient query execution
- Include/ThenInclude optimization
- Performance tuning

### Phase 6: Metadata & Tooling
- Frontend metadata generation
- JSON schema output
- Developer tooling
- Documentation

### Phase 7: Testing & Polish
- Comprehensive unit tests
- Integration tests
- Performance benchmarks
- Documentation
- NuGet packaging

---

## Success Criteria

✅ **Zero boilerplate**: Users only write attributes on domain models
✅ **Type-safe**: All filters compile-time checked
✅ **Performant**: Generated queries as efficient as hand-written
✅ **Flexible**: Supports simple to complex scenarios
✅ **Clean architecture**: Zero Roslyn dependencies in consumer runtime
✅ **Verified packaging**: Test consumer project shows no analyzer pollution
✅ **Well-documented**: Clear examples and API docs
✅ **Well-tested**: >80% code coverage
✅ **Production-ready**: Stable, reliable, maintainable

### Pre-Release Checklist

Before publishing to NuGet:

- [ ] Extract .nupkg and verify folder structure
- [ ] Create fresh test project and reference package
- [ ] Run `dotnet list package` - should show only Facet.Search
- [ ] Check test project's bin folder - NO Roslyn DLLs
- [ ] Verify generated code appears in obj/generated
- [ ] Test all facet types generate correctly
- [ ] Run full test suite
- [ ] Verify documentation is complete
- [ ] Check package metadata (description, tags, etc.)
- [ ] Test on clean machine (no local builds)

---

## Future Enhancements (Post-v1.0)

- Elasticsearch/OpenSearch provider
- Azure Cognitive Search integration
- Spatial/geo search
- Fuzzy text matching
- Search result highlighting
- Query suggestions/autocomplete
- Analytics and search telemetry
- Multi-language support
- Custom facet renderers

---

## Notes for Implementation

1. **Follow Facet patterns**: Study existing Facet packages for consistency
2. **CRITICAL - Package Architecture**: Verify Roslyn dependencies never reach consumer runtime
   - Test with `dotnet list package` after referencing
   - Check bin folder for Roslyn DLLs (should be absent)
   - Use single-package approach for simplicity
3. **Source generator best practices**: Use incremental generators for performance
4. **EF Core compatibility**: Target EF Core 6+ for best feature support
5. **Documentation**: Match Facet's documentation quality and style
6. **Testing**: Extensive testing is critical for source generators
   - Test that generated code compiles
   - Test that no Roslyn dependencies leak
   - Test with various configurations
7. **Performance**: Generated code should be optimal, no runtime overhead
8. **Versioning**: Follow semantic versioning
9. **Breaking changes**: Avoid in minor/patch releases
10. **Build verification**: Always check NuGet package contents before publishing

---

## Build and Test Commands

```bash
# Build solution
dotnet build Facet.Search.sln

# Run tests
dotnet test Facet.Search.sln

# Pack NuGet packages
dotnet pack src/Facet.Search/Facet.Search.csproj -c Release
dotnet pack src/Facet.Search.Generators/Facet.Search.Generators.csproj -c Release
dotnet pack src/Facet.Search.EFCore/Facet.Search.EFCore.csproj -c Release

# Install locally for testing
dotnet add package Facet.Search --source ./src/Facet.Search/bin/Release
```

---

## Questions to Consider During Implementation

1. How should we handle collection properties (List<T>, IEnumerable<T>)?
2. Should we support custom facet value formatters?
3. How to handle localization of display names?
4. Should aggregations be lazy or eager by default?
5. How to handle very large facet value lists (pagination)?
6. Should we support facet exclusion (NOT filters)?
7. How to integrate with existing AutoMapper/Mapster configurations?
8. Should we generate OpenAPI/Swagger annotations?

---

This document provides a comprehensive blueprint for building Facet.Search. Follow the structure, implement each component methodically, and ensure alignment with the Facet ecosystem's philosophy and patterns.
