using Microsoft.CodeAnalysis;

using Penqueen.CodeGenerators.ConstructorTech.Entities.Generators;
using Penqueen.CodeGenerators.Entities;
using Penqueen.CodeGenerators.Entities.Descriptors;

namespace Penqueen.CodeGenerators.ConstructorTech.Entities;

public class DefaultEntityClassGeneratorFactory(INamedTypeSymbol? modifiableInterface) : IEntityClassGeneratorFactory
{
    public IEntityPartialClassGenerator? GetEntityPartialClassGenerator(EntityClassDescriptor entityClassDescriptor)
    {
        return new DefaultEntityPartialClassGenerator(entityClassDescriptor, modifiableInterface);
    }

    public IEntityExtensionsGenerator? GetEntityExtensionsGenerator(EntityClassDescriptor entityClassDescriptor)
    {
        return new DefaultEntityExtensionsGenerator(entityClassDescriptor, modifiableInterface);
    }

    public IEntityCollectionInterfaceGenerator? GetEntityCollectionInterfaceGenerator(EntityClassDescriptor entityClassDescriptor)
    {
        return new CodeGenerators.Entities.Generators.DefaultEntityCollectionInterfaceGenerator(entityClassDescriptor);
    }
}