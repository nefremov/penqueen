using Penqueen.CodeGenerators.Proxies.Descriptors;

using System.Text;

namespace Penqueen.CodeGenerators.Proxies.Generators;

public class DefaultDbContextOptionsBuilderExtensionsGenerator(DbContextDescriptor dbContextDescriptor) : IDbContextOptionsBuilderExtensionsGenerator
{
    public string Generate()
    {
        var sb = new StringBuilder();
        sb.Append("using Microsoft.EntityFrameworkCore.Infrastructure;").AppendLine();
        sb.Append("using Microsoft.EntityFrameworkCore.Proxies.Internal;").AppendLine();
        sb.Append("using Microsoft.Extensions.DependencyInjection;").AppendLine();
        sb.AppendLine();
        sb.Append("using ").Append(dbContextDescriptor.DbContextType.ContainingNamespace.ToDisplayString()).Append(".Proxy;").AppendLine();
        sb.Append("namespace Microsoft.EntityFrameworkCore.Proxies.Internal;").AppendLine();
        sb.AppendLine();
        sb.Append("public static class ").Append(dbContextDescriptor.DbContextType.Name).Append("ProxiesDbContextOptionsBuilderExtensions").AppendLine();
        sb.Append("{").AppendLine();
        sb.Append("    public static DbContextOptionsBuilder Use").Append(dbContextDescriptor.DbContextType.Name).Append("Proxies(this DbContextOptionsBuilder optionsBuilder)").AppendLine();
        sb.Append("    {").AppendLine();
        sb.Append("        return optionsBuilder.ReplaceService<IProxyFactory, ").Append(dbContextDescriptor.DbContextType.Name).Append("ProxyFactory>();").AppendLine();
        sb.Append("    }").AppendLine();
        sb.Append("}").AppendLine();

        return sb.ToString();
    }
}