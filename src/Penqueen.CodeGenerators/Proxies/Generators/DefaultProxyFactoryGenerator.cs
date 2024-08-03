using Penqueen.CodeGenerators.Proxies.Descriptors;

using System.Text;

namespace Penqueen.CodeGenerators.Proxies.Generators;

public class DefaultProxyFactoryGenerator(DbContextDescriptor dbContextDescriptor) : IProxyFactoryGenerator
{
    public string Generate()
    {
        var sb = new StringBuilder(2000);
        sb
            .Append("using Microsoft.EntityFrameworkCore;").AppendLine()
            .Append("using Microsoft.EntityFrameworkCore.Infrastructure;").AppendLine()
            .Append("using Microsoft.EntityFrameworkCore.Metadata;").AppendLine()
            .Append("using Microsoft.EntityFrameworkCore.Proxies.Internal;").AppendLine()
            .AppendLine()
            .Append("using System;").AppendLine()
            .AppendLine()
            .Append("namespace ").Append(dbContextDescriptor.DbContextType.ContainingNamespace.ToDisplayString()).Append(".Proxy;").AppendLine()
            .AppendLine()
            .Append("public class ").Append(dbContextDescriptor.DbContextType.Name).Append("ProxyFactory : IProxyFactory").AppendLine()
            .Append("{").AppendLine()
            .Sp().Append("public object CreateLazyLoadingProxy(").AppendLine()
            .Sp().Sp().Append("DbContext context,").AppendLine()
            .Sp().Sp().Append("IEntityType entityType,").AppendLine()
            .Sp().Sp().Append("ILazyLoader loader,").AppendLine()
            .Sp().Sp().Append("object[] constructorArguments").AppendLine()
            .Sp().Append(")").AppendLine()
            .Sp().Append("{").AppendLine();
        foreach (EntityDescriptor entityDescriptor in dbContextDescriptor.EntityDescriptors)
        {
            sb
                .Sp().Sp().Append("if (entityType.ClrType == typeof(").Append(entityDescriptor.EntityType).Append("))").AppendLine()
                .Sp().Sp().Append("{").AppendLine().WriteProxyConstructorCall(entityDescriptor.EntityType, 12)
                .Sp().Sp().Append("}")
                .AppendLine();
        }

        sb
            .Sp().Sp().Append("throw new NotSupportedException();").AppendLine()
            .Sp().Append("}").AppendLine()
            .AppendLine()
            .Sp().Append("public Type CreateProxyType(IEntityType entityType)").AppendLine()
            .Sp().Append("{").AppendLine();
        foreach (EntityDescriptor entityDescriptor in dbContextDescriptor.EntityDescriptors)
        {
            sb
                .Sp().Sp()
                .Append("if (entityType.ClrType == typeof(").Append(entityDescriptor.EntityType).Append("))").AppendLine()
                .Sp().Sp().Append("{").AppendLine()
                .Sp().Sp().Sp().Append("return typeof(").Append(entityDescriptor.EntityType.Name).Append("Proxy);").AppendLine()
                .Sp().Sp().Append("}").AppendLine();
        }

        sb
            .Sp().Sp().Append("throw new NotSupportedException();").AppendLine()
            .Sp().Append("}").AppendLine()
            .AppendLine();
        sb.Append(@"
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
            .AppendLine()
            .Append("}").AppendLine();

        return sb.ToString();
    }
}