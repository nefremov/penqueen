using Microsoft.CodeAnalysis;

using Penqueen.CodeGenerators.Entities.Descriptors;

using System.Text;

namespace Penqueen.CodeGenerators.Entities.Generators;

public class DefaultEntityCollectionInterfaceGenerator : IEntityCollectionInterfaceGenerator
{
    private readonly EntityClassDescriptor _entityClassDescriptor;
    private readonly List<IMethodSymbol> _constructors;

    public DefaultEntityCollectionInterfaceGenerator(EntityClassDescriptor entityClassDescriptor)
    {
        _entityClassDescriptor = entityClassDescriptor;
        _constructors = entityClassDescriptor.EntityType.GetMembers().OfType<IMethodSymbol>()
            .Where(m => m.MethodKind == MethodKind.Constructor && m.Parameters.Any()).ToList();
    }

    protected IEnumerable<string> DefaultNamespaces { get; set; } = ["Penqueen.Collections"];

    protected virtual StringBuilder GenerateNamespace(StringBuilder sb)
        => sb.Append("namespace ").Append(_entityClassDescriptor.EntityType.ContainingNamespace.ToDisplayString()).AppendLine(";");

    protected virtual StringBuilder GenerateInterfaceDeclaration(StringBuilder sb)
        => sb.WriteTypeAccessibility(_entityClassDescriptor.EntityType.DeclaredAccessibility).Append("interface I").Append(_entityClassDescriptor.EntityType.Name).AppendLine(">");

    protected virtual StringBuilder GenerateFactoryMethods(StringBuilder sb)
    {
        foreach (var constructor in _constructors)
        {
            sb.Sp().Append(_entityClassDescriptor.EntityType.Name).AppendLine(" CreateNew");
            sb.Sp().AppendLine("(");
            sb.WriteConstructorParamDeclaration(constructor, 8).AppendLine();
            sb.Sp().AppendLine(");");
        }

        return sb;
    }

    protected virtual StringBuilder GenerateInterfaceBody(StringBuilder sb)
        => GenerateFactoryMethods(sb);

    public string Generate()
    {
        var sb = new StringBuilder();
        sb.WriteUsings(DefaultNamespaces);
        sb.AppendLine();
        GenerateNamespace(sb);
        sb.AppendLine();
        GenerateInterfaceDeclaration(sb);
        sb.AppendLine("{");
        GenerateInterfaceBody(sb);
        sb.AppendLine("}");

        return sb.ToString();
    }
}