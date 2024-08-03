using Microsoft.CodeAnalysis;

using Penqueen.CodeGenerators.Proxies.Descriptors;
using Penqueen.CodeGenerators.Proxies.Generators;

namespace Penqueen.CodeGenerators.Proxies;

public class DefaultSourceGeneratorFactory(GeneratorExecutionContext context, DbContextDescriptor dbContextDescriptor, INamedTypeSymbol actionTypeSymbol) : ISourceGeneratorFactory
{
    public IProxyClassGenerator GetProxyClassGenerator(EntityDescriptor entity) => new DefaultProxyClassGenerator(context, entity, dbContextDescriptor, actionTypeSymbol);

    public ICollectionClassGenerator GetCollectionClassGenerator(EntityDescriptor entity) => new DefaultCollectionClassGenerator(entity, dbContextDescriptor);

    public IProxyFactoryGenerator GetProxyFactoryGenerator() => new DefaultProxyFactoryGenerator(dbContextDescriptor);

    public IDbContextOptionsBuilderExtensionsGenerator GetDbContextOptionsBuilderExtensionsGenerator() => new DefaultDbContextOptionsBuilderExtensionsGenerator(dbContextDescriptor);

    public IEntityConfigurationMixinGenerator GetEntityConfigurationMixinGenerator(EntityDescriptor entity) => new DefaultEntityConfigurationMixinGenerator(entity, dbContextDescriptor);
}