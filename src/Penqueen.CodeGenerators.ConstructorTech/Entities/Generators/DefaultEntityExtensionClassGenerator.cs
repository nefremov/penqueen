using Microsoft.CodeAnalysis;

using Penqueen.CodeGenerators.Entities;
using Penqueen.CodeGenerators.Entities.Descriptors;

using System.Text;

namespace Penqueen.CodeGenerators.ConstructorTech.Entities.Generators;

public class DefaultEntityExtensionsGenerator: IEntityExtensionsGenerator
{
    protected bool IsModifiable;
    private readonly List<IPropertySymbol> _simpleFields = new(10);

    protected EntityClassDescriptor EntityClassDescriptor { get; private set; }
    protected virtual IEnumerable<string> DefaultNamespaces => [
        "Constructor.Platform.Common",
        "Constructor.Platform.Security",
        "Constructor.Platform.Validation",
    ];

    protected virtual StringBuilder GenerateNamespace(StringBuilder sb)
        => sb.Append("namespace ").Append(EntityClassDescriptor.EntityType.ContainingNamespace.ToDisplayString()).AppendLine(";");

    protected virtual StringBuilder GenerateClassDeclaration(StringBuilder sb)
        => sb.Append("public static class ").Append(EntityClassDescriptor.EntityType.Name).Append("Extensions");
    protected virtual StringBuilder GenerateSetExtensionMethods(StringBuilder sb, IPropertySymbol property)
    {
        var result = property.GetPropertyType();
        if (result == null)
        {
            return sb;
        }

        var (type, nullable, _) = result.Value;

        var name = property.Name;
        var entityType = EntityClassDescriptor.EntityType.Name;
        sb.Sp().Append("public static (").Append(entityType).Append(" Entity, NewOperationContext OperationContext) Set")
            .Append(name).Append("(this (").Append(entityType).Append(" Entity, NewOperationContext OperationContext) source, ")
            .Append(type).Append(nullable ? "? " : " ").Append(" value)").AppendLine();
        sb.Sp().Sp().Append("=> source.Entity.Set").Append(name).Append("(source.OperationContext, value)");
        return sb;
    }

    protected virtual StringBuilder GenerateClassBody(StringBuilder sb)
    {
        foreach (IPropertySymbol property in _simpleFields)
        {
            GenerateSetExtensionMethods(sb, property);
            sb.AppendLine();
        }

        return sb;
    }


    public string? Generate()
    {
        if (!IsModifiable)
        {
            return null;
        }

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

    public DefaultEntityExtensionsGenerator(EntityClassDescriptor entityClassDescriptor, INamedTypeSymbol? modifiableInterface) 
    {
        EntityClassDescriptor = entityClassDescriptor;
        IsModifiable = modifiableInterface != null && entityClassDescriptor.EntityType.AllInterfaces.Contains(modifiableInterface, SymbolEqualityComparer.Default);

        if (!IsModifiable)
        {
            return;
        }

        foreach (IPropertySymbol property in entityClassDescriptor.EntityType.GetVirtualNotOverridenProperties())
        {
            if (property.Type is not INamedTypeSymbol type)
            {
                continue;
            }

            if (type.MetadataName is "ICollection`1" or "IQueryableCollection`1")
            {
                continue;
            }

            if (property is
                {
                    DeclaredAccessibility: Accessibility.Public,
                    SetMethod.DeclaredAccessibility: Accessibility.Protected
                }
               )
            {
                _simpleFields.Add(property);
            }
        }
    }
}
