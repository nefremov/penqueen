using Penqueen.Collections;

namespace Penqueen.Tests.Domain.Manual;

public interface IPostCollection : IQueryableCollection<Post>
{
    Post CreateNew(Guid id, string name, Blog blog);
}