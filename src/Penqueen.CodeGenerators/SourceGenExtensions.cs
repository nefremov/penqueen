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
}