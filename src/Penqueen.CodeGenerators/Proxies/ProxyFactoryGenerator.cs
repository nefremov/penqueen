using Microsoft.CodeAnalysis;

using System.Text;

namespace Penqueen.CodeGenerators;

public class ProxyFactoryGenerator
{
    private readonly List<EntityData> _entities;

    public ProxyFactoryGenerator(List<EntityData> entities)
    {
        _entities = entities;
    }

    public string Generate()
    {
        var stringBuilder = new StringBuilder();
        foreach (var item in _entities.GroupBy(e => e.DbContext,
                     (s, datas) => new { DbContext = s, EntityDatas = datas }, SymbolEqualityComparer.Default))
        {
            stringBuilder.AppendLine("using Microsoft.EntityFrameworkCore;");
            stringBuilder.AppendLine("using Microsoft.EntityFrameworkCore.Infrastructure;");
            stringBuilder.AppendLine("using Microsoft.EntityFrameworkCore.Metadata;");
            stringBuilder.AppendLine("using Microsoft.EntityFrameworkCore.Proxies.Internal;");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine();
            stringBuilder.Append("namespace ").Append(item.DbContext.ContainingNamespace.ToDisplayString()).AppendLine(".Proxy;");
            stringBuilder.AppendLine(); stringBuilder.Append(@"public class ").Append(item.DbContext.Name).AppendLine(@"ProxyFactory : IProxyFactory
{
    public object CreateLazyLoadingProxy(
        DbContext context,
        IEntityType entityType,
        ILazyLoader loader,
        object[] constructorArguments)
    {");
            foreach (EntityData entityData in item.EntityDatas)
            {
                stringBuilder.AppendLine($@"
        if (entityType.ClrType == typeof({entityData.EntityType.Name}))
        {{
            return new {entityData.EntityType.Name}Proxy(({item.DbContext.Name})context, entityType, loader);
        }}");
            }

            stringBuilder.AppendLine(@"
        throw new NotSupportedException();
    }

    public object CreateProxy(
        DbContext context,
        IEntityType entityType,
        object[] constructorArguments)
    {
        return CreateLazyLoadingProxy(
            context,
            entityType,
            context.GetService<ILazyLoader>(),
            constructorArguments);

    }

    public Type CreateProxyType(
        IEntityType entityType)
    {");
            foreach (EntityData entityData in item.EntityDatas)
            {
                stringBuilder.AppendLine($@"
        if (entityType.ClrType == typeof({entityData.EntityType.Name}))
        {{
            return typeof({entityData.EntityType.Name}Proxy);
        }}");
            }

            stringBuilder.AppendLine(@"
        throw new NotSupportedException();
    }

    public object Create(
            DbContext context,
            Type type,
            params object[] constructorArguments)
    {
        var entityType = context.Model.FindRuntimeEntityType(type);
        return CreateProxy(context, entityType, constructorArguments);
    }
}");
        }

        return stringBuilder.ToString();
    }
}