using Microsoft.CodeAnalysis;

using System.Text;

namespace Penqueen.CodeGenerators
{
    public class CollectionInterfaceGenerator
    {
        private readonly EntityTypeCollectionData _entityData;
        private readonly List<IMethodSymbol> _constructors;

        public CollectionInterfaceGenerator(EntityTypeCollectionData entityData)
        {
            _entityData = entityData;
            _constructors = entityData.EntityType.GetMembers().OfType<IMethodSymbol>()
                .Where(m => m.MethodKind == MethodKind.Constructor && m.Parameters.Any()).ToList();
        }

        public string Generate()
        {
            var type = _entityData.EntityType.Name;
            var sb = new StringBuilder();
            sb
                .AppendLine("using Penqueen.Collections;")
                .AppendLine()
                .Append("namespace ").Append(_entityData.EntityType.ContainingNamespace.ToDisplayString()).AppendLine(";")
                .AppendLine()
                .Append("public interface I").Append(type).Append("Collection : IQueryableCollection<").Append(type).AppendLine(">")
                .AppendLine("{");
            foreach (IMethodSymbol constructor in _constructors)
            {
                sb
                    .Sp().Append(type).AppendLine(" CreateNew")
                    .Sp().AppendLine("(")
                    .WriteConstructorParamDeclaration(constructor, 8).AppendLine()
                    .Sp().AppendLine(");");
            }

            sb.AppendLine("}");
            return sb.ToString();
        }
    }
}
