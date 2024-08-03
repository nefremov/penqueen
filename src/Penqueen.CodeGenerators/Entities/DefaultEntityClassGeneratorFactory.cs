using Penqueen.CodeGenerators.Entities.Descriptors;
using Penqueen.CodeGenerators.Entities.Generators;

namespace Penqueen.CodeGenerators.Entities;

public class DefaultEntityClassGeneratorFactory : IEntityClassGeneratorFactory
{
    public IEntityPartialClassGenerator? GetEntityPartialClassGenerator(EntityClassDescriptor entityClassDescriptor)
    {
        return new DefaultEntityPartialClassGenerator(entityClassDescriptor);
    }

    public IEntityExtensionsGenerator? GetEntityExtensionsGenerator(EntityClassDescriptor entityClassDescriptor)
    {
        return null;
    }

    public IEntityCollectionInterfaceGenerator? GetEntityCollectionInterfaceGenerator(EntityClassDescriptor entityClassDescriptor)
    {
        return new DefaultEntityCollectionInterfaceGenerator(entityClassDescriptor);
    }
}