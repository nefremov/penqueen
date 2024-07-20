namespace Penqueen.Collections;

public interface IQueryableCollection<T> : ICollection<T>, IQueryable<T>
{
    void Load();
    IReadOnlyCollection<T> Local { get; }
}

