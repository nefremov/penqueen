using Microsoft.CodeAnalysis;

using Penqueen.CodeGenerators.Proxies;
using Penqueen.Types.ConstructorTech;

namespace Penqueen.CodeGenerators.ConstructorTech.Entities;

[Generator]
public class EntityClassGenerator : CodeGenerators.Entities.EntityClassGenerator
{
    private static readonly string AttributeName = nameof(ConstructorTechEntityAttribute).Substring(0, nameof(ConstructorTechEntityAttribute).Length - nameof(Attribute).Length);

    public override void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new ProxyGeneratorTargetTypeTracker(AttributeName));
    }

    public override void Execute(GeneratorExecutionContext context)
    {
        var modifiableType = context.Compilation.GetTypeByMetadataName("Constructor.Domain.Common.IModifiable");
        GeneratorFactory = new DefaultEntityClassGeneratorFactory(modifiableType);
        base.Execute(context);
    }
}