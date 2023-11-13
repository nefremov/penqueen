using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Penqueen.CodeGenerators;

public class CollectionDeclarationTargetTypeTracker : ISyntaxContextReceiver
{
    public IImmutableList<TypeDeclarationSyntax> TypesGeneration = ImmutableList.Create<TypeDeclarationSyntax>();

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context) {
        if (context.Node is TypeDeclarationSyntax cdecl) {
            if (cdecl.IsDecoratedWithAttribute("DeclareCollection")) {
                TypesGeneration = TypesGeneration.Add(cdecl);
            }
        }
    }
}

