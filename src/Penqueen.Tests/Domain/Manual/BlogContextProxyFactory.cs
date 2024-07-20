using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Proxies.Internal;

namespace Penqueen.Tests.Domain.Manual;

public class BlogContextProxyFactory : IProxyFactory
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public object CreateLazyLoadingProxy(
        DbContext context,
        IEntityType entityType,
        ILazyLoader loader,
        object[] constructorArguments)
    {
        if (entityType.ClrType == typeof(Blog))
        {
            return new BlogProxy(context, entityType, loader);
        }

        if (entityType.ClrType == typeof(Post))
        {
            return new PostProxy(context, entityType, loader);
        }

        throw new NotSupportedException();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
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

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public Type CreateProxyType(
        IEntityType entityType)
    {
        if (entityType.ClrType == typeof(Blog))
        {
            return typeof(BlogProxy);
        }

        if (entityType.ClrType == typeof(Post))
        {
            return typeof(PostProxy);
        }
        throw new NotSupportedException();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public object Create(
            DbContext context,
            Type type,
            params object[] constructorArguments)
    {
        var entityType = context.Model.FindRuntimeEntityType(type);
        return CreateProxy(context, entityType, constructorArguments);
    }
}