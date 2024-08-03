using Microsoft.CodeAnalysis;

namespace Penqueen.CodeGenerators.Entities.Descriptors;

public struct EntityClassDescriptor
{
    public INamedTypeSymbol EntityType;
    public List<IPropertySymbol> CollectionProperties;
    public HashSet<INamedTypeSymbol> CollectionItemTypes;
}