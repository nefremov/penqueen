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
    private readonly List<IMethodSymbol> _constructors;

    public ProxyClassGenerator(EntityData entity, List<EntityData> entities)
    {
        _entity = entity;
        _entities = entities;
        foreach (IPropertySymbol member in entity.EntityType.GetMembers().OfType<IPropertySymbol>())
        {
            if (!member.IsVirtual && !member.IsOverride)
            {
                continue;
            }

            if (member.Type is not INamedTypeSymbol type)
            {
                continue;
            }

            if (type.MetadataName == "ICollection`1")
            {
                _collectionFields.Add(member);
                continue;
            }

            if (entities.Any(e => e.EntityType.Equals(type, SymbolEqualityComparer.Default)))
            {
                _entityFields.Add(member);
                continue;
            }

            _simpleFields.Add(member);
        }

        _constructors = entity.EntityType.GetMembers().OfType<IMethodSymbol>()
            .Where(m => m.MethodKind == MethodKind.Constructor && m.Parameters.Any()).ToList();
    }

    public string Generate()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("using Microsoft.EntityFrameworkCore.ChangeTracking;");
        stringBuilder.AppendLine("using Microsoft.EntityFrameworkCore.Infrastructure;");
        stringBuilder.AppendLine("using Microsoft.EntityFrameworkCore.Metadata;");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine("using System.ComponentModel;");
        stringBuilder.AppendLine();
        stringBuilder.Append("using ").Append(_entity.EntityType.ContainingNamespace.ToDisplayString()).AppendLine(";");
        stringBuilder.AppendLine();
        stringBuilder.Append("namespace ").Append(_entity.DbContext.ContainingNamespace.ToDisplayString()).AppendLine(".Proxy;");
        stringBuilder.AppendLine();
        stringBuilder.Append("public class ").Append(_entity.EntityType.Name).Append("Proxy : ")
            .Append(_entity.EntityType.Name).AppendLine(", INotifyPropertyChanged, INotifyPropertyChanging");
        stringBuilder.Append("{");
        stringBuilder.AppendLine($"    private readonly {_entity.DbContext.Name} _context;");
        stringBuilder.AppendLine("    private readonly IEntityType _entityType;");
        stringBuilder.AppendLine("    private readonly ILazyLoader _lazyLoader;");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine(
            $"    public {_entity.EntityType.Name}Proxy({_entity.DbContext.Name} context, IEntityType entityType, ILazyLoader lazyLoader)");
        stringBuilder.AppendLine("    {");
        stringBuilder.AppendLine("        _context = context;");
        stringBuilder.AppendLine("        _entityType = entityType;");
        stringBuilder.AppendLine("        _lazyLoader = lazyLoader;");
        WriteInitChildCollections(stringBuilder);

        stringBuilder.AppendLine("    }");

        foreach (IMethodSymbol constructor in _constructors)
        {
            stringBuilder.Append(
                $"    public {_entity.EntityType.Name}Proxy(");
            foreach (IParameterSymbol parameter in constructor.Parameters)
            {
                stringBuilder.Append(parameter.Type).Append(" ").Append(parameter.Name).Append(", ");
            }
            stringBuilder.AppendLine($"{_entity.DbContext.Name} context, IEntityType entityType, ILazyLoader lazyLoader)");
            stringBuilder.Append("        :base (");
            for (var index = 0; index < constructor.Parameters.Length; index++)
            {
                var parameter = constructor.Parameters[index];
                stringBuilder.Append(parameter.Name);
                if (index != constructor.Parameters.Length - 1)
                {
                    stringBuilder.Append(", ");
                }
            }

            stringBuilder.AppendLine(")");
            stringBuilder.AppendLine("    {");
            stringBuilder.AppendLine("        _context = context;");
            stringBuilder.AppendLine("        _entityType = entityType;");
            stringBuilder.AppendLine("        _lazyLoader = lazyLoader;");

            WriteInitChildCollections(stringBuilder);

            stringBuilder.AppendLine("    }");
        }

        foreach (IPropertySymbol member in _simpleFields)
        {
            var type = (member.Type as INamedTypeSymbol)!;

            if (type.MetadataName == "Nullable`1")
            {
                type = (type.TypeArguments[0] as INamedTypeSymbol);
                stringBuilder.AppendLine($@"
    public override {type.Name}? {member.Name}
    {{
        set
        {{
            if (value != base.{member.Name})
            {{
                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(""{member.Name}""));
                base.{member.Name} = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""{member.Name}""));
            }}
        }}
    }}");
            }
            else
            {
                stringBuilder.AppendLine($@"
    public override {type.Name} {member.Name}
    {{
        set
        {{
            if (value != base.{member.Name})
            {{
                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(""{member.Name}""));
                base.{member.Name} = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""{member.Name}""));
            }}
        }}
    }}");
            }
        }

        foreach (IPropertySymbol member in _entityFields)
        {
            var type = member.Type as INamedTypeSymbol;
            stringBuilder.AppendLine($@"
    private bool _{member.Name}IsLoaded = false;
    public override {type.Name} {member.Name}
    {{
        set
        {{
            if (value != base.{member.Name})
            {{
                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(""{member.Name}""));
                base.{member.Name} = value;
                _{member.Name}IsLoaded = true;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""{member.Name}""));
            }}
        }}
        get
        {{
            if (!_{member.Name}IsLoaded)
            {{
                var navigationName = ""{member.Name}"";
                var navigationBase = _entityType.FindNavigation(navigationName) ?? (INavigationBase?)_entityType.FindSkipNavigation(navigationName);

                if (navigationBase != null && (!(navigationBase is INavigation navigation && navigation.ForeignKey.IsOwnership)))
                {{
                    _lazyLoader.Load(this, navigationName);
                    _{member.Name}IsLoaded = true;
                }}
            }}

            return base.{member.Name};
        }}
    }}");
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
            stringBuilder.AppendLine($"        {member.Name} = new {childEntityData.EntityType.Name}Collection<{_entity.EntityType.Name}>((ObservableHashSet<{childEntityData.EntityType.Name}>)_{char.ToLower(member.Name[0])}{member.Name.Substring(1)}, _context, _context.{childEntityData.DbSetName}, this, _ => _.{member.Name}, _entityType, _lazyLoader);");
        }

        return stringBuilder;
    }
}