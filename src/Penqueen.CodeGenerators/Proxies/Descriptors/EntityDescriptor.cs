using Microsoft.CodeAnalysis;

namespace Penqueen.CodeGenerators.Proxies.Descriptors;

public record struct EntityDescriptor(ITypeSymbol EntityType, string DbSetName);