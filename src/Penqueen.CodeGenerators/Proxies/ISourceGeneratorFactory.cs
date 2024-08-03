using Penqueen.CodeGenerators.Proxies.Descriptors;

namespace Penqueen.CodeGenerators.Proxies;

public interface ISourceGeneratorFactory
{
    IProxyClassGenerator? GetProxyClassGenerator(EntityDescriptor entity);
    ICollectionClassGenerator? GetCollectionClassGenerator(EntityDescriptor entity);
    IProxyFactoryGenerator? GetProxyFactoryGenerator();
    IDbContextOptionsBuilderExtensionsGenerator? GetDbContextOptionsBuilderExtensionsGenerator();
    IEntityConfigurationMixinGenerator? GetEntityConfigurationMixinGenerator(EntityDescriptor entity);
}