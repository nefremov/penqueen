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
        var sb = new StringBuilder(2000);
        foreach (var item in _entities.GroupBy(e => e.DbContext.DbContextType,
                     (s, items) => new {DbContext = s!, EntityDatas = items}, SymbolEqualityComparer.Default))
        {
            sb
                .AppendLine("using Microsoft.EntityFrameworkCore;")
                .AppendLine("using Microsoft.EntityFrameworkCore.Infrastructure;")
                .AppendLine("using Microsoft.EntityFrameworkCore.Metadata;")
                .AppendLine("using Microsoft.EntityFrameworkCore.Proxies.Internal;")
                .AppendLine()
                .AppendLine()
                .Append("namespace ").Append(item.DbContext.ContainingNamespace.ToDisplayString()).AppendLine(".Proxy;")
                .AppendLine()
                .Append("public class ").Append(item.DbContext.Name).AppendLine("ProxyFactory : IProxyFactory")
                .AppendLine("{")
                .Sp().AppendLine("public object CreateLazyLoadingProxy(")
                .Sp().Sp().AppendLine("DbContext context,")
                .Sp().Sp().AppendLine("IEntityType entityType,")
                .Sp().Sp().AppendLine("ILazyLoader loader,")
                .Sp().Sp().AppendLine("object[] constructorArguments")
                .Sp().AppendLine(")")
                .Sp().AppendLine("{");
            foreach (EntityData entityData in item.EntityDatas)
            {
                sb
                    .Sp().Sp().Append("if (entityType.ClrType == typeof(").Append(entityData.EntityType).AppendLine("))")
                    .Sp().Sp().AppendLine("{")
                    .Sp().Sp().Sp().Append("return new ").Append(entityData.EntityType.Name).AppendLine("Proxy(context, entityType, loader);")
                    .Sp().Sp().AppendLine("}");
            }

            sb
                .Sp().Sp().AppendLine("throw new NotSupportedException();")
                .Sp().AppendLine("}")
                .AppendLine()
                .Sp().AppendLine("public Type CreateProxyType(IEntityType entityType)")
                .Sp().AppendLine("{");
            foreach (EntityData entityData in item.EntityDatas)
            {
                sb
                    .Sp().Sp().Append("if (entityType.ClrType == typeof(").Append(entityData.EntityType).AppendLine("))")
                    .Sp().Sp().AppendLine("{")
                    .Sp().Sp().Sp().Append("return typeof(").Append(entityData.EntityType.Name).AppendLine("Proxy);")
                    .Sp().Sp().AppendLine("}");
            }

            sb
                .Sp().Sp().AppendLine("throw new NotSupportedException();")
                .Sp().AppendLine("}")
                .AppendLine();
            sb.AppendLine(@"
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

    public object Create(
            DbContext context,
            Type type,
            params object[] constructorArguments)
    {
        var entityType = context.Model.FindRuntimeEntityType(type);
        return CreateProxy(context, entityType, constructorArguments);
    }")
                .AppendLine("}");
        }

        return sb.ToString();
    }
}