using Penqueen.Types;

namespace Penqueen.Tests.Domain.Generated;

[DeclareCollection]
public class Blog
{

    public virtual Guid Id { get; set; }
    public virtual string Name { get; set; }
    public virtual int? Sample { get; protected set; }

    protected ICollection<Post> _posts;
    public virtual ICollection<Post> Posts { get; protected set; }

    protected Blog() { }

    protected Blog(Guid id, string name, int? sample = null)
    {
        Id = id;
        Name = name;
        Sample = sample;
    }

    public Post AddPost(Guid id, string text)
    {
        var post = ((IPostCollection)Posts).CreateNew(id, text, this);
        _posts.Add(post);
        return post;
    }
}

