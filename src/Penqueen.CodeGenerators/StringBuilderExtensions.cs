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
        var reference = type.IsReferenceType;
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
            .Sp(shift).Sp().AppendLine("{");
        if (nullable || reference)
        {
            builder
                .Sp(shift).Sp().Sp().Append($"if (value == null ? base.{name} == null : value.Equals(base.{name}))");

        }
        else
        {
            builder
                .Sp(shift).Sp().Sp().Append($"if (value.Equals(base.{name}))");

        }

        builder
            .Sp(shift).Sp().Sp().AppendLine("{")
            .Sp(shift).Sp().Sp().Sp().AppendLine("return;")
            .Sp(shift).Sp().Sp().AppendLine("}")
            .Sp(shift).Sp().Sp().Append("PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(\"").Append(name).AppendLine("\"));")
            .Sp(shift).Sp().Sp().Append("base.").Append(name).AppendLine(" = value;")
            .Sp(shift).Sp().Sp().Append("PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(\"").Append(name).AppendLine("\"));")
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
            .Sp(shift).Sp().Sp().Append($"if (object.ReferenceEquals(value, base.").Append(name).AppendLine("))")
            .Sp(shift).Sp().Sp().AppendLine("{")
            .Sp(shift).Sp().Sp().Sp().AppendLine("return;")
            .Sp(shift).Sp().Sp().AppendLine("}")
            .Sp(shift).Sp().Sp().Append("PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(\"").Append(name).AppendLine("\"));")
            .Sp(shift).Sp().Sp().Append("base.").Append(name).AppendLine(" = value;")
            .Sp(shift).Sp().Sp().Append("_").Append(name).AppendLine("IsLoaded = true;")
            .Sp(shift).Sp().Sp().Append("PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(\"").Append(name).AppendLine("\"));")
            .Sp(shift).Sp().AppendLine("}")
            .Sp(shift).Sp().WriteMethodAccessibility(property.GetMethod!.DeclaredAccessibility).AppendLine("get")
            .Sp(shift).Sp().AppendLine("{")
            .Sp(shift).Sp().Sp().Append("if (_").Append(name).AppendLine("IsLoaded || !_initialized)")
            .Sp(shift).Sp().Sp().AppendLine("{")
            .Sp(shift).Sp().Sp().Sp().Append("return base.").Append(name).AppendLine(";")
            .Sp(shift).Sp().Sp().AppendLine("}")
            .Sp(shift).Sp().Sp().Append("var navigationName = \"").Append(name).AppendLine("\";")
            .Sp(shift).Sp().Sp().AppendLine("var navigationBase = _entityType.FindNavigation(navigationName) ?? (INavigationBase?)_entityType.FindSkipNavigation(navigationName);")
            .Sp(shift).Sp().Sp().AppendLine("if (navigationBase != null && (!(navigationBase is INavigation navigation && navigation.ForeignKey.IsOwnership)))")
            .Sp(shift).Sp().Sp().AppendLine("{")
            .Sp(shift).Sp().Sp().Sp().AppendLine("_lazyLoader.Load(this, navigationName);")
            .Sp(shift).Sp().Sp().Sp().Append("_").Append(name).AppendLine("IsLoaded = true;")
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

    public static StringBuilder WriteTypeAccessibility(this StringBuilder builder, Accessibility accessibility)
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
                return builder.Append("public ");
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
            .AddMemberOptions(SymbolDisplayMemberOptions.IncludeContainingType)
            .AddMiscellaneousOptions(
                SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
            );

        for (var index = 0; index < constructor.Parameters.Length; index++)
        {
            var parameter = constructor.Parameters[index];
            var type = (INamedTypeSymbol) parameter.Type;
            var nullable = type.MetadataName == "Nullable`1";
            if (nullable)
            {
                type = (INamedTypeSymbol) type.TypeArguments[0];
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

    public static StringBuilder WriteConstructorParamCallWithCast(this StringBuilder builder, IMethodSymbol constructor, int shift)
    {
        var format = SymbolDisplayFormat.FullyQualifiedFormat
            .AddParameterOptions(
                SymbolDisplayParameterOptions.IncludeDefaultValue
                | SymbolDisplayParameterOptions.IncludeName
                | SymbolDisplayParameterOptions.IncludeType)
            .AddMemberOptions(SymbolDisplayMemberOptions.IncludeContainingType)
            .AddMiscellaneousOptions(
                SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
            );

        for (var index = 0; index < constructor.Parameters.Length; index++)
        {
            var parameter = constructor.Parameters[index];
            builder.Sp(shift).Append("(").Append(parameter.Type.ToDisplayString(format)).Append(") constructorArguments[").Append(index).Append("]");
            if (index != constructor.Parameters.Length - 1)
            {
                builder.AppendLine(", ");
            }
        }

        return builder;
    }

    public static StringBuilder WriteInitChildCollections(this StringBuilder stringBuilder, ITypeSymbol ownerType, IEnumerable<IPropertySymbol> collectionFields, bool supportCollectionsInitialized, int spaces)
    {
        foreach (IPropertySymbol member in collectionFields)
        {
            var type = ((INamedTypeSymbol) member.Type).TypeArguments[0];
            var collectionType = member.Type.MetadataName == "IQueryableCollection`1" ? "QueryableHashSet" : "ObservableHashSet";
            var name = char.ToLower(member.Name[0]) + member.Name.Substring(1);
            stringBuilder
                .Sp(spaces).Append("_").Append(name).Append(" = _").Append(name).Append(" == null ? new ").Append(collectionType).Append("<").Append(type).Append(">() : new ObservableHashSet<").Append(type).Append(">(_").Append(name).AppendLine(");")
                .Sp(spaces).Append(member.Name).Append(" = new ").Append(type.Name).Append("Collection<").Append(ownerType).Append(">((ObservableHashSet<").Append(type).Append(">) _").Append(name).Append(", _context, this, _ => _.")
                .Append(member.Name).AppendLine(", _entityType, _lazyLoader);");
        }

        if (supportCollectionsInitialized)
        {
            stringBuilder
                .Sp(spaces).AppendLine("if (CollectionsInitialized != null)")
                .Sp(spaces).AppendLine("{")
                .Sp(spaces).Sp().AppendLine("CollectionsInitialized();")
                .Sp(spaces).AppendLine("}");

        }

        return stringBuilder;
    }

    public static StringBuilder WriteProxyConstructorCall(this StringBuilder builder, ITypeSymbol type, int shift)
    {
        var format = SymbolDisplayFormat.FullyQualifiedFormat
            .AddParameterOptions(
                SymbolDisplayParameterOptions.IncludeDefaultValue
                | SymbolDisplayParameterOptions.IncludeName
                | SymbolDisplayParameterOptions.IncludeType)
            .AddMemberOptions(SymbolDisplayMemberOptions.IncludeContainingType)
            .AddMiscellaneousOptions(
                SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
            );

        builder
            .Sp(shift).AppendLine("if (constructorArguments.Length == 0)")
            .Sp(shift).AppendLine("{")
            .Sp(shift).Sp().Append("return new ").Append(type.Name).AppendLine("Proxy(context, entityType, loader);")
            .Sp(shift).AppendLine("}");

        var constructors = type.GetMembers().OfType<IMethodSymbol>()
            .Where(m => m.MethodKind == MethodKind.Constructor && m.Parameters.Any()).OrderBy(c => c.Parameters.Length).ToList();


        foreach (var constructor in constructors)
        {
            builder
                .Sp(shift).Append("if (constructorArguments.Length == ").Append(constructor.Parameters.Length).AppendLine(")")
                .Sp(shift).AppendLine("{")
                .Sp(shift).Sp().Append("return new ").Append(type.Name).AppendLine("Proxy")
                .Sp(shift).Sp().AppendLine("(")
                .Sp(shift).Sp().Sp().AppendLine("context,")
                .Sp(shift).Sp().Sp().AppendLine("entityType,")
                .Sp(shift).Sp().Sp().AppendLine("loader,")
                .WriteConstructorParamCallWithCast(constructor, shift + 8)
                .Sp(shift).Sp().AppendLine(");")
                .Sp(shift).AppendLine("}");
        }

        return builder;
    }
}