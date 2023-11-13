using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

using System.ComponentModel;

namespace Penqueen.Tests.Domain.Manual;

public class BlogProxy : Blog, INotifyPropertyChanged, INotifyPropertyChanging
{
    private readonly DbContext _context;
    private readonly IEntityType _entityType;
    private readonly ILazyLoader _lazyLoader;

    public BlogProxy(BlogContext context, IEntityType entityType, ILazyLoader lazyLoader)
    {
        _context = context;
        _entityType = entityType;
        _lazyLoader = lazyLoader;
        _posts = new ObservableHashSet<Post>();
        Posts = new PostCollection<Blog>((ObservableHashSet<Post>)_posts, _context, this, _ => _.Posts,
            entityType, lazyLoader);
    }

    public BlogProxy(Guid id, string name, int? sample, DbContext context, IEntityType entityType, ILazyLoader lazyLoader)
        : base(id, name, sample)
    {
        _context = context;
        _entityType = entityType;
        _lazyLoader = lazyLoader;
        _posts = new ObservableHashSet<Post>();
        Posts = new PostCollection<Blog>((ObservableHashSet<Post>)_posts, _context,  this, _ => _.Posts,
            entityType, lazyLoader);
    }

    //public BlogProxy(BlogContext context, IEntityType entityType, ILazyLoader lazyLoader, params object[] arguments)
    //    : this(context, entityType, lazyLoader, (Guid)arguments[0], (string)arguments[1], (int?)arguments[2])
    //{
    //    Posts = new PostCollection<Blog>((ObservableHashSet<Post>)_posts, _context, this, _ => _.Posts,
    //        entityType, lazyLoader);
    //}

    public override Guid Id
    {
        set
        {
            if (value != base.Id)
            {
                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(Id)));
                base.Id = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Id)));
            }
        }
    }

    public override string Name
    {
        set
        {
            if (value != base.Name)
            {
                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(Name)));
                base.Name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public event PropertyChangingEventHandler? PropertyChanging;
}