using System.Text;

using Microsoft.CodeAnalysis;

namespace Penqueen.CodeGenerators;

public static class StringBuilderExtensions
{
    public static StringBuilder WriteUsings(this StringBuilder builder, IEnumerable<string> namespaces)
    {
        foreach (var group in namespaces.GroupBy(s => s.Split('.')[0]).OrderBy(g => g.Key))
        {
            foreach (var @namespace in group.OrderBy(s => s))
            {
                builder.Append("using ").Append(@namespace).AppendLine(";");
            }
            builder.AppendLine();
        }

        return builder;
    }

    public static StringBuilder Sp(this StringBuilder builder, int prefixSpaces)
    {
        return builder.Append(' ', prefixSpaces);
    }

    public static StringBuilder Sp(this StringBuilder builder)
    {
        return builder.Sp(4);
    }

    public static StringBuilder WhiteSimpleNotificationWrapper(this StringBuilder builder, IPropertySymbol property, int shift)
    {
        var type = (property.Type as INamedTypeSymbol);
        if (type == null)
        {
            return builder;
        }

        var nullable = type.MetadataName == "Nullable`1";
        if (nullable)
        {
            type = (type.TypeArguments[0] as INamedTypeSymbol);
            if (type == null)
            {
                return builder;
            }
        }

        var name = property.Name;

        builder
            .Sp(shift).Append("public override ").Append(type).Append(nullable ? "? " : " ").AppendLine(name)
            .Sp(shift).AppendLine("{")
            .Sp(shift).Sp().WriteMethodAccessibility(property.SetMethod!.DeclaredAccessibility).AppendLine("set")
            .Sp(shift).Sp().AppendLine("{")
            .Sp(shift).Sp().Sp().Append($"if (value != base.").Append(name).AppendLine(")")
            .Sp(shift).Sp().Sp().AppendLine("{")
            .Sp(shift).Sp().Sp().Sp().Append("PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(\"").Append(name).AppendLine("\"));")
            .Sp(shift).Sp().Sp().Sp().Append("base.").Append(name).AppendLine(" = value;")
            .Sp(shift).Sp().Sp().Sp().Append("PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(\"").Append(name).AppendLine("\"));")
            .Sp(shift).Sp().Sp().AppendLine("}")
            .Sp(shift).Sp().AppendLine("}")
            .Sp(shift).AppendLine("}");

        return builder;

    }
    public static StringBuilder WhiteDomainNotificationWrapper(this StringBuilder builder, IPropertySymbol property, int shift)
    {
        var type = (property.Type as INamedTypeSymbol);
        if (type == null)
        {
            return builder;
        }

        var name = property.Name;

        builder
            .Sp(shift).Append("private bool _").Append(name).AppendLine("IsLoaded = false;")
            .Sp(shift).Append("public override ").Append(type).Append(" ").AppendLine(name)
            .Sp(shift).AppendLine("{")
            .Sp(shift).Sp().WriteMethodAccessibility(property.SetMethod!.DeclaredAccessibility).AppendLine("set")
            .Sp(shift).Sp().AppendLine("{")
            .Sp(shift).Sp().Sp().Append($"if (value != base.").Append(name).AppendLine(")")
            .Sp(shift).Sp().Sp().AppendLine("{")
            .Sp(shift).Sp().Sp().Sp().Append("PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(\"").Append(name).AppendLine("\"));")
            .Sp(shift).Sp().Sp().Sp().Append("base.").Append(name).AppendLine(" = value;")
            .Sp(shift).Sp().Sp().Sp().Append("_").Append(name).AppendLine("IsLoaded = true;")
            .Sp(shift).Sp().Sp().Sp().Append("PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(\"").Append(name).AppendLine("\"));")
            .Sp(shift).Sp().Sp().AppendLine("}")
            .Sp(shift).Sp().AppendLine("}")
            .Sp(shift).Sp().WriteMethodAccessibility(property.GetMethod!.DeclaredAccessibility).AppendLine("get")
            .Sp(shift).Sp().AppendLine("{")
            .Sp(shift).Sp().Sp().Append($"if (_").Append(name).AppendLine("IsLoaded)")
            .Sp(shift).Sp().Sp().AppendLine("{")
            .Sp(shift).Sp().Sp().Sp().Append("var navigationName = \"").Append(name).AppendLine("\";")
            .Sp(shift).Sp().Sp().Sp().AppendLine("var navigationBase = _entityType.FindNavigation(navigationName) ?? (INavigationBase?)_entityType.FindSkipNavigation(navigationName);")
            .Sp(shift).Sp().Sp().Sp().AppendLine("if (navigationBase != null && (!(navigationBase is INavigation navigation && navigation.ForeignKey.IsOwnership)))")
            .Sp(shift).Sp().Sp().Sp().AppendLine("{")
            .Sp(shift).Sp().Sp().Sp().Sp().AppendLine("_lazyLoader.Load(this, navigationName);")
            .Sp(shift).Sp().Sp().Sp().Sp().Append("_").Append(name).AppendLine("IsLoaded = true;")
            .Sp(shift).Sp().Sp().Sp().AppendLine("}")
            .Sp(shift).Sp().Sp().AppendLine("}")
            .Sp(shift).Sp().Sp().Append("return base.").Append(name).AppendLine(";")
            .Sp(shift).Sp().AppendLine("}")
            .Sp(shift).AppendLine("}");
        return builder;

    }

