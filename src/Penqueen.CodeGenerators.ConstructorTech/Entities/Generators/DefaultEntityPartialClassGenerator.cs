using Microsoft.CodeAnalysis;

using Penqueen.CodeGenerators.Entities.Descriptors;

using System.Text;

namespace Penqueen.CodeGenerators.ConstructorTech.Entities.Generators;

public class DefaultEntityPartialClassGenerator : CodeGenerators.Entities.Generators.DefaultEntityPartialClassGenerator
{
    protected bool IsModifiable;
    private readonly List<IPropertySymbol> _simpleFields = new(10);

    protected override IEnumerable<string> DefaultNamespaces => [
        "System.Collections.Generic",
        "Constructor.Domain.Common",
        "Constructor.Platform.Common",
        "Constructor.Platform.Security",
        "Constructor.Platform.Validation",
        "Constructor.Platform.Validation.Rules"
    ];


    public virtual StringBuilder GenerateSetMethodForPropertiesWithProtectedSetters(StringBuilder sb, IPropertySymbol property)
    {
        var result = property.GetPropertyType();
        if (result == null)
        {
            return sb;
        }

        if (!IsModifiable)
        {
            return sb;
        }


        var (type, nullable, _) = result.Value;

        var name = property.Name;


        sb.Append("    public ").Append(type).Append(nullable ? "? " : " ").Append("Set").Append(name).Append("(NewOperationContext operationContext, ").Append(type).Append(nullable ? "? " : " ").Append("value)").AppendLine();
        sb.Append("    {").AppendLine();
        sb.Append("    if (EntityRules == null || !EntityRules.Update.Accept(this, operationContext))").AppendLine();
        sb.Append("    {").AppendLine();
        sb.Append("        throw new SecurityException();").AppendLine();
        sb.Append("    }").AppendLine();
        sb.AppendLine();
        sb.Append("    if (EntityRules != null && EntityRules.PropertyRules.TryGetValue(").Append(name).Append(", out var validationRules))").AppendLine();
        sb.Append("    {").AppendLine();
        sb.Append("        value = ((PropertyRuleSet<").Append(EntityClassDescriptor.EntityType.ToDisplayString()).Append(", ").Append(type).Append(nullable ? "?>" : ">").Append(")validationRules).Validate(entity, value, operationContext.Validation);").AppendLine();
        sb.Append("        if (!operationContext.Validation.IsValid)").AppendLine();
        sb.Append("        {").AppendLine();
        sb.Append("           return entity;").AppendLine();
        sb.Append("        }").AppendLine();
        sb.Append("    }").AppendLine();
        sb.Append("    ").Append(name).Append(" = value;").AppendLine();
        sb.AppendLine();
        sb.Append("    entity.Modified = operationContext.DateTimeService.UtcNow;").AppendLine();
        sb.Append("    entity.ModifiedByUserId = operationContext.UserId;").AppendLine();
        sb.Append("    entity.ModifiedOnBehalfOfUserId = operationContext.OnBehalfOfUserId;").AppendLine();
        sb.Append("    entity.ModifiedByApplicationId = operationContext.ApplicationId;").AppendLine();
        sb.AppendLine();
        sb.Append("    return entity;").AppendLine();
        sb.Append("}").AppendLine();

        return sb;
    }

    protected override StringBuilder GenerateClassBody(StringBuilder sb)
    {
        GenerateCollectionBackingFields(sb);
        sb.AppendLine();
        if (!IsModifiable)
        {
            return sb;
        }

        foreach (var property in _simpleFields)
        {
            GenerateSetMethodForPropertiesWithProtectedSetters(sb, property);
            sb.AppendLine();
        }

        return sb;
    }

    public DefaultEntityPartialClassGenerator(EntityClassDescriptor entityClassDescriptor, INamedTypeSymbol? modifiableInterface)
        : base(entityClassDescriptor)
    {
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