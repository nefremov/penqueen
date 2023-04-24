using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Penqueen.CodeGenerators;

[Generator]
public class ProxyGenerator : ISourceGenerator {

    public void Execute(GeneratorExecutionContext context)
    {
        var cancellationToken = context.CancellationToken;

        var dbContextType = context.Compilation.GetTypeByMetadataName("Microsoft.EntityFrameworkCore.DbContext");
        var dbSetType = context.Compilation.GetTypeByMetadataName("Microsoft.EntityFrameworkCore.DbSet`1");
        if (dbContextType is null || dbSetType is null)
        {
            // DbContext proxy generators are not used by the project being compiled
            return;
        }

        var targetTypeTracker = context.SyntaxContextReceiver as TargetTypeTracker;
        if (targetTypeTracker is null)
        {
            return;
        }


        var builders = DetectEntities(context, targetTypeTracker, dbContextType, dbSetType);

        var collectionTypeHosts = new Dictionary<ITypeSymbol, HashSet<ITypeSymbol>>(SymbolEqualityComparer.Default);
        foreach (var builder in builders)
        {
            var classGenerator = new EntityPartialClassGenerator(builder, builders);
            var (text, collectionTypes) = classGenerator.Generate();
            context.AddSource($"{builder.EntityType.Name}.g", SourceText.From(text, Encoding.UTF8));

            foreach (ITypeSymbol collectionType in collectionTypes)
            {
               if (collectionTypeHosts.TryGetValue(collectionType, out var hashset))
               {
                    hashset.Add(builder.EntityType);
               } 
               else
               {
                   collectionTypeHosts.Add(collectionType, new HashSet<ITypeSymbol>(new[] { builder.EntityType }, SymbolEqualityComparer.Default));
               }
            }
        }

        foreach (var builder in builders)
        {
            var generator = new ProxyClassGenerator(builder, builders);
            context.AddSource($"{builder.EntityType.Name}Proxy.g", SourceText.From(generator.Generate(), Encoding.UTF8));
        }

        foreach (var builder in builders)
        {
            var generator = new CollectionClassGenerator(builder, builders, collectionTypeHosts);
            context.AddSource($"{builder.EntityType.Name}Collection.g", SourceText.From(generator.Generate(), Encoding.UTF8));
        }

        var extGenerator = new DbContextOptionsBuilderExtensionGenerator(builders);
        context.AddSource($"ProxyExtensions.g", SourceText.From(extGenerator.Generate(), Encoding.UTF8));
        var pfGenerator = new ProxyFactoryGenerator(builders);
        context.AddSource($"ProxyFactories.g", SourceText.From(pfGenerator.Generate(), Encoding.UTF8));
        var hsGenerator = new BackedObservableHashSetGenerator();
        context.AddSource($"BackedObservableHashSet.g", SourceText.From(hsGenerator.Generate(), Encoding.UTF8));
    }

    public void Initialize(GeneratorInitializationContext context) {
        context.RegisterForSyntaxNotifications(() => new TargetTypeTracker());
    }
    private static List<EntityData> DetectEntities(GeneratorExecutionContext context, TargetTypeTracker targetTypeTracker, INamedTypeSymbol dbContextType, INamedTypeSymbol dbSetType)
    {
        List<EntityData> result = new List<EntityData>();
        var symbolEqualityComparer = SymbolEqualityComparer.Default;
        foreach (var typeNode in targetTypeTracker.TypesForProxyGeneration)
        {
            // Use the semantic model to get the symbol for this type
            var semanticModel = context.Compilation.GetSemanticModel(typeNode.SyntaxTree);
            var typeNodeSymbol = semanticModel.GetDeclaredSymbol(typeNode);

            if (typeNodeSymbol is null || !symbolEqualityComparer.Equals(typeNodeSymbol.BaseType, dbContextType)) // only direct inheritance from DbContext is supported now
            {
                continue;
            }

            var dbSetProperties = typeNode.Members.OfType<PropertyDeclarationSyntax>().Select(p => new { Syntax = p, Symbol = semanticModel.GetDeclaredSymbol(p) })
                .Where(x => x.Symbol != null && symbolEqualityComparer.Equals(x.Symbol.Type.OriginalDefinition, dbSetType));

            foreach (var rec in dbSetProperties)
            {
                ITypeSymbol entityType = (rec.Symbol.Type as INamedTypeSymbol).TypeArguments.First();
                result.Add(new EntityData { EntityType = entityType, DbSetName = rec.Symbol.Name, DbContext = typeNodeSymbol });
            }
        }

        return result;
    }
}

public class TargetTypeTracker : ISyntaxContextReceiver
{
    public IImmutableList<TypeDeclarationSyntax> TypesForProxyGeneration = ImmutableList.Create<TypeDeclarationSyntax>();

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context) {
        if (context.Node is TypeDeclarationSyntax cdecl) {
            if (cdecl.IsDecoratedWithAttribute("GenerateProxies")) {
                TypesForProxyGeneration = TypesForProxyGeneration.Add(cdecl);
            }
        }
    }
}

