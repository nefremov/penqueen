using Microsoft.CodeAnalysis;

using System.Text;

namespace Penqueen.CodeGenerators;

public class EntityPartialClassGenerator
{
    private readonly EntityTypeCollectionData _entityData;

    public EntityPartialClassGenerator(EntityTypeCollectionData entityDataData)
    {
        _entityData = entityDataData;
    }

    public string Generate()
    {
        var sb = new StringBuilder();
        sb
            .Append("namespace ").Append(_entityData.EntityType.ContainingNamespace.ToDisplayString()).AppendLine(";")
            .AppendLine()
            .WriteTypeAccessibility(_entityData.EntityType.DeclaredAccessibility).Append("partial class ").Append(_entityData.EntityType.Name)
            .AppendLine("{");
        foreach (IPropertySymbol member in _entityData.CollectionProperties)
        {
            var type = (member.Type as INamedTypeSymbol)!.TypeArguments[0];

            sb.Sp().Append("protected ICollection<").Append(type).Append("> _").Append(char.ToLower(member.Name[0])).Append(member.Name.Substring(1)).AppendLine(";");
        }
        sb.AppendLine("}");

        return sb.ToString();
    }
}