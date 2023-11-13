using Microsoft.CodeAnalysis;

namespace Penqueen.CodeGenerators;

public class EntityData
{
    public ITypeSymbol EntityType { get; set; }
    public string DbSetName { get; set; }
    public ITypeSymbol DbContext { get; set; }
}