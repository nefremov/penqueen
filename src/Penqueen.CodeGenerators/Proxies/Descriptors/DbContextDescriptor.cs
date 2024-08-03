using Microsoft.CodeAnalysis;

namespace Penqueen.CodeGenerators.Proxies.Descriptors;

public record struct DbContextDescriptor(ITypeSymbol DbContextType, bool GenerateProxies, bool GenerateMixins, ICollection<EntityDescriptor> EntityDescriptors)
{
    public readonly bool Equals(DbContextDescriptor other)
    {
        return SymbolEqualityComparer.Default.Equals(DbContextType, other.DbContextType);
    }

    public override int GetHashCode() => SymbolEqualityComparer.Default.GetHashCode(DbContextType);

    public void AddEntityDescriptor(EntityDescriptor entityDescriptor)
    {
        EntityDescriptors.Add(entityDescriptor);
    }
}