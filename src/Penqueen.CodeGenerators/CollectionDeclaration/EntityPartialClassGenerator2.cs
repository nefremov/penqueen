using Microsoft.CodeAnalysis;

using System.Text;

namespace Penqueen.CodeGenerators;

public class EntityPartialClassGenerator2
{
    private readonly EntityTypeCollectionData _entityData;

    public EntityPartialClassGenerator2(EntityTypeCollectionData entityDataData)
    {
        _entityData = entityDataData;
    }

    public string Generate()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append("namespace ").Append(_entityData.EntityType.ContainingNamespace.ToDisplayString()).AppendLine(";");
        stringBuilder.AppendLine();
        stringBuilder.Append("public partial class ").Append(_entityData.EntityType.Name);
        stringBuilder.AppendLine("{");
        foreach (IPropertySymbol member in _entityData.CollectionProperties)
        {
            var type = (member.Type as INamedTypeSymbol)!.TypeArguments[0];

            stringBuilder.AppendLine($"    protected ICollection<{type.Name}> _{char.ToLower(member.Name[0])}{member.Name.Substring(1)} = new ObservableHashSet<{type.Name}>();");
        }
        stringBuilder.AppendLine("}");

        return stringBuilder.ToString();
    }
}