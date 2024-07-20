using System.Collections;

namespace Penqueen.Collections;

public class ReadOnlyHashSet<T> : IReadOnlySet<T>
{
    private readonly ISet<T> _source;

    public ReadOnlyHashSet(ISet<T> source)
    {
        _source = source;
    }

    public IEnumerator<T> GetEnumerator() => _source.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => _source.Count;
    public bool Contains(T item) => _source.Contains(item);

    public bool IsProperSubsetOf(IEnumerable<T> other) => _source.IsProperSubsetOf(other);

    public bool IsProperSupersetOf(IEnumerable<T> other) => _source.IsProperSupersetOf(other);

    public bool IsSubsetOf(IEnumerable<T> other) => _source.IsSubsetOf(other);

    public bool IsSupersetOf(IEnumerable<T> other) => _source.IsSupersetOf(other);

    public bool Overlaps(IEnumerable<T> other)=> _source.Overlaps(other);

    public bool SetEquals(IEnumerable<T> other)=> _source.SetEquals(other);
}