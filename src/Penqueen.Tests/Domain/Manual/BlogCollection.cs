﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

using Penqueen.Collections;

using System.Linq.Expressions;
using Penqueen.CodeGenerators;

namespace Penqueen.Tests.Domain.Manual;


public class BlogCollection<T> : BackedObservableHashSet<Blog, T>, IBlogCollection where T : class
{
    public BlogCollection
    (
        ObservableHashSet<Blog> internalCollection,
        DbContext context,
        T ownerEntity,
        Expression<Func<T, IEnumerable<Blog>>> collectionAccessor,
        IEntityType entityType,
        ILazyLoader lazyLoader
    )
        : base(internalCollection, context, ownerEntity, collectionAccessor, entityType, lazyLoader)
    {
    }


    public Blog CreateNew(
        Guid id,
        string name,
        SampleEnum enumProp = SampleEnum.Second,
        int? sample = null
    )
    {
        var blog = new BlogProxy
        (
            Context,
            EntityType,
            LazyLoader,
            id,
            name,
            enumProp,
            sample
        );
        AddInternal(blog);
        return blog;
    }
}