using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Proxies.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Penqueen.Tests.Domain.Manual;

public static class ProxiesServiceCollectionExtensions
{
    /// <summary>
    ///     Adds the services required for proxy support in Entity Framework.
    /// </summary>
    /// <remarks>
    ///     Calling this method is no longer necessary when building most applications, including those that
    ///     use dependency injection in ASP.NET or elsewhere.
    ///     It is only needed when building the internal service provider for use with
    ///     the <see cref="DbContextOptionsBuilder.UseInternalServiceProvider" /> method.
    ///     This is not recommend other than for some advanced scenarios.
    /// </remarks>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>
    ///     The same service collection so that multiple calls can be chained.
    /// </returns>
    public static IServiceCollection AddBlogContextProxies(
        this IServiceCollection serviceCollection)
    {
        new EntityFrameworkServicesBuilder(serviceCollection)
            .TryAddProviderSpecificServices(
                b => b.TryAddSingleton<IProxyFactory, BlogContextProxyFactory>());

        return serviceCollection;
    }
}