using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Collections.Immutable;

namespace Penqueen.CodeGenerators.Proxies;

public class ProxyGeneratorTargetTypeTracker(string attributeName) : ISyntaxContextReceiver
{
    public IImmutableList<TypeDeclarationSyntax> TypesForProxyGeneration = ImmutableList.Create<TypeDeclarationSyntax>();

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        if (context.Node is not TypeDeclarationSyntax classDecl)
        {
            return;
        }

        if (classDecl.IsDecoratedWithAttribute(attributeName)) {
            TypesForProxyGeneration = TypesForProxyGeneration.Add(classDecl);
        }
    }
}