    public static StringBuilder WriteMethodAccessibility(this StringBuilder builder, Accessibility accessibility)
    {
        switch (accessibility)
        {
            case Accessibility.NotApplicable:
                return builder;
            case Accessibility.Private:
                return builder.Append("private ");
            case Accessibility.ProtectedAndInternal:
                return builder.Append("private protected ");
            case Accessibility.Protected:
                return builder.Append("protected ");
            case Accessibility.Internal:
                return builder.Append("internal ");
            case Accessibility.ProtectedOrInternal:
                return builder.Append("protected internal ");
            case Accessibility.Public:
                return builder;
            default:
                return builder;
        }
    }

    public static StringBuilder WriteConstructorParamDeclaration(this StringBuilder builder, IMethodSymbol constructor, int shift)
    {
        var format = SymbolDisplayFormat.FullyQualifiedFormat
            .AddParameterOptions(
                SymbolDisplayParameterOptions.IncludeDefaultValue
                | SymbolDisplayParameterOptions.IncludeName
                | SymbolDisplayParameterOptions.IncludeType)
            .AddMiscellaneousOptions(
                SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
            );

        for (var index = 0; index < constructor.Parameters.Length; index++)
        {
            var parameter = constructor.Parameters[index];
            var type = (INamedTypeSymbol)parameter.Type;
            var nullable = type.MetadataName == "Nullable`1";
            if (nullable)
            {
                type = (INamedTypeSymbol)type.TypeArguments[0];
            }

            builder
                .Sp(shift).Append(parameter.ToDisplayString(format));
            /*.Append(type).Append(nullable ? "? " : " ").Append(parameter.Name);
        if (parameter.HasExplicitDefaultValue)
        {
            builder.Append(" = ").Append(parameter.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))
        }*/
            if (index != constructor.Parameters.Length - 1)
            {
                builder.AppendLine(", ");
            }
        }
        return builder;
    }
    public static StringBuilder WriteConstructorParamCall(this StringBuilder builder, IMethodSymbol constructor, int shift)
    {
        for (var index = 0; index < constructor.Parameters.Length; index++)
        {
            var parameter = constructor.Parameters[index];
            builder.Sp(shift).Append(parameter.Name);
            if (index != constructor.Parameters.Length - 1)
            {
                builder.AppendLine(", ");
            }
        }
        return builder;
    }

    public static StringBuilder WriteInitChildCollections(this StringBuilder stringBuilder, ITypeSymbol ownerType, IEnumerable<IPropertySymbol> collectionFields)
    {
        foreach (IPropertySymbol member in collectionFields)
        {
            var type = ((INamedTypeSymbol)member.Type).TypeArguments[0];
            stringBuilder.AppendLine($"        _{char.ToLower(member.Name[0])}{member.Name.Substring(1)} = new ObservableHashSet<{type}>();");
            stringBuilder.AppendLine($"        {member.Name} = new {type.Name}Collection<{ownerType}>((ObservableHashSet<{type}>) _{char.ToLower(member.Name[0])}{member.Name.Substring(1)}, _context, this, _ => _.{member.Name}, _entityType, _lazyLoader);");
        }

        return stringBuilder;
    }
}