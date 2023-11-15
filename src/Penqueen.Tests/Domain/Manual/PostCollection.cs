using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Metadata;

using Penqueen.Collections;

using System.Linq.Expressions;

namespace Penqueen.Tests.Domain.Manual;


public class PostCollection<T> : BackedObservableHashSet<Post, T>, IPostCollection where T : class
{
    public PostCollection
    (
        ObservableHashSet<Post> internalCollection,
        DbContext context,
        T ownerEntity,
        Expression<Func<T, IEnumerable<Post>>> collectionAccessor,
        IEntityType entityType,
        ILazyLoader lazyLoader
    )
        : base(internalCollection, context, ownerEntity, collectionAccessor, entityType, lazyLoader)
    {
    }


    public Post CreateNew
    (
        Guid id,
        string name,
        Blog blog
    )
    {
        var post = new PostProxy
        (
            Context,
            EntityType,
            LazyLoader,
            id,
            name,
            blog
        );
        Context.Add(post);
        return post;
    }
}