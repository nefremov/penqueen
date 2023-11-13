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
            .Sp(shift).Append("public override ").Append(type).Append(nullable? "? " : " ").AppendLine(name)
            .Sp(shift).AppendLine("{")
            .Sp(shift).Sp().AppendLine("set")
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
            .Sp(shift).Sp().AppendLine("set")
            .Sp(shift).Sp().AppendLine("{")
            .Sp(shift).Sp().Sp().Append($"if (value != base.").Append(name).AppendLine(")")
            .Sp(shift).Sp().Sp().AppendLine("{")
            .Sp(shift).Sp().Sp().Sp().Append("PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(\"").Append(name).AppendLine("\"));")
            .Sp(shift).Sp().Sp().Sp().Append("base.").Append(name).AppendLine(" = value;")
            .Sp(shift).Sp().Sp().Sp().Append("_").Append(name).AppendLine("IsLoaded = true;")
            .Sp(shift).Sp().Sp().Sp().Append("PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(\"").Append(name).AppendLine("\"));")
            .Sp(shift).Sp().Sp().AppendLine("}")
            .Sp(shift).Sp().AppendLine("}")
            .Sp(shift).Sp().AppendLine("get")
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

    public static StringBuilder WriteConstructorParamDeclaration(this StringBuilder builder, IMethodSymbol constructor, bool lastComma = false)
    {
        for (var index = 0; index < constructor.Parameters.Length; index++)
        {
            var parameter = constructor.Parameters[index];
            var type = (INamedTypeSymbol)parameter.Type;
            var nullable = type.MetadataName == "Nullable`1";
            if (nullable)
            {
                type = (INamedTypeSymbol)type.TypeArguments[0];
            }

            builder.Append(type.Name).Append(nullable ? "? " : " ").Append(parameter.Name);
            if (index != constructor.Parameters.Length - 1 || lastComma)
            {
                builder.Append(", ");
            }
        }
        return builder;
    }
    public static StringBuilder WriteConstructorParamCall(this StringBuilder builder, IMethodSymbol constructor, bool lastComma = false)
    {
        for (var index = 0; index < constructor.Parameters.Length; index++)
        {
            var parameter = constructor.Parameters[index];
            builder.Append(parameter.Name);
            if (index != constructor.Parameters.Length - 1 || lastComma)
            {
                builder.Append(", ");
            }
        }
        return builder;
    }
}