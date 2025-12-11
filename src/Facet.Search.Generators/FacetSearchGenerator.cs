using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Facet.Search.Generators.Models;

namespace Facet.Search.Generators;

/// <summary>
/// Incremental source generator for Facet.Search.
/// </summary>
[Generator]
public class FacetSearchGenerator : IIncrementalGenerator
{
    private const string FacetedSearchAttributeFullName = "Facet.Search.FacetedSearchAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classDeclarations = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                FacetedSearchAttributeFullName,
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (ctx, _) => GetSearchableModel(ctx))
            .Where(static m => m is not null);

        context.RegisterSourceOutput(classDeclarations,
            static (spc, model) => Execute(model!, spc));
    }

    private static SearchableModel? GetSearchableModel(GeneratorAttributeSyntaxContext context)
    {
        if (context.TargetSymbol is not INamedTypeSymbol classSymbol)
            return null;

        return SearchableModel.Create(classSymbol, context.Attributes[0]);
    }

    private static void Execute(SearchableModel model, SourceProductionContext context)
    {
        try
        {
            context.AddSource($"{model.FilterClassName}.g.cs", FilterClassGenerator.Generate(model));
            context.AddSource($"{model.ClassName}SearchExtensions.g.cs", ExtensionMethodsGenerator.Generate(model));

            if (model.GenerateAggregations)
                context.AddSource($"{model.ClassName}FacetAggregations.g.cs", AggregationGenerator.Generate(model));

            if (model.GenerateMetadata)
                context.AddSource($"{model.ClassName}SearchMetadata.g.cs", MetadataGenerator.Generate(model));
        }
        catch
        {
        }
    }
}
