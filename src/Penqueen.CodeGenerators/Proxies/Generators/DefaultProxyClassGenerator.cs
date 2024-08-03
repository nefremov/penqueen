using Microsoft.CodeAnalysis;

using Penqueen.CodeGenerators.Proxies.Descriptors;

using System.Text;

namespace Penqueen.CodeGenerators.Proxies.Generators;

public class DefaultProxyClassGenerator : IProxyClassGenerator
{
    private static readonly DiagnosticDescriptor SetterRequiredDescriptor = new(
        id: "PQ002",
        title: "Overridable set method required",
        messageFormat: "EntityDescriptor type `{0}` must have overridable setter for the property `{1}`",
        category: "ProxyGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    protected readonly GeneratorExecutionContext Context;
    protected readonly EntityDescriptor EntityDescriptor;
    protected readonly DbContextDescriptor DbContextDescriptor;
    private readonly List<IPropertySymbol> _simpleFields = new(10);
    private readonly List<IPropertySymbol> _entityFields = new(10);
    private readonly List<IPropertySymbol> _collectionFields = new(10);
    private readonly HashSet<string> _namespaces;
    private readonly List<IMethodSymbol> _constructors;
    private readonly bool _supportCollectionInitialized;

    public virtual IEnumerable<string> DefaultNamespaces => [
        "Microsoft.EntityFrameworkCore",
        "Microsoft.EntityFrameworkCore.ChangeTracking",
        "Microsoft.EntityFrameworkCore.Infrastructure",
        "Microsoft.EntityFrameworkCore.Metadata",
        "System.ComponentModel"
    ];

    public virtual StringBuilder GenerateUsings(StringBuilder sb)
    {
        sb.WriteUsings(_namespaces);

        return sb;
    }
    public virtual StringBuilder GenerateMandatoryConstructor(StringBuilder sb)
    {
        sb.Append("    public ").Append(EntityDescriptor.EntityType.Name).Append("Proxy(DbContext context, IEntityType entityType, ILazyLoader lazyLoader)").AppendLine();
        sb.Append("    {").AppendLine();
        sb.Append("        _context = context;").AppendLine();
        sb.Append("        _entityType = entityType;").AppendLine();
        sb.Append("        _lazyLoader = lazyLoader;").AppendLine();
        sb.WriteInitChildCollections(EntityDescriptor.EntityType, _collectionFields, _supportCollectionInitialized, 8);
        sb.Append("    }").AppendLine();

        return sb;
    }

    public virtual StringBuilder GenerateConstructor(StringBuilder sb, IMethodSymbol constructor)
    {
        sb.Append("    public ").Append(EntityDescriptor.EntityType.Name).Append("Proxy").AppendLine();
        sb.Append("    (").AppendLine();
        sb.Append("        DbContext context,").AppendLine();
        sb.Append("        IEntityType entityType,").AppendLine();
        sb.Append("        ILazyLoader lazyLoader,").AppendLine();
        sb.WriteConstructorParamDeclaration(constructor, 8).AppendLine();
        sb.Append("    )").AppendLine();
        sb.Append("        : base").AppendLine();
        sb.Append("        (").AppendLine();
        sb.WriteConstructorParamCall(constructor, 12).AppendLine();
        sb.Append("        )").AppendLine();
        sb.Append("    {").AppendLine();
        sb.Append("        _context = context;").AppendLine();
        sb.Append("        _entityType = entityType;").AppendLine();
        sb.Append("        _lazyLoader = lazyLoader;").AppendLine();
        sb.WriteInitChildCollections(EntityDescriptor.EntityType, _collectionFields, _supportCollectionInitialized, 8);
        sb.Append("    }").AppendLine();
        return sb;
    }

    public virtual StringBuilder GenerateSimpleProperty(StringBuilder sb, IPropertySymbol property)
    {
        if (property.SetMethod == null)
        {
            Context.ReportDiagnostic(Diagnostic.Create(SetterRequiredDescriptor, Location.None, EntityDescriptor.EntityType, property.Name));
        }
        sb.WhiteSimpleNotificationWrapper(property, 4);

        return sb;
    }

    public virtual StringBuilder GenerateNavigationProperty(StringBuilder sb, IPropertySymbol property)
    {
        if (property.SetMethod == null)
        {
            Context.ReportDiagnostic(Diagnostic.Create(SetterRequiredDescriptor, Location.None, EntityDescriptor.EntityType, property.Name));
        }
        sb.WhiteDomainNotificationWrapper(property, 4);

        return sb;
    }

    public virtual StringBuilder GenerateMandatoryFields(StringBuilder sb)
    {
        sb.AppendLine("    private readonly DbContext _context;");
        sb.AppendLine("    private readonly IEntityType _entityType;");
        sb.AppendLine("    private readonly ILazyLoader _lazyLoader;");
        sb.AppendLine("    private bool _initialized => _context != null;");
        sb.AppendLine();
        sb.AppendLine("    public event PropertyChangedEventHandler? PropertyChanged;");
        sb.AppendLine("    public event PropertyChangingEventHandler? PropertyChanging;");
        return sb;
    }

    public DefaultProxyClassGenerator(GeneratorExecutionContext context, EntityDescriptor entityDescriptor, DbContextDescriptor dbContextDescriptor, INamedTypeSymbol actionTypeSymbol)
    {
        Context = context;
        EntityDescriptor = entityDescriptor;
        DbContextDescriptor = dbContextDescriptor;
        // ReSharper disable once VirtualMemberCallInConstructor
        _namespaces = [..DefaultNamespaces, entityDescriptor.EntityType.ContainingNamespace.ToDisplayString()];

        foreach (IPropertySymbol property in entityDescriptor.EntityType.GetVirtualNotOverridenProperties())
        {
            if (property.Type is not INamedTypeSymbol type)
            {
                continue;
            }

            if (type.MetadataName == "ICollection`1")
            {
                if (dbContextDescriptor.EntityDescriptors.Any(e => e.EntityType.Equals(type.TypeArguments[0], SymbolEqualityComparer.Default)))
                {
                    _collectionFields.Add(property);
                    _namespaces.Add("System.Collections.Generic");
                    continue;
                }
                // simple field
            }

            if (type.MetadataName == "IQueryableCollection`1")
            {
                if (dbContextDescriptor.EntityDescriptors.Any(e => e.EntityType.Equals(type.TypeArguments[0], SymbolEqualityComparer.Default)))
                {
                    _collectionFields.Add(property);
                    _namespaces.Add("Penqueen.Collections");
                    continue;
                }
                // simple field
            }

            if (dbContextDescriptor.EntityDescriptors.Any(e => e.EntityType.Equals(type, SymbolEqualityComparer.Default)))
            {
                _entityFields.Add(property);
                continue;
            }

            _simpleFields.Add(property);
        }


        _constructors = entityDescriptor.EntityType.GetMembers().OfType<IMethodSymbol>()
            .Where(m => m.MethodKind == MethodKind.Constructor && m.Parameters.Any()).ToList();


        _supportCollectionInitialized = entityDescriptor.EntityType
            .GetMembersWithInherited()
            .OfType<IFieldSymbol>()
            .Any(s => s.Name == "CollectionsInitialized" && SymbolEqualityComparer.Default.Equals(s.Type, actionTypeSymbol));

    }

    public virtual string Generate()
    {
        var sb = new StringBuilder(2000);
        GenerateUsings(sb);
        sb.AppendLine();
        sb.Append("namespace ").Append(DbContextDescriptor.DbContextType.ContainingNamespace.ToDisplayString()).AppendLine(".Proxy;");
        sb.AppendLine();
        sb.Append("public class ").Append(EntityDescriptor.EntityType.Name).Append("Proxy : ").Append(EntityDescriptor.EntityType).AppendLine(", INotifyPropertyChanged, INotifyPropertyChanging");
        sb.Append("{").AppendLine();
        GenerateMandatoryFields(sb);
        sb.AppendLine();
        GenerateMandatoryConstructor(sb);
        sb.AppendLine();

        foreach (IMethodSymbol constructor in _constructors)
        {
            GenerateConstructor(sb, constructor);
            sb.AppendLine();
        }

        foreach (IPropertySymbol property in _simpleFields)
        {
            GenerateSimpleProperty(sb, property);
            sb.AppendLine();
        }

        foreach (IPropertySymbol property in _entityFields)
        {
            GenerateNavigationProperty(sb, property);
            sb.AppendLine();
        }
        sb.AppendLine("}");
        return sb.ToString();
    }

}