using Microsoft.CodeAnalysis;

namespace Penqueen.CodeGenerators;

public record struct EntityData(ITypeSymbol EntityType, string DbSetName, DbContextData DbContext);

public record struct DbContextData(ITypeSymbol DbContextType, bool GenerateProxies, bool GenerateMixins);