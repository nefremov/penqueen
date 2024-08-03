using Microsoft.CodeAnalysis;

using Penqueen.CodeGenerators.Proxies.Descriptors;

using System.Text;

namespace Penqueen.CodeGenerators.Proxies.Generators;

public class DefaultEntityConfigurationMixinGenerator : IEntityConfigurationMixinGenerator
{
    private readonly EntityDescriptor _entity;
    private readonly DbContextDescriptor _dbContextDescriptor;
    private readonly List<IPropertySymbol> _collectionFields = new(10);

    public DefaultEntityConfigurationMixinGenerator(EntityDescriptor entity, DbContextDescriptor dbContextDescriptor)
    {
        _entity = entity;
        _dbContextDescriptor = dbContextDescriptor;

        foreach (IPropertySymbol property in entity.EntityType.GetVirtualNotOverridenProperties())
        {
            if (property.Type is not INamedTypeSymbol type)
            {
                continue;
            }

            if (type.MetadataName == "ICollection`1")
            {
                if (dbContextDescriptor.EntityDescriptors.Any(e => e.EntityType.Equals(type.TypeArguments[0], SymbolEqualityComparer.Default)))
                {
                    _collectionFields.Add(property);
                    continue;
                }
                // simple field
            }

            if (type.MetadataName == "IQueryableCollection`1")
            {
                if (dbContextDescriptor.EntityDescriptors.Any(e => e.EntityType.Equals(type.TypeArguments[0], SymbolEqualityComparer.Default)))
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
        sb.AppendLine("using Microsoft.EntityFrameworkCore;");
        sb.AppendLine("using Microsoft.EntityFrameworkCore.Metadata.Builders;");
        sb.AppendLine();
        sb.Append("namespace ").Append(_dbContextDescriptor.DbContextType.ContainingNamespace.ToDisplayString()).AppendLine(".Configurations;");
        sb.AppendLine();
        sb.Append("public static class ").Append(_entity.EntityType.Name).AppendLine("EntityTypeConfigurationMixin");
        sb.AppendLine("{");
        sb.Sp().Append("public static EntityTypeBuilder<").Append(_entity.EntityType).Append("> ConfigureBackingFields(this EntityTypeBuilder<").Append(_entity.EntityType).AppendLine("> builder)");
        sb.Sp().AppendLine("{");
        foreach (IPropertySymbol member in _collectionFields)
        {
            sb.Sp().Sp().Append("builder.Navigation(g => g.").Append(member.Name).Append(").HasField(\"_").Append(char.ToLower(member.Name[0])).Append(member.Name.Substring(1)).AppendLine("\").UsePropertyAccessMode(PropertyAccessMode.Field);");
        }

        sb.Sp().Sp().AppendLine("return builder;");
        sb.Sp().AppendLine("}");
        sb.AppendLine("}");

        return sb.ToString();
    }
}