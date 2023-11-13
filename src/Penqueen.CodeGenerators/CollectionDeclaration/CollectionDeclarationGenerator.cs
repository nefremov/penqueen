using System.Text;
using Microsoft.CodeAnalysis;
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


        var entityTypeCollectionDatas = DetectEntities(context, targetTypeTracker);

        var collectionTypeHosts = new Dictionary<ITypeSymbol, HashSet<ITypeSymbol>>(SymbolEqualityComparer.Default);
        foreach (var entityType in entityTypeCollectionDatas)
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

        //foreach (var entityTypeCollectionData in entityTypeCollectionDatas)
        //{
        //    var classGenerator = new EntityPartialClassGenerator2(entityTypeCollectionData);
        //    context.AddSource($"{entityTypeCollectionData.EntityType.Name}.g", SourceText.From(classGenerator.Generate(), Encoding.UTF8));

        //}

        foreach (var entityTypeCollectionData in entityTypeCollectionDatas)
        {
            var generator = new CollectionInterfaceGenerator(entityTypeCollectionData);
            context.AddSource($"{entityTypeCollectionData.EntityType.Name}Collection.g", SourceText.From(generator.Generate(), Encoding.UTF8));
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
                collectionProperties
                    .Select(p => p.Type as INamedTypeSymbol)
                    .Where(t => t is not null)
                    .Select(t => t!.TypeArguments[0] as INamedTypeSymbol)
                    .Where(t => t is not null),
                SymbolEqualityComparer.Default
            );

            result.Add(new EntityTypeCollectionData{EntityType = entityType, CollectionProperties = collectionProperties, CollectionItemTypes = collectionItemTypes});


        }

        return result;
    }
}