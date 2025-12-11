using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Linq;
using Facet.Search.Generators.Models;

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
        return node is ClassDeclarationSyntax classDeclaration
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
