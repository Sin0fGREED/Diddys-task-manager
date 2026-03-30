public interface ITaskCollection<T>
{
    void Add(T item);
    bool Remove(T item);
    T FindById(int id);
    ITaskCollection<T> Filter(Func<T, bool> predicate);
    void Sort(Comparison<T> comparison);
    int Count { get; }
    bool Dirty { get; set; }
    R Reduce<R>(R initial, Func<R, T, R> accumulator);
    IMyIterator<T> GetIterator();
    IMyIterator<T> GetEnumerator();
    T[] ToArray();
    void Clear();
}

public interface IMyIterator<T>
{
    bool HasNext();
    T Next();
    void Reset();
}