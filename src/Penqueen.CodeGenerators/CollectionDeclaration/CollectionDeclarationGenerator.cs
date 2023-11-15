using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Penqueen.CodeGenerators;

public struct EntityTypeCollectionData
{
    public INamedTypeSymbol EntityType;
    public List<IPropertySymbol> CollectionProperties;
    public HashSet<INamedTypeSymbol> CollectionItemTypes;
}

[Generator]
public class CollectionDeclarationGenerator : ISourceGenerator {

    public void Execute(GeneratorExecutionContext context)
    {
        var targetTypeTracker = context.SyntaxContextReceiver as CollectionDeclarationTargetTypeTracker;
        if (targetTypeTracker is null)
        {
            return;
        }


        var entityTypeCollectionInfos = DetectEntities(context, targetTypeTracker);

        var collectionTypeHosts = new Dictionary<ITypeSymbol, HashSet<ITypeSymbol>>(SymbolEqualityComparer.Default);
        foreach (var entityType in entityTypeCollectionInfos)
        {
            foreach (var collectionType in entityType.CollectionItemTypes)
            {
                if (collectionTypeHosts.TryGetValue(collectionType, out var hashset))
                {
                    hashset.Add(entityType.EntityType);
                }
                else
                {
                    collectionTypeHosts.Add(collectionType,
                        new HashSet<ITypeSymbol>(new[] {entityType.EntityType}, SymbolEqualityComparer.Default));
                }
            }
        }

        foreach (var entityTypeCollectionInfo in
                 entityTypeCollectionInfos
                     .Where(
                         e =>
                             e.EntityType.DeclaringSyntaxReferences.Any(
                                 sr =>
                                     sr.GetSyntax() is ClassDeclarationSyntax cd
                                     && cd.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword))
                             )
                     )
                )
        {
            var generator = new EntityPartialClassGenerator(entityTypeCollectionInfo);
            context.AddSource($"{entityTypeCollectionInfo.EntityType.Name}.g", SourceText.From(generator.Generate(), Encoding.UTF8));
        }

        foreach (var entityTypeCollectionInfo in entityTypeCollectionInfos)
        {
            var generator = new CollectionInterfaceGenerator(entityTypeCollectionInfo);
            context.AddSource($"I{entityTypeCollectionInfo.EntityType.Name}Collection.g", SourceText.From(generator.Generate(), Encoding.UTF8));
        }
    }

    public void Initialize(GeneratorInitializationContext context) {
        context.RegisterForSyntaxNotifications(() => new CollectionDeclarationTargetTypeTracker());
    }
    private static List<EntityTypeCollectionData> DetectEntities(GeneratorExecutionContext context, CollectionDeclarationTargetTypeTracker targetTypeTracker)
    {
        List<EntityTypeCollectionData> result = new(targetTypeTracker.TypesGeneration.Count);
        foreach (var typeNode in targetTypeTracker.TypesGeneration)
        {
            // Use the semantic model to get the symbol for this type
            var semanticModel = context.Compilation.GetSemanticModel(typeNode.SyntaxTree);
            var entityType = semanticModel.GetDeclaredSymbol(typeNode) as INamedTypeSymbol;

            if (entityType is null) 
            {
                continue;
            }

            var collectionProperties = entityType.GetVirtualNotOverridenProperties()
                .Where(p => p.Type.MetadataName == "ICollection`1" || p.Type.MetadataName == "IQueryableCollection`1")
                .ToList();

            var collectionItemTypes = new HashSet<INamedTypeSymbol>(
                from p in collectionProperties
                        let type = p.Type as INamedTypeSymbol
                where type is not null
                    let typeArg = type.TypeArguments[0] as INamedTypeSymbol
                where typeArg is not null
                select typeArg!,
                SymbolEqualityComparer.Default
            );

            result.Add(new EntityTypeCollectionData{EntityType = entityType, CollectionProperties = collectionProperties, CollectionItemTypes = collectionItemTypes});


        }

        return result;
    }
}