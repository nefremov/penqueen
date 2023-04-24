using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Penqueen.Tests.Domain.Manual;

public partial class Blog
{

    protected ICollection<Post> _posts = new ObservableHashSet<Post>();
    public interface IPostCollection : ICollection<Post>
    {
        Post CreateNew(Guid id, string name, Blog blog);
    }
}