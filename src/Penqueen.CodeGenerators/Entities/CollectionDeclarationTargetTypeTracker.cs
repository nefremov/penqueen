using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Penqueen.CodeGenerators.Entities;

public class CollectionDeclarationTargetTypeTracker(string attributeName) : ISyntaxContextReceiver
{
    public IImmutableList<ClassDeclarationSyntax> TypesGeneration = ImmutableList.Create<ClassDeclarationSyntax>();

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        if (context.Node is not ClassDeclarationSyntax classDeclaration)
        {
            return;
        }

        if (classDeclaration.IsDecoratedWithAttribute(attributeName))
        {
            TypesGeneration = TypesGeneration.Add(classDeclaration);
        }
    }
}

