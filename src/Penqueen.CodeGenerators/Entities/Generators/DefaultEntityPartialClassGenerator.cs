using Microsoft.CodeAnalysis;

using Penqueen.CodeGenerators.Entities.Descriptors;

using System.Text;

namespace Penqueen.CodeGenerators.Entities.Generators;

public class DefaultEntityPartialClassGenerator(EntityClassDescriptor entityClassDescriptor) : IEntityPartialClassGenerator

{
    protected EntityClassDescriptor EntityClassDescriptor { get; } = entityClassDescriptor;

    protected virtual IEnumerable<string> DefaultNamespaces { get; set; } = ["System.Collections.Generic"];

    protected virtual StringBuilder GenerateNamespace(StringBuilder sb)
        => sb.Append("namespace ").Append(EntityClassDescriptor.EntityType.ContainingNamespace.ToDisplayString()).AppendLine(";");

    protected virtual StringBuilder GenerateClassDeclaration(StringBuilder sb)
        => sb.WriteTypeAccessibility(EntityClassDescriptor.EntityType.DeclaredAccessibility).Append("partial class ").Append(EntityClassDescriptor.EntityType.Name);

    protected virtual StringBuilder GenerateCollectionBackingFields(StringBuilder sb)
    {
        foreach (IPropertySymbol member in EntityClassDescriptor.CollectionProperties)
        {
            var type = (member.Type as INamedTypeSymbol)!.TypeArguments[0];

            sb.Sp().Append("protected ICollection<").Append(type).Append(">? _").Append(char.ToLower(member.Name[0])).Append(member.Name.Substring(1)).AppendLine(";");
        }

        return sb;
    }

    protected virtual StringBuilder GenerateClassBody(StringBuilder sb)
        => GenerateCollectionBackingFields(sb);

    public string Generate()
    {
        var sb = new StringBuilder();
        sb.WriteUsings(DefaultNamespaces);
        sb.AppendLine();
        GenerateNamespace(sb);
        sb.AppendLine();
        GenerateClassDeclaration(sb);
        sb.AppendLine("{");
        GenerateClassBody(sb);
        sb.AppendLine("}");

        return sb.ToString();
    }

}