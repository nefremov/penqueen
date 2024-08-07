﻿using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using Penqueen.Types;

namespace Penqueen.CodeGenerators;

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

    private static readonly string[] RequiredReferences = { "Microsoft.EntityFrameworkCore", "Microsoft.EntityFrameworkCore.Proxies", "Penqueen.Collections" };

    public void Execute(GeneratorExecutionContext context)
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


            var builders = DetectEntities(context, targetTypeTracker, dbContextType, dbSetType);

            if (builders.Count > 0)
            {
                foreach (var requiredReference in RequiredReferences)
                {
                    if (!context.Compilation.ReferencedAssemblyNames.Any(ai => ai.Name.Equals(requiredReference, StringComparison.OrdinalIgnoreCase)))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(ReferenceRequiredDescriptor, Location.None, requiredReference));
                    }
                }
            }

            foreach (var builder in builders.Where(b => b.DbContext.GenerateProxies))
            {
                var generator = new ProxyClassGenerator(context, builder, builders, actionType);
                context.AddSource($"{builder.EntityType.Name}Proxy.g", SourceText.From(generator.Generate(), Encoding.UTF8));
            }

            foreach (var builder in builders.Where(b => b.DbContext.GenerateProxies))
            {
                var generator = new CollectionClassGenerator(builder);
                context.AddSource($"{builder.EntityType.Name}Collection.g", SourceText.From(generator.Generate(), Encoding.UTF8));
            }

            foreach (var builder in builders.Where(b => b.DbContext.GenerateMixins))
            {
                var generator = new EntityConfigurationMixinGenerator(builder, builders);
                context.AddSource($"{builder.EntityType.Name}EntityTypeConfigurationMixin.g", SourceText.From(generator.Generate(), Encoding.UTF8));
            }


            var extGenerator = new DbContextOptionsBuilderExtensionGenerator(builders.Where(b => b.DbContext.GenerateProxies).ToList());
            context.AddSource("ProxyExtensions.g", SourceText.From(extGenerator.Generate(), Encoding.UTF8));
            var pfGenerator = new ProxyFactoryGenerator(builders.Where(b => b.DbContext.GenerateProxies).ToList());
            context.AddSource("ProxyFactories.g", SourceText.From(pfGenerator.Generate(), Encoding.UTF8));
        }
        catch (Exception ex)
        {
            context.ReportDiagnostic(Diagnostic.Create(GlobalExceptionDescriptor, Location.None, ex.Message, ex.StackTrace));
        }
    }

    public void Initialize(GeneratorInitializationContext context)
    {
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

            var attr = typeNodeSymbol.GetAttributes().FirstOrDefault(a => a.AttributeClass.Name == nameof(GenerateProxiesAttribute));

            var customProxiesArg = attr!.NamedArguments.Where(a => a.Key == nameof(GenerateProxiesAttribute.CustomProxies)).Select(a => a.Value.Value).FirstOrDefault();
            var cnfigurationMixinsArg = attr!.NamedArguments.Where(a => a.Key == nameof(GenerateProxiesAttribute.ConfigurationMixins)).Select(a => a.Value.Value).FirstOrDefault();

            bool generateProxies = customProxiesArg != null && (bool)customProxiesArg;
            bool generateMixins = cnfigurationMixinsArg != null && (bool)cnfigurationMixinsArg;


            var dbContextData = new DbContextData(typeNodeSymbol, generateProxies, generateMixins);

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
                result.Add(new EntityData(entityType, rec.Symbol.Name, dbContextData));
            }
        }

        return result;
    }
}