using Microsoft.CodeAnalysis;

using System.Text;

namespace Penqueen.CodeGenerators
{
    public class CollectionClassGenerator
    {
        private readonly EntityData _entity;
        private readonly List<IMethodSymbol> _constructors;

        public CollectionClassGenerator(EntityData entity)
        {
            _entity = entity;

            _constructors = entity.EntityType.GetMembers().OfType<IMethodSymbol>()
                .Where(m => m.MethodKind == MethodKind.Constructor && m.Parameters.Any()).ToList();
        }

        public string Generate()
        {
            var type = _entity.EntityType.Name;
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("using Microsoft.EntityFrameworkCore;")
                .AppendLine("using Microsoft.EntityFrameworkCore.ChangeTracking;")
                .AppendLine("using Microsoft.EntityFrameworkCore.Infrastructure;")
                .AppendLine("using Microsoft.EntityFrameworkCore.Metadata;")
                .AppendLine()
                .AppendLine("using Penqueen.Collections;")
                .AppendLine()
                .AppendLine("using System.Linq.Expressions;")
                .AppendLine()
                .Append("using ").Append(_entity.EntityType.ContainingNamespace.ToDisplayString()).AppendLine(";")
                .AppendLine()
                .Append("namespace ").Append(_entity.DbContext.ContainingNamespace.ToDisplayString()).AppendLine(".Proxy;")
                .AppendLine()
                .Append("public class ").Append(type).Append("Collection<T> : BackedObservableHashSet<").Append(type).Append(", T>, I").Append(type).Append("Collection").AppendLine(" where T : class")
                .AppendLine("{")
                .Sp().Append("public ").Append(type).Append("Collection(ObservableHashSet<").Append(type).AppendLine("> internalCollection, DbContext context,")
                .Sp().Sp().Append("T ownerEntity, Expression<Func<T, IEnumerable<").Append(type).AppendLine(">>> collectionAccessor,")
                .Sp().Sp().AppendLine("IEntityType entityType, ILazyLoader lazyLoader)")
                .Sp().AppendLine(": base(internalCollection, context, ownerEntity, collectionAccessor, entityType, lazyLoader)")
                .Sp().AppendLine("{")
                .Sp().AppendLine("}");
            foreach (IMethodSymbol constructor in _constructors)
            {
                stringBuilder
                    .Sp().Append("public ").Append(type).Append(" CreateNew(").WriteConstructorParamDeclaration(constructor).AppendLine(")")
                    .Sp().AppendLine("{")
                    .Sp().Sp().Append("var item = new ").Append(type).Append("Proxy(Context, EntityType, LazyLoader, ").WriteConstructorParamCall(constructor).AppendLine(");")
                    .Sp().Sp().AppendLine("Context.Add(item);")
                    .Sp().Sp().AppendLine("return item;")
                    .Sp().AppendLine("}");
            }

            stringBuilder.AppendLine("}");

            return stringBuilder.ToString();
        }
    }
}
