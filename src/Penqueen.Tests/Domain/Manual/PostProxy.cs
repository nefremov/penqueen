using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

using System.ComponentModel;
using Microsoft.EntityFrameworkCore;

namespace Penqueen.Tests.Domain.Manual;

public class PostProxy : Post, INotifyPropertyChanged, INotifyPropertyChanging
{
    private readonly IEntityType _entityType;
    private readonly ILazyLoader _lazyLoader;
    private readonly DbContext _context;

    public PostProxy(BlogContext context, IEntityType entityType, ILazyLoader lazyLoader)
    {
        _context = context;
        _entityType = entityType;
        _lazyLoader = lazyLoader;
    }

    public PostProxy(Guid id, string name, Blog blog, DbContext context, IEntityType entityType, ILazyLoader lazyLoader)
        : base(id, name, blog)
    {
        _context = context;
        _entityType = entityType;
        _lazyLoader = lazyLoader;
    }

    //public PostProxy(BlogContext context, IEntityType entityType, ILazyLoader lazyLoader, Guid id, string name, Blog blog)
    //    : base(id, name, blog)
    //{
    //    _context = context;
    //    _entityType = entityType;
    //    _lazyLoader = lazyLoader;
    //}
    //public PostProxy(BlogContext context, IEntityType entityType, ILazyLoader lazyLoader, params object[] arguments)
    //: this(context, entityType, lazyLoader, (Guid)arguments[0], (string)arguments[1], (Blog)arguments[2])
    //{ }

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

    public override string Text
    {
        set
        {
            if (value != base.Text)
            {
                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(Text)));
                base.Text = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
            }
        }
    }

    private bool _BlogIsLoaded = false;
    public override Blog Blog
    {
        set
        {
            if (value != base.Blog)
            {
                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(Blog)));
                base.Blog = value;
                _BlogIsLoaded = true;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Blog)));
            }
        }
        get
        {
            if (!_BlogIsLoaded)
            {
                var navigationName = "Blog";
                var navigationBase = _entityType.FindNavigation(navigationName) ?? (INavigationBase?)_entityType.FindSkipNavigation(navigationName);

                if (navigationBase != null && !(navigationBase is INavigation navigation && navigation.ForeignKey.IsOwnership))
                {
                    _lazyLoader.Load(this, navigationName);
                    _BlogIsLoaded = true;
                }
            }

            return base.Blog;
        }
    }

    public override Guid BlogId
    {
        set
        {
            if (value != base.BlogId)
            {
                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(BlogId)));
                base.BlogId = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BlogId)));
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public event PropertyChangingEventHandler? PropertyChanging;
}