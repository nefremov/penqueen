using Microsoft.CodeAnalysis;

using System.Text;

namespace Penqueen.CodeGenerators;

public class EntityPartialClassGenerator
{
    private readonly EntityData _entity;
    private readonly List<EntityData> _entities;
    private readonly List<IPropertySymbol> _collectionFields = new(10);

    public EntityPartialClassGenerator(EntityData entity, List<EntityData> entities)
    {
        _entity = entity;
        _entities = entities;
        foreach (IPropertySymbol member in entity.EntityType.GetMembers().OfType<IPropertySymbol>())
        {
            if (!member.IsVirtual)
            {
                continue;
            }

            if (member.Type is not INamedTypeSymbol type)
            {
                continue;
            }

            if (type.MetadataName == "ICollection`1" || type.MetadataName == "IQueryableCollection`1")
            {
                _collectionFields.Add(member);
            }
        }
    }

    public (string, HashSet<ITypeSymbol>) Generate()
    {
        HashSet<ITypeSymbol> collectionTypes = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
        var stringBuilder = new StringBuilder();
        stringBuilder.Append("namespace ").Append(_entity.EntityType.ContainingNamespace.ToDisplayString()).AppendLine(";");
        stringBuilder.AppendLine();
        stringBuilder.Append("public partial class ").Append(_entity.EntityType.Name);
        stringBuilder.AppendLine("{");
        foreach (IPropertySymbol collectionProperty in _collectionFields)
        {
            var type = (collectionProperty.Type as INamedTypeSymbol)!;
            var childEntityData = _entities.FirstOrDefault(e => e.EntityType.Equals(type.TypeArguments[0], SymbolEqualityComparer.Default));
            if (childEntityData == null)
            {
                throw new NotSupportedException($"Can't find type {type} in dbSets");
            }

            collectionTypes.Add(childEntityData.EntityType);
            stringBuilder.AppendLine($"    protected ICollection<{childEntityData.EntityType.Name}> _{char.ToLower(collectionProperty.Name[0])}{collectionProperty.Name.Substring(1)} = new ObservableHashSet<{childEntityData.EntityType.Name}>();");
        }

        foreach (ITypeSymbol collectionType in collectionTypes)
        {
            stringBuilder.AppendLine($"    public interface I{collectionType.Name}Collection : ICollection<{collectionType.Name}>");
            stringBuilder.AppendLine("    {");

            var constructors = collectionType.GetMembers().OfType<IMethodSymbol>()
                .Where(m => m.MethodKind == MethodKind.Constructor && m.Parameters.Any()).ToList();

            foreach (var constructor in constructors)
            {
                stringBuilder
                    .Append($"        {collectionType.Name} CreateNew(")
                    .WriteConstructorParamDeclaration(constructor, 12)
                    .Sp(8).AppendLine(");");
            }

            stringBuilder.AppendLine("    }");

        }
        stringBuilder.AppendLine("}");

        return (stringBuilder.ToString(), collectionTypes);
    }
}