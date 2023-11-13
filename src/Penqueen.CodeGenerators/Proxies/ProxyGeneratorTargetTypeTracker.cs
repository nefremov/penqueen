using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Penqueen.CodeGenerators;

public class ProxyGeneratorTargetTypeTracker : ISyntaxContextReceiver
{
    public IImmutableList<TypeDeclarationSyntax> TypesForProxyGeneration = ImmutableList.Create<TypeDeclarationSyntax>();

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context) {
        if (context.Node is TypeDeclarationSyntax cdecl) {
            if (cdecl.IsDecoratedWithAttribute("GenerateProxies")) {
                TypesForProxyGeneration = TypesForProxyGeneration.Add(cdecl);
            }
        }
    }
}

