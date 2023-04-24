namespace Penqueen.Tests.Domain.Manual
{
    public class Post
    {
        public virtual Guid Id { get; set; }
        public virtual string Text { get; set; }
        public virtual Guid BlogId { get; set; }
        public virtual Blog Blog { get; set; }

        protected Post()
        {
        }

        protected Post(Guid id, string text, Blog blog)
        {
            Id = id;
            Text = text;
            Blog = blog;
        }
    }

    //public class PostCollection<T> : ICollection<Post>, INotifyCollectionChanged, IQueryable<Post> where T : class
    //{
    //    private static FieldInfo suspendedField =
    //        typeof(ChangeDetector).GetField("_suspended", BindingFlags.Instance | BindingFlags.NonPublic);

    //    private readonly BlogContext _context;
    //    private readonly T _ownerEntity;
    //    private readonly IEntityType _entityType;
    //    private readonly ILazyLoader _lazyLoader;
    //    private readonly Expression<Func<T, IEnumerable<Post>>> _filter;

    //    private EntityEntry<T> _entityEntry;

    //    private EntityEntry<T> Entry => _entityEntry ??= _context.Entry(_ownerEntity);
    //    private IQueryable<Post>? _query;

    //    private ObservableHashSet<Post> _storedCollection = new ObservableHashSet<Post>();


    //    private IQueryable<Post> Query
    //    {
    //        get
    //        {
    //            if (_query != null)
    //                return _query;
    //            else
    //            {
    //                var _query = Entry.Collection(_filter).Query();

    //                return _query;
    //            }
    //        }
    //    }

    //    public PostCollection(BlogContext context, T ownerEntity, IEntityType entityType, ILazyLoader lazyLoader, Expression<Func<T, IEnumerable<Post>>> collectionAccessor)
    //    {
    //        _context = context;
    //        _ownerEntity = ownerEntity;
    //        _entityType = entityType;
    //        _lazyLoader = lazyLoader;
    //        _filter = collectionAccessor;

    //        //_entityEntry = context.Entry(ownerEntity);
    //    }

    //    public IEnumerator<Post> GetEnumerator()
    //    {
    //        return _storedCollection.GetEnumerator();
    //    }

    //    IEnumerator IEnumerable.GetEnumerator()
    //    {
    //        return GetEnumerator();
    //    }

    //    [Obsolete]
    //    public void Add(Post item)
    //    {
    //        if (UseLocalCollection)
    //        {
    //            _storedCollection.Add(item);
    //        }
    //        else
    //        {
    //            //_context.Posts.Add(item);
    //        }
    //    }

    //    private bool UseLocalCollection =>
    //        Entry.State == EntityState.Added || Entry.State == EntityState.Detached;

    //    private bool TrackingSuspended => (bool)suspendedField.GetValue(_context.GetDependencies().ChangeDetector);
    //    public Post Add(Guid id, string name, Blog blog)
    //    {
    //        PostProxy item = new PostProxy(id, name, blog, _context, _entityType, _lazyLoader);
    //        if (UseLocalCollection)
    //        {
    //            _storedCollection.Add(item);
    //        }
    //        else
    //        {
    //            _context.Posts.Add(item);
    //        }

    //        return item;
    //    }

    //    public void Clear()
    //    {
    //        throw new NotSupportedException();
    //    }

    //    public bool Contains(Post item)
    //    {

    //        return UseLocalCollection || TrackingSuspended || _context.Entry(item).State == EntityState.Added 
    //            ? _storedCollection.Contains(item) 
    //            : Query.Any(_ => _.Id == item.Id);
    //    }

    //    public void CopyTo(Post[] array, int arrayIndex)
    //    {
    //        throw new NotSupportedException();
    //    }

    //    public bool Remove(Post item)
    //    {
    //        return UseLocalCollection? _storedCollection.Remove(item) : _context.Posts.Remove(item).State != EntityState.Unchanged; // Unchanged will be for not existing or Detached
    //    }

    //    public int Count => _storedCollection.Count;
    //    public bool IsReadOnly => false;

    //    public event NotifyCollectionChangedEventHandler? CollectionChanged
    //    {
    //        add => _storedCollection.CollectionChanged += value;
    //        remove => _storedCollection.CollectionChanged -= value;
    //    }

    //    Type IQueryable.ElementType => Query.ElementType;

    //    Expression IQueryable.Expression => Query.Expression;

    //    IQueryProvider IQueryable.Provider => Query.Provider;
    //}
}
