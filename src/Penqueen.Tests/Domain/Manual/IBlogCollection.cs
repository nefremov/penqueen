using Penqueen.CodeGenerators;
using Penqueen.Collections;

namespace Penqueen.Tests.Domain.Manual;

public interface IBlogCollection : IQueryableCollection<Blog>
{
    Blog CreateNew
    (
        Guid id,
        string name,
        SampleEnum enumParam = SampleEnum.Second,
        int? sample = null
    );
}