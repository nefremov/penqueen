using Microsoft.CodeAnalysis;

namespace Penqueen.CodeGenerators;

public record struct EntityData(ITypeSymbol EntityType, string DbSetName, ITypeSymbol DbContext);