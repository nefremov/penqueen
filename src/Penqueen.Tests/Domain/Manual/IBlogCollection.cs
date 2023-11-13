using Penqueen.Collections;

namespace Penqueen.Tests.Domain.Manual;

public interface IBlogCollection : IQueryableCollection<Blog>
{
    Blog CreateNew(Guid id, string name, int? sample);
}