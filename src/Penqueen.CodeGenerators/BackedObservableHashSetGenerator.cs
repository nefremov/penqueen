using System.Text;
using Microsoft.CodeAnalysis;

namespace Penqueen.CodeGenerators;

public class BackedObservableHashSetGenerator
{
    public BackedObservableHashSetGenerator()
    {
    }

    public string Generate()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine(@"
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

using System.Collections;
using System.Collections.Specialized;
using System.Linq.Expressions;

namespace Penqueen.Types;
");
        stringBuilder.AppendLine(@"
public abstract class BackedObservableHashSet<TItem, TOwner, TContext> : ICollection<TItem>, IQueryable<TItem>, INotifyCollectionChanged
    where TOwner : class
    where TItem : class
    where TContext : DbContext
{
    protected TContext Context { get; }
    protected IEntityType EntityType { get; }
    protected ILazyLoader LazyLoader { get; }

    protected readonly TOwner OwnerEntity;
    private readonly Expression<Func<TOwner, IEnumerable<TItem>>> _collectionAccessor;

    private EntityEntry<TOwner>? _entityEntry;
    private EntityEntry<TOwner> Entry => _entityEntry ??= Context.Entry(OwnerEntity);


    private CollectionEntry<TOwner, TItem>? _collectionEntry;
    protected CollectionEntry<TOwner, TItem> CollectionEntry => _collectionEntry ??= Entry.Collection(_collectionAccessor);

    private IQueryable<TItem>? _query;
    protected IQueryable<TItem> Query => _query ??= CollectionEntry.Query();

    private readonly ObservableHashSet<TItem> _storedCollection;

    public BackedObservableHashSet(
        ObservableHashSet<TItem> internalCollection,
        TContext context, TOwner ownerEntity, Expression<Func<TOwner, IEnumerable<TItem>>> collectionAccessor,
        IEntityType entityType, ILazyLoader lazyLoader)
    {
        Context = context;
        EntityType = entityType;
        LazyLoader = lazyLoader;
        OwnerEntity = ownerEntity;
        _collectionAccessor = collectionAccessor;
        _storedCollection = internalCollection;
    }

    public IEnumerator<TItem> GetEnumerator() => _storedCollection.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    [Obsolete]
    public void Add(TItem item)
    {
        throw new NotSupportedException();
    }

    public void Load()
    {
        CollectionEntry.Load();
    }

    public void Clear()
    {
        _storedCollection.Clear();
    }

    public bool Contains(TItem item)
    {
        //Query.Any()

        return _storedCollection.Contains(item);
    }

    public void CopyTo(TItem[] array, int arrayIndex)
    {
        throw new NotSupportedException();
    }

    public bool Remove(TItem item)
    {
        throw new NotSupportedException();
    }

    public int Count => _storedCollection.Count;

    public bool IsReadOnly => false;

    public event NotifyCollectionChangedEventHandler? CollectionChanged
    {
        add => _storedCollection.CollectionChanged += value;
        remove => _storedCollection.CollectionChanged -= value;
    }

    Type IQueryable.ElementType => Query.ElementType;

    Expression IQueryable.Expression => Query.Expression;

    IQueryProvider IQueryable.Provider => Query.Provider;
}");

        return stringBuilder.ToString();
    }
}