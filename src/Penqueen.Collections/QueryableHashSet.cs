using Microsoft.EntityFrameworkCore.ChangeTracking;

using System.Linq.Expressions;

namespace Penqueen.Collections
{
    public class QueryableHashSet<T> : ObservableHashSet<T>, IQueryableCollection<T>
    {
        public void Load()
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<T> Local => this;

        public Type ElementType => throw new NotImplementedException();
        public Expression Expression => throw new NotImplementedException();
        public IQueryProvider Provider => throw new NotImplementedException();
    }
}
