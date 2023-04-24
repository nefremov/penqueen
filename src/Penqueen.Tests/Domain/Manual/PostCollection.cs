using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

using System.Linq.Expressions;

namespace Penqueen.Tests.Domain.Manual;


public class PostCollection<T> : BackedObservableHashSet<Post, T, BlogContext>, Blog.IPostCollection where T : class
{
    private readonly DbSet<Post> _set;

    public PostCollection(ObservableHashSet<Post> internalCollection, BlogContext context, DbSet<Post> set,
        T ownerEntity, Expression<Func<T, IEnumerable<Post>>> collectionAccessor, IEntityType entityType, ILazyLoader lazyLoader)
        : base(internalCollection, context, ownerEntity, collectionAccessor, entityType, lazyLoader)
    {
        _set = set;
    }


    public Post CreateNew(Guid id, string name, Blog blog)
    {
        var post = new PostProxy(id, name, blog, Context, EntityType, LazyLoader);
        _set.Add(post);
        return post;
    }
}