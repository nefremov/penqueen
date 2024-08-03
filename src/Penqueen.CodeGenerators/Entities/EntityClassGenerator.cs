using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using Penqueen.CodeGenerators.Entities.Descriptors;
using Penqueen.Types;

using System.Text;

namespace Penqueen.CodeGenerators.Entities;

[Generator]
public class EntityClassGenerator : ISourceGenerator {
    private static readonly DiagnosticDescriptor PartialRequiredDescriptor = new(
        id: "PQ010",
        title: "Entity must be a partial class",
        messageFormat: "Class `{0}` is not a partial class. The class is omitted from the source generation.",
        category: "PartialClassGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly string AttributeName = nameof(EntityAttribute).Substring(0, nameof(EntityAttribute).Length - nameof(Attribute).Length);

    protected virtual IEntityClassGeneratorFactory GeneratorFactory { get; set; } = new DefaultEntityClassGeneratorFactory();

    public virtual void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxContextReceiver is not CollectionDeclarationTargetTypeTracker targetTypeTracker)
        {
            return;
        }


        var entityClassDescriptors = DetectEntities(context, targetTypeTracker);

        // collection types may be for any types
        var collectionTypeHosts = new Dictionary<ITypeSymbol, HashSet<ITypeSymbol>>(SymbolEqualityComparer.Default);
        foreach (var entityType in entityClassDescriptors)
        {
            foreach (var collectionType in entityType.CollectionItemTypes)
            {
                if (collectionTypeHosts.TryGetValue(collectionType, out var hashset))
                {
                    hashset.Add(entityType.EntityType);
                }
                else
                {
                    collectionTypeHosts.Add(collectionType, new HashSet<ITypeSymbol>([entityType.EntityType], SymbolEqualityComparer.Default));
                }
            }
        }

        // entity may be extended only for partial classes
        foreach (var entityClassDescriptor in
                 entityClassDescriptors
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
            var entityPartialClassGenerator = GeneratorFactory.GetEntityPartialClassGenerator(entityClassDescriptor);
            var partialClassText = entityPartialClassGenerator?.Generate();
            if (partialClassText != null)
            {
                context.AddSource($"{entityClassDescriptor.EntityType.Name}.g", SourceText.From(partialClassText, Encoding.UTF8));
            }
            var entityExtensionsGenerator = GeneratorFactory.GetEntityExtensionsGenerator(entityClassDescriptor);
                var extensionsText = entityExtensionsGenerator?.Generate();
                if (extensionsText != null)
                {
                    context.AddSource($"{entityClassDescriptor.EntityType.Name}Extensions.g", SourceText.From(extensionsText, Encoding.UTF8));
                }
        }

        foreach (var entityTypeCollectionInfo in entityClassDescriptors)
        {
            var generator = GeneratorFactory.GetEntityCollectionInterfaceGenerator(entityTypeCollectionInfo);
            var text = generator?.Generate();
            if (text != null)
            {
                context.AddSource($"I{entityTypeCollectionInfo.EntityType.Name}Collection.g", SourceText.From(text, Encoding.UTF8));
            }
        }
    }

    public virtual void Initialize(GeneratorInitializationContext context) {
        context.RegisterForSyntaxNotifications(() => new CollectionDeclarationTargetTypeTracker(AttributeName));
    }
    private static List<EntityClassDescriptor> DetectEntities(GeneratorExecutionContext context, CollectionDeclarationTargetTypeTracker targetTypeTracker)
    {
        List<EntityClassDescriptor> result = new(targetTypeTracker.TypesGeneration.Count);
        foreach (var typeNode in targetTypeTracker.TypesGeneration)
        {
            if (!typeNode.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
            {
                context.ReportDiagnostic(Diagnostic.Create(PartialRequiredDescriptor, typeNode.GetLocation(), typeNode.Identifier.Text));
            }

            // Use the semantic model to get the symbol for this type
            var semanticModel = context.Compilation.GetSemanticModel(typeNode.SyntaxTree);
            var entityType = semanticModel.GetDeclaredSymbol(typeNode);

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

            result.Add(new EntityClassDescriptor{EntityType = entityType, CollectionProperties = collectionProperties, CollectionItemTypes = collectionItemTypes});


        }

        return result;
    }
}