using Microsoft.CodeAnalysis;

using System.Text;

namespace Penqueen.CodeGenerators;

public class EntityConfigurationMixinGenerator
{
    private readonly EntityData _entity;
    private readonly List<IPropertySymbol> _collectionFields = new(10);

    public EntityConfigurationMixinGenerator(EntityData entity, List<EntityData> entities)
    {
        _entity = entity;

        foreach (IPropertySymbol property in entity.EntityType.GetVirtualNotOverridenProperties())
        {
            if (property.Type is not INamedTypeSymbol type)
            {
                continue;
            }

            if (type.MetadataName == "ICollection`1")
            {
                if (entities.Any(e => e.EntityType.Equals(type.TypeArguments[0], SymbolEqualityComparer.Default)))
                {
                    _collectionFields.Add(property);
                    continue;
                }
                // simple field
            }

            if (type.MetadataName == "IQueryableCollection`1")
            {
                if (entities.Any(e => e.EntityType.Equals(type.TypeArguments[0], SymbolEqualityComparer.Default)))
                {
                    _collectionFields.Add(property);
                }
                // simple field
            }
        }
    }

    public string Generate()
    {
        var sb = new StringBuilder();
        sb
            .AppendLine("using Microsoft.EntityFrameworkCore;")
            .AppendLine("using Microsoft.EntityFrameworkCore.Metadata.Builders;")
            .AppendLine()
            .Append("namespace ").Append(_entity.DbContext.DbContextType.ContainingNamespace.ToDisplayString()).AppendLine(".Configurations;")
            .AppendLine()
            .Append("public static class ").Append(_entity.EntityType.Name).AppendLine("EntityTypeConfigurationMixin")
            .AppendLine("{")
            .Sp().Append("public static EntityTypeBuilder<").Append(_entity.EntityType).Append("> ConfigureBackingFields(this EntityTypeBuilder<").Append(_entity.EntityType).AppendLine("> builder)")
            .Sp().AppendLine("{");
        foreach (IPropertySymbol member in _collectionFields)
        {
            sb
                .Sp().Sp().Append("builder.Navigation(g => g.").Append(member.Name).Append(").HasField(\"_").Append(char.ToLower(member.Name[0])).Append(member.Name.Substring(1)).AppendLine("\").UsePropertyAccessMode(PropertyAccessMode.Field);");
        }

        sb
            .Sp().Sp().AppendLine("return builder;")
            .Sp().AppendLine("}")
            .AppendLine("}");

        return sb.ToString();
    }
}