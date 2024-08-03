using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Penqueen.CodeGenerators;

public static class SourceGenExtensions
{
    private const string AttributeSuffix = "attribute";
    public static bool IsDecoratedWithAttribute(this TypeDeclarationSyntax cdecl, string attributeName)
    {
        attributeName = attributeName.ToLower();
        string attributeNameLong;
        if (attributeName.EndsWith(AttributeSuffix))
        {
            attributeNameLong = attributeName;
            attributeName = attributeName.Substring(0,attributeName.Length - AttributeSuffix.Length);
        }
        else
        {
            attributeNameLong = attributeName + AttributeSuffix;
        }

        return cdecl.AttributeLists
            .SelectMany(x => x.Attributes)
            .Select(x => x.Name.ToString().ToLower())
            .Any(x => x == attributeName || x == attributeNameLong);
    }


    public static List<IPropertySymbol> GetVirtualNotOverridenProperties(this ITypeSymbol namedTypeSymbol)
    {
        var members = namedTypeSymbol.GetMembers().OfType<IPropertySymbol>().ToArray();
        List<IPropertySymbol> results = members.Where(m => m.IsVirtual).ToList();
        List<IPropertySymbol> overrides = members.Where(m => m.IsOverride).ToList();
        while (namedTypeSymbol.BaseType is not null)
        {
            members = namedTypeSymbol.BaseType.GetMembers().OfType<IPropertySymbol>().ToArray();

            // add virtual properties not overriden in inheritors
            results.AddRange(members.Where(m => m.IsVirtual && overrides.All(p => p.Name != m.Name)));
            // add more new overrides
            overrides.AddRange(members.Where(m => m.IsOverride && overrides.All(p => p.Name != m.Name)));

            namedTypeSymbol = namedTypeSymbol.BaseType;
        }

        return results;
    }
    public static List<ISymbol> GetMembersWithInherited(this ITypeSymbol namedTypeSymbol)
    {
        var members = namedTypeSymbol.GetMembers().ToArray();
        List<ISymbol> results = members.ToList();
        while (namedTypeSymbol.BaseType is not null)
        {
            members = namedTypeSymbol.BaseType.GetMembers().OfType<ISymbol>().ToArray();

            // add virtual properties not overriden in inheritors
            results.AddRange(members.Where(m=> results.All(p => p.Name != m.Name)));
            // add more new overrides
            namedTypeSymbol = namedTypeSymbol.BaseType;
        }

        return results;
    }

    public static (INamedTypeSymbol Type, bool Nullable, bool Reference)? GetPropertyType(this IPropertySymbol property)
    {
        if (property.Type is not INamedTypeSymbol type)
        {
            return null;
        }

        var nullable = type.MetadataName == "Nullable`1";
        if (!nullable)
        {
            return (type, nullable, type.IsReferenceType);
        }

        if (type.TypeArguments[0] is not INamedTypeSymbol internalType)
        {
            return null;
        }

        return (internalType, nullable, internalType.IsReferenceType);
    }

    public static bool InheritsFromOrEquals(this ITypeSymbol type, ITypeSymbol baseType, bool includeInterfaces)
    {
        if (!includeInterfaces)
        {
            return InheritsFromOrEquals(type, baseType);
        }

        return type.GetBaseTypesAndThis().Concat(type.AllInterfaces).Contains(baseType, SymbolEqualityComparer.Default);
    }

    public static bool InheritsFromOrEquals(this ITypeSymbol type, ITypeSymbol baseType)
    {
        return type.GetBaseTypesAndThis().Contains(baseType, SymbolEqualityComparer.Default);
    }


    public static IEnumerable<ITypeSymbol> GetBaseTypesAndThis(this ITypeSymbol? type)
    {
        var current = type;
        while (current != null)
        {
            yield return current;
            current = current.BaseType;
        }
    }

}