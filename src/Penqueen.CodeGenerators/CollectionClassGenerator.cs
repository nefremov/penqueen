using Microsoft.CodeAnalysis;

using System.Text;

namespace Penqueen.CodeGenerators
{
    public class CollectionClassGenerator
    {
        private readonly EntityData _entity;
        private readonly List<EntityData> _entities;
        private readonly Dictionary<ITypeSymbol, HashSet<ITypeSymbol>> _collectionTypeHosts;
        private readonly List<IPropertySymbol> _simpleFields = new(10);
        private readonly List<IPropertySymbol> _entityFields = new(10);
        private readonly List<IPropertySymbol> _collectionFields = new(10);
        private readonly List<IMethodSymbol> _constructors;

        public CollectionClassGenerator(EntityData entity, List<EntityData> entities, Dictionary<ITypeSymbol, HashSet<ITypeSymbol>> collectionTypeHosts)
        {
            _entity = entity;
            _entities = entities;
            _collectionTypeHosts = collectionTypeHosts;
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
            stringBuilder.AppendLine("using Microsoft.EntityFrameworkCore;");
            stringBuilder.AppendLine("using Microsoft.EntityFrameworkCore.ChangeTracking;");
            stringBuilder.AppendLine("using Microsoft.EntityFrameworkCore.Infrastructure;");
            stringBuilder.AppendLine("using Microsoft.EntityFrameworkCore.Metadata;");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("using Penqueen.Types;"); 
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("using System.Linq.Expressions;");
            stringBuilder.AppendLine();
            stringBuilder.Append("using ").Append(_entity.EntityType.ContainingNamespace.ToDisplayString()).AppendLine(";");
            stringBuilder.AppendLine();
            stringBuilder.Append("namespace ").Append(_entity.DbContext.ContainingNamespace.ToDisplayString()).AppendLine(".Proxy;");
            stringBuilder.AppendLine();
            stringBuilder.Append(
                $"public class {_entity.EntityType.Name}Collection<T> : BackedObservableHashSet<{_entity.EntityType.Name}, T, {_entity.DbContext.Name}>");

            if (_collectionTypeHosts.TryGetValue(_entity.EntityType, out var hostTypes))
            {
                foreach (ITypeSymbol hostType in hostTypes)
                {
                    stringBuilder.Append($", {hostType.Name}.I{_entity.EntityType.Name}Collection");
                }
            }
            stringBuilder.AppendLine(" where T : class");
            stringBuilder.AppendLine(@$"
{{
    private readonly DbSet<{_entity.EntityType.Name}> _set;

    public {_entity.EntityType.Name}Collection(ObservableHashSet<{_entity.EntityType.Name}> internalCollection, {_entity.DbContext.Name} context, DbSet<{_entity.EntityType.Name}> set,
         T ownerEntity, Expression<Func<T, IEnumerable<{_entity.EntityType.Name}>>> collectionAccessor,
         IEntityType entityType, ILazyLoader lazyLoader)
    : base(internalCollection, context, ownerEntity, collectionAccessor, entityType, lazyLoader)
    {{
        _set = set;
    }}");
            foreach (IMethodSymbol constructor in _constructors)
            {
                stringBuilder.Append(@$"    public {_entity.EntityType.Name} CreateNew(");
                for (var index = 0; index < constructor.Parameters.Length; index++)
                {
                    var parameter = constructor.Parameters[index];
                    var type = (INamedTypeSymbol)parameter.Type;
                    if (type.MetadataName == "Nullable`1")
                    {
                        type = (INamedTypeSymbol)type.TypeArguments[0];
                    }

                    stringBuilder.Append(type.Name);
                    stringBuilder.Append(" ");
                    stringBuilder.Append(parameter.Name);
                    if (index != constructor.Parameters.Length - 1)
                    {
                        stringBuilder.Append(", ");
                    }
                }

                stringBuilder.Append(@$")
    {{
        var item = new {_entity.EntityType.Name}Proxy(");
                foreach (IParameterSymbol parameter in constructor.Parameters)
                {
                    stringBuilder.Append(parameter.Name).Append(", ");
                }

                stringBuilder.AppendLine(@$"Context, EntityType, LazyLoader);
        _set.Add(item);

        return item;
    }}
}}
");
            }

            return stringBuilder.ToString();
        }
    }
}
