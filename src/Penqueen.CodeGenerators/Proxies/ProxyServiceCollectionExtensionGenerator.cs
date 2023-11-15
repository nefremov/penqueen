using System.Text;
using Microsoft.CodeAnalysis;

namespace Penqueen.CodeGenerators;

public class DbContextOptionsBuilderExtensionGenerator
{
    private readonly List<ITypeSymbol> _dbContexts;
    private readonly List<string> _namespaces;

    public DbContextOptionsBuilderExtensionGenerator(List<EntityData> entities)
    {
        _dbContexts = entities.Select(e => e.DbContext.DbContextType).Distinct(SymbolEqualityComparer.Default).OfType<ITypeSymbol>().ToList();
        _namespaces = _dbContexts.Select(c => c.ContainingNamespace.ToDisplayString()).Distinct().ToList();
    }

    public string Generate()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("using Microsoft.EntityFrameworkCore.Infrastructure;");
        stringBuilder.AppendLine("using Microsoft.EntityFrameworkCore.Proxies.Internal;");
        stringBuilder.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        stringBuilder.AppendLine();

        foreach (string ns in _namespaces)
        {
            stringBuilder.Append("using ").Append(ns).AppendLine(".Proxy;");
        }

        stringBuilder.AppendLine("namespace Microsoft.EntityFrameworkCore.Proxies.Internal;");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine("public static class ProxiesDbContextOptionsBuilderExtensions");
        stringBuilder.AppendLine("{");
        foreach (ITypeSymbol dbContext in _dbContexts)
        {
            stringBuilder.AppendLine(
                $"    public static DbContextOptionsBuilder Use{dbContext.Name}Proxies(this DbContextOptionsBuilder optionsBuilder)");
            stringBuilder.AppendLine("    {");
            stringBuilder.AppendLine($"        return optionsBuilder.ReplaceService<IProxyFactory, {dbContext.Name}ProxyFactory>();");
            stringBuilder.AppendLine(@"   }");
        }

        stringBuilder.AppendLine(@" }");

        return stringBuilder.ToString();
    }
}