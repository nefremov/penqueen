﻿using Penqueen.Types;

namespace Penqueen.Tests.Domain.Generated;
[DeclareCollection]
public partial class Post
{
    public virtual Guid Id { get; set; }
    public virtual string Text { get; set; }
    public virtual Guid BlogId { get; set; }
    public virtual Blog Blog { get; set; }

    protected Post()
    {
    }

    public Post(Guid id, string text, Blog blog)
    {
        Id = id;
        Text = text;
        Blog = blog;
    }
}


