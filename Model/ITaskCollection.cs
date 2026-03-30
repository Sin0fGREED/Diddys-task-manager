public interface ITaskCollection<T>
{
    void Add(T item);
    bool Remove(T item);
    T FindById(int id);
    T[] ToArray();
    int Count { get; }
    void Clear();
}