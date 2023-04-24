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
}