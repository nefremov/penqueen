using Microsoft.CodeAnalysis;

using System.Text;

namespace Penqueen.CodeGenerators;

public class ProxyClassGenerator
{
    private readonly EntityData _entity;
    private readonly List<IPropertySymbol> _simpleFields = new(10);
    private readonly List<IPropertySymbol> _entityFields = new(10);
    private readonly List<IPropertySymbol> _collectionFields = new(10);
    private readonly HashSet<string> _namespaces = new();
    private readonly List<IMethodSymbol> _constructors;

    public ProxyClassGenerator(EntityData entity, List<EntityData> entities)
    {
        _entity = entity;
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
                if (entities.Any(e => e.EntityType.Equals(type.TypeArguments[0], SymbolEqualityComparer.Default)))
                {
                    _collectionFields.Add(property);
                    _namespaces.Add("System.Collections.Generic");
                    continue;
                }
                // simple field
            }

            if (type.MetadataName == "IQueryableCollection`1")
            {
                if (entities.Any(e => e.EntityType.Equals(type.TypeArguments[0], SymbolEqualityComparer.Default)))
                {
                    _collectionFields.Add(property);
                    _namespaces.Add("Penqueen.Collections");
                    continue;
                }
                // simple field
            }

            if (entities.Any(e => e.EntityType.Equals(type, SymbolEqualityComparer.Default)))
            {
                _entityFields.Add(property);
                continue;
            }

            _simpleFields.Add(property);
        }


        _constructors = entity.EntityType.GetMembers().OfType<IMethodSymbol>()
            .Where(m => m.MethodKind == MethodKind.Constructor && m.Parameters.Any()).ToList();
    }

    public string Generate()
    {
        var sb = new StringBuilder(2000);
        sb.WriteUsings(_namespaces)
            .Append("namespace ").Append(_entity.DbContext.DbContextType.ContainingNamespace.ToDisplayString()).AppendLine(".Proxy;")
            .AppendLine()
            .Append("public class ").Append(_entity.EntityType.Name).Append("Proxy : ")
            .Append(_entity.EntityType).AppendLine(", INotifyPropertyChanged, INotifyPropertyChanging")
            .AppendLine("{")
            .AppendLine("    private readonly DbContext _context;")
            .AppendLine("    private readonly IEntityType _entityType;")
            .AppendLine("    private readonly ILazyLoader _lazyLoader;")
            .AppendLine()
            .Append("    public ").Append(_entity.EntityType.Name).AppendLine("Proxy(DbContext context, IEntityType entityType, ILazyLoader lazyLoader)")
            .AppendLine("    {")
            .AppendLine("        _context = context;")
            .AppendLine("        _entityType = entityType;")
            .AppendLine("        _lazyLoader = lazyLoader;")
            .WriteInitChildCollections(_entity.EntityType, _collectionFields)

            .AppendLine("    }")
            .AppendLine();

        foreach (IMethodSymbol constructor in _constructors)
        {
            sb
                .Sp().Append("public ").Append(_entity.EntityType.Name).Append("Proxy")
                .Sp().AppendLine("(")
                .Sp().Sp().AppendLine("DbContext context,")
                .Sp().Sp().AppendLine("IEntityType entityType,")
                .Sp().Sp().AppendLine("ILazyLoader lazyLoader,")
                .WriteConstructorParamDeclaration(constructor, 8).AppendLine()
                .Sp().AppendLine(")")

                .Sp().Sp().AppendLine(": base")
                .Sp().Sp().AppendLine("(")
                .WriteConstructorParamCall(constructor, 12).AppendLine()
                .Sp().Sp().AppendLine(")")
                .Sp().AppendLine("{")
                .Sp().Sp().AppendLine("_context = context;")
                .Sp().Sp().AppendLine("_entityType = entityType;")
                .Sp().Sp().AppendLine("_lazyLoader = lazyLoader;")
                .WriteInitChildCollections(_entity.EntityType, _collectionFields)
                .Sp().AppendLine("}")
                .AppendLine();
        }

        foreach (IPropertySymbol member in _simpleFields)
        {
            sb.WhiteSimpleNotificationWrapper(member, 4);
        }

        foreach (IPropertySymbol member in _entityFields)
        {
            sb.WhiteDomainNotificationWrapper(member, 4);
        }


        sb.AppendLine("    public event PropertyChangedEventHandler? PropertyChanged;");
        sb.AppendLine("    public event PropertyChangingEventHandler? PropertyChanging;");


        sb.AppendLine("}");
        return sb.ToString();
    }

}