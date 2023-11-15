using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Penqueen.CodeGenerators;

[Generator]
public class ProxyGenerator : ISourceGenerator {

    private static readonly DiagnosticDescriptor ReferenceRequiredDescriptor = new (
        id: "PQ001",
        title: "Reference required",
        messageFormat: "Generated code requires referencing to \"{0}\" nuget package",
        category: "ProxyGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly string[] RequiredReferences = {"Microsoft.EntityFrameworkCore", "Microsoft.EntityFrameworkCore.Proxies", "Penqueen.Collections"};

    public void Execute(GeneratorExecutionContext context)
    {
        foreach (var requiredReference in RequiredReferences)
        {
            if (!context.Compilation.ReferencedAssemblyNames.Any(ai => ai.Name.Equals(requiredReference, StringComparison.OrdinalIgnoreCase)))
            {
                context.ReportDiagnostic(Diagnostic.Create(ReferenceRequiredDescriptor, Location.None, requiredReference));
            }
        }

        var dbContextType = context.Compilation.GetTypeByMetadataName("Microsoft.EntityFrameworkCore.DbContext");
        var dbSetType = context.Compilation.GetTypeByMetadataName("Microsoft.EntityFrameworkCore.DbSet`1");
        if (dbContextType is null || dbSetType is null)
        {
            // DbContext proxy generators are not used by the project being compiled
            return;
        }

        var targetTypeTracker = context.SyntaxContextReceiver as ProxyGeneratorTargetTypeTracker;
        if (targetTypeTracker is null)
        {
            return;
        }


        var builders = DetectEntities(context, targetTypeTracker, dbContextType, dbSetType);

        foreach (var builder in builders)
        {
            var generator = new ProxyClassGenerator(builder, builders);
            context.AddSource($"{builder.EntityType.Name}Proxy.g", SourceText.From(generator.Generate(), Encoding.UTF8));
        }

        foreach (var builder in builders)
        {
            var generator = new CollectionClassGenerator(builder);
            context.AddSource($"{builder.EntityType.Name}Collection.g", SourceText.From(generator.Generate(), Encoding.UTF8));
        }

        var extGenerator = new DbContextOptionsBuilderExtensionGenerator(builders);
        context.AddSource("ProxyExtensions.g", SourceText.From(extGenerator.Generate(), Encoding.UTF8));
        var pfGenerator = new ProxyFactoryGenerator(builders);
        context.AddSource("ProxyFactories.g", SourceText.From(pfGenerator.Generate(), Encoding.UTF8));
    }

    public void Initialize(GeneratorInitializationContext context) {
        context.RegisterForSyntaxNotifications(() => new ProxyGeneratorTargetTypeTracker());
    }
    private static List<EntityData> DetectEntities(GeneratorExecutionContext context, ProxyGeneratorTargetTypeTracker proxyGeneratorTargetTypeTracker, INamedTypeSymbol dbContextType, INamedTypeSymbol dbSetType)
    {
        List<EntityData> result = new List<EntityData>();
        var symbolEqualityComparer = SymbolEqualityComparer.Default;
        foreach (var typeNode in proxyGeneratorTargetTypeTracker.TypesForProxyGeneration)
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
                if (rec.Symbol == null)
                {
                    continue;
                }

                if (rec.Symbol!.Type is not INamedTypeSymbol type)
                {
                    continue;
                }
                ITypeSymbol entityType = type.TypeArguments.First();
                result.Add(new EntityData( entityType, rec.Symbol.Name, typeNodeSymbol));
            }
        }

        return result;
    }
}