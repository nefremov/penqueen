using Microsoft.CodeAnalysis;

using System.Text;

namespace Penqueen.CodeGenerators;

public class ProxyClassGenerator
{
    private readonly EntityData _entity;
    private readonly List<EntityData> _entities;
    private readonly List<IPropertySymbol> _simpleFields = new(10);
    private readonly List<IPropertySymbol> _entityFields = new(10);
    private readonly List<IPropertySymbol> _collectionFields = new(10);
    private readonly HashSet<string> _namespaces = new();
    private readonly List<IMethodSymbol> _constructors;

    public ProxyClassGenerator(EntityData entity, List<EntityData> entities)
    {
        _entity = entity;
        _entities = entities;
        _namespaces.Add("Microsoft.EntityFrameworkCore");
        _namespaces.Add("Microsoft.EntityFrameworkCore.ChangeTracking");
        _namespaces.Add("Microsoft.EntityFrameworkCore.Infrastructure");
        _namespaces.Add("Microsoft.EntityFrameworkCore.Metadata");
        _namespaces.Add("System.ComponentModel");
        _namespaces.Add(entity.EntityType.ContainingNamespace.ToDisplayString());

        foreach (IPropertySymbol property in entity.EntityType.GetVirtualNotOverridenProperties())
        {
            if (property.Type is not INamedTypeSymbol type)
            {
                continue;
            }

            if (type.MetadataName == "ICollection`1")
            {
                _collectionFields.Add(property);
                _namespaces.Add("System.Collections.Generic");
                continue;
            }

            if (type.MetadataName == "IQueryableCollection`1")
            {
                _collectionFields.Add(property);
                _namespaces.Add("Penqueen.Collections");
                continue;
            }

            if (entities.Any(e => e.EntityType.Equals(type, SymbolEqualityComparer.Default)))
            {
                _entityFields.Add(property);
                _namespaces.Add(property.Type.ContainingNamespace.ToDisplayString());
                continue;
            }

            _simpleFields.Add(property);
            _namespaces.Add(property.Type.ContainingNamespace.ToDisplayString());
        }


        _constructors = entity.EntityType.GetMembers().OfType<IMethodSymbol>()
            .Where(m => m.MethodKind == MethodKind.Constructor && m.Parameters.Any()).ToList();
    }

    public string Generate()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.WriteUsings(_namespaces);
        stringBuilder.Append("namespace ").Append(_entity.DbContext.ContainingNamespace.ToDisplayString()).AppendLine(".Proxy;");
        stringBuilder.AppendLine();
        stringBuilder.Append("public class ").Append(_entity.EntityType.Name).Append("Proxy : ")
            .Append(_entity.EntityType.Name).AppendLine(", INotifyPropertyChanged, INotifyPropertyChanging");
        stringBuilder.AppendLine("{");
        stringBuilder.AppendLine("    private readonly DbContext _context;");
        stringBuilder.AppendLine("    private readonly IEntityType _entityType;");
        stringBuilder.AppendLine("    private readonly ILazyLoader _lazyLoader;");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine(
            $"    public {_entity.EntityType.Name}Proxy(DbContext context, IEntityType entityType, ILazyLoader lazyLoader)");
        stringBuilder.AppendLine("    {");
        stringBuilder.AppendLine("        _context = context;");
        stringBuilder.AppendLine("        _entityType = entityType;");
        stringBuilder.AppendLine("        _lazyLoader = lazyLoader;");
        WriteInitChildCollections(stringBuilder);

        stringBuilder.AppendLine("    }");

        foreach (IMethodSymbol constructor in _constructors)
        {
            stringBuilder
                .Append($"    public {_entity.EntityType.Name}Proxy(DbContext context, IEntityType entityType, ILazyLoader lazyLoader, ")
                .WriteConstructorParamDeclaration(constructor)
                .AppendLine(")");
            stringBuilder
                .Append("        :base (").WriteConstructorParamCall(constructor).AppendLine(")");
            stringBuilder.AppendLine("    {");
            stringBuilder.AppendLine("        _context = context;");
            stringBuilder.AppendLine("        _entityType = entityType;");
            stringBuilder.AppendLine("        _lazyLoader = lazyLoader;");

            WriteInitChildCollections(stringBuilder);

            stringBuilder.AppendLine("    }");
        }

        foreach (IPropertySymbol member in _simpleFields)
        {
            stringBuilder.WhiteSimpleNotificationWrapper(member, 4);
        }

        foreach (IPropertySymbol member in _entityFields)
        {
            stringBuilder.WhiteDomainNotificationWrapper(member, 4);
        }


        stringBuilder.AppendLine("    public event PropertyChangedEventHandler? PropertyChanged;");
        stringBuilder.AppendLine("    public event PropertyChangingEventHandler? PropertyChanging;");


        stringBuilder.AppendLine("}");
        return stringBuilder.ToString();
    }


    private StringBuilder WriteInitChildCollections(StringBuilder stringBuilder)
    {
        foreach (IPropertySymbol member in _collectionFields)
        {
            var type = member.Type as INamedTypeSymbol;
            var childEntityData = _entities.FirstOrDefault(e => e.EntityType.Equals(type.TypeArguments[0], SymbolEqualityComparer.Default));
            if (childEntityData == null)
            {
                throw new NotSupportedException($"Can't find type {type} in dbSets");
            }
            stringBuilder.AppendLine($"        _{char.ToLower(member.Name[0])}{member.Name.Substring(1)} = new ObservableHashSet<{childEntityData.EntityType.Name}>();");
            stringBuilder.AppendLine($"        {member.Name} = new {childEntityData.EntityType.Name}Collection<{_entity.EntityType.Name}>((ObservableHashSet<{childEntityData.EntityType.Name}>)_{char.ToLower(member.Name[0])}{member.Name.Substring(1)}, _context, this, _ => _.{member.Name}, _entityType, _lazyLoader);");
        }

        return stringBuilder;
    }
}