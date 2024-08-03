using Penqueen.CodeGenerators.Entities.Descriptors;

namespace Penqueen.CodeGenerators.Entities;

public interface IEntityClassGeneratorFactory
{
    IEntityPartialClassGenerator? GetEntityPartialClassGenerator(EntityClassDescriptor entityClassDescriptor);
    IEntityExtensionsGenerator? GetEntityExtensionsGenerator(EntityClassDescriptor entityClassDescriptor);
    IEntityCollectionInterfaceGenerator? GetEntityCollectionInterfaceGenerator(EntityClassDescriptor entityClassDescriptor);
}