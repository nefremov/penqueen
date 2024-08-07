﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

using System.Collections;
using System.Collections.Specialized;
using System.Linq.Expressions;

namespace Penqueen.Collections;


public abstract class BackedObservableHashSet<TItem, TOwner> : IQueryableCollection<TItem>, INotifyCollectionChanged
    where TOwner : class
    where TItem : class
{
    protected DbContext Context { get; }
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

    private readonly ReadOnlyHashSet<TItem> _local;
    public IReadOnlyCollection<TItem> Local => _local;

    protected BackedObservableHashSet(
        ObservableHashSet<TItem> internalCollection,
        DbContext context, TOwner ownerEntity, Expression<Func<TOwner, IEnumerable<TItem>>> collectionAccessor,
        IEntityType entityType, ILazyLoader lazyLoader)
    {
        Context = context;
        EntityType = entityType;
        LazyLoader = lazyLoader;
        OwnerEntity = ownerEntity;
        _collectionAccessor = collectionAccessor;
        _storedCollection = internalCollection;
        _local = new ReadOnlyHashSet<TItem>(_storedCollection);
    }


    public IEnumerator<TItem> GetEnumerator() => _storedCollection.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    [Obsolete("This method is marked as obsolete to make the collection encapsulated")]
    public void Add(TItem item)
    {
        throw new NotSupportedException();
    }

    protected void AddInternal(TItem item)
    {
        _storedCollection.Add(item);
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
        if (CollectionEntry.IsLoaded)
        {
            _storedCollection.CopyTo(array, arrayIndex);
        }

        else
        {
            Load();
            _storedCollection.CopyTo(array, arrayIndex);
        }
    }

    public bool Remove(TItem item)
    {
        var result = Context.Remove(item);
        _storedCollection.Remove(item);
        return result.State == EntityState.Deleted || result.State == EntityState.Detached; // Detached == 0 so flag checking doesn't work
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
}