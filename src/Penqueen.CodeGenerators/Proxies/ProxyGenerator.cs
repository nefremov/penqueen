using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using Penqueen.CodeGenerators.Proxies.Descriptors;
using Penqueen.Types;

using System.Text;

namespace Penqueen.CodeGenerators.Proxies;

[Generator]
public class ProxyGenerator : ISourceGenerator
{

    private static readonly DiagnosticDescriptor ReferenceRequiredDescriptor = new(
        id: "PQ001",
        title: "Reference required",
        messageFormat: "Generated code requires referencing to \"{0}\" nuget package",
        category: "ProxyGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor GlobalExceptionDescriptor = new(
        id: "PQ000",
        title: "Exception handled",
        messageFormat: "Exception: {0}, Trace: {1}",
        category: "ProxyGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly string AttributeName = nameof(GenerateProxiesAttribute).Substring(0, nameof(GenerateProxiesAttribute).Length - nameof(Attribute).Length);

    private static readonly string[] RequiredReferences = ["Microsoft.EntityFrameworkCore", "Microsoft.EntityFrameworkCore.Proxies", "Penqueen.Collections"];

    public virtual ISourceGeneratorFactory CreateSourceGeneratorFactory(GeneratorExecutionContext context, DbContextDescriptor dbContextDescriptor, INamedTypeSymbol actionTypeSymbol)
        => new DefaultSourceGeneratorFactory(context, dbContextDescriptor, actionTypeSymbol);

    public virtual void Execute(GeneratorExecutionContext context)
    {
        try
        {
            var dbContextType = context.Compilation.GetTypeByMetadataName("Microsoft.EntityFrameworkCore.DbContext");
            var dbSetType = context.Compilation.GetTypeByMetadataName("Microsoft.EntityFrameworkCore.DbSet`1");
            if (dbContextType is null || dbSetType is null)
            {
                // DbContext proxy generators are not used by the project being compiled
                return;
            }

            var actionType = context.Compilation.GetTypeByMetadataName("System.Action");
            if (actionType is null)
            {
                return;
            }

            var targetTypeTracker = context.SyntaxContextReceiver as ProxyGeneratorTargetTypeTracker;
            if (targetTypeTracker is null)
            {
                return;
            }


            var dbContextDescriptors = DetectEntities(context, targetTypeTracker, dbContextType, dbSetType);

            if (dbContextDescriptors.Count > 0)
            {
                foreach (var requiredReference in RequiredReferences)
                {
                    if (!context.Compilation.ReferencedAssemblyNames.Any(ai => ai.Name.Equals(requiredReference, StringComparison.OrdinalIgnoreCase)))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(ReferenceRequiredDescriptor, Location.None, requiredReference));
                    }
                }
            }

            foreach (var dbContextDescriptor in dbContextDescriptors)
            {
                ISourceGeneratorFactory factory =  CreateSourceGeneratorFactory(context, dbContextDescriptor, actionType);
                if (dbContextDescriptor.GenerateProxies)
                {
                    foreach (var entityDescriptor in dbContextDescriptor.EntityDescriptors)
                    {
                        var generator = factory.GetProxyClassGenerator(entityDescriptor);
                        var proxyClassText = generator?.Generate();
                        if (proxyClassText != null)
                        {
                            context.AddSource($"{entityDescriptor.EntityType.Name}Proxy.g", SourceText.From(proxyClassText, Encoding.UTF8));
                        }
                    }

                    foreach (var entityDescriptor in dbContextDescriptor.EntityDescriptors)
                    {
                        var generator = factory.GetCollectionClassGenerator(entityDescriptor);
                        var collectionClassText = generator?.Generate();
                        if (collectionClassText != null)
                        {
                            context.AddSource($"{entityDescriptor.EntityType.Name}Collection.g", SourceText.From(collectionClassText, Encoding.UTF8));
                        }
                    }

                    var extGenerator = factory.GetDbContextOptionsBuilderExtensionsGenerator();
                    var extensionsText = extGenerator?.Generate();
                    if (extensionsText != null)
                    {
                        context.AddSource($"{dbContextDescriptor.DbContextType.Name}ProxyExtensions.g", SourceText.From(extensionsText, Encoding.UTF8));
                    }

                    var pfGenerator = factory.GetProxyFactoryGenerator();
                    var factoryText = pfGenerator?.Generate();
                    if (factoryText != null)
                    {
                        context.AddSource("ProxyFactories.g", SourceText.From(factoryText, Encoding.UTF8));
                    }
                }

                if (dbContextDescriptor.GenerateMixins)
                {
                    foreach (var entityDescriptor in dbContextDescriptor.EntityDescriptors)
                    {
                        var generator = factory.GetEntityConfigurationMixinGenerator(entityDescriptor);
                        var mixinText = generator?.Generate();
                        if (mixinText != null)
                        {
                            context.AddSource($"{dbContextDescriptor.DbContextType.Name}{entityDescriptor.EntityType.Name}EntityTypeConfigurationMixin.g", SourceText.From(mixinText, Encoding.UTF8));
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            context.ReportDiagnostic(Diagnostic.Create(GlobalExceptionDescriptor, Location.None, ex.Message, ex.StackTrace));
        }
    }

    public virtual void Initialize(GeneratorInitializationContext context)
    {

        context.RegisterForSyntaxNotifications(() => new ProxyGeneratorTargetTypeTracker(AttributeName));
    }
    private static List<DbContextDescriptor> DetectEntities(GeneratorExecutionContext context, ProxyGeneratorTargetTypeTracker proxyGeneratorTargetTypeTracker, INamedTypeSymbol dbContextType, INamedTypeSymbol dbSetType)
    {
        List<DbContextDescriptor> result = new List<DbContextDescriptor>();
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

            var attr = typeNodeSymbol.GetAttributes().First(a => a.AttributeClass?.Name == nameof(GenerateProxiesAttribute));

            var customProxiesArg = attr.NamedArguments
                .Where(a => a.Key == nameof(GenerateProxiesAttribute.CustomProxies))
                .Select(a => a.Value.Value)
                .FirstOrDefault();
            var configurationMixinsArg = attr.NamedArguments
                .Where(a => a.Key == nameof(GenerateProxiesAttribute.ConfigurationMixins))
                .Select(a => a.Value.Value)
                .FirstOrDefault();

            var generateProxies = customProxiesArg != null && (bool)customProxiesArg;
            var generateMixins = configurationMixinsArg != null && (bool)configurationMixinsArg;


            var dbContextDescriptor = new DbContextDescriptor(typeNodeSymbol, generateProxies, generateMixins, new List<EntityDescriptor>());

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
                dbContextDescriptor.AddEntityDescriptor(new EntityDescriptor(entityType, rec.Symbol.Name));
            }
        }

        result.RemoveAll(r => r.EntityDescriptors.Count == 0);

        return result;
    }
}