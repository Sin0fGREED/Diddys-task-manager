public class ArrayTaskCollection<T> : ITaskCollection<T>
{
    private T[] items = new T[0];
    public int Count => items.Length;
    public bool Dirty { get; set; } = false;

    public void Add(T item)
    {
        T[] newItems = new T[items.Length + 1];
        for (int i = 0; i < items.Length; i++)
            newItems[i] = items[i];
        newItems[items.Length] = item;
        items = newItems;
        Dirty = true;
    }

    public bool Remove(T item)
    {
        int index = -1;
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i]?.Equals(item) == true)
            {
                index = i;
                break;
            }
        }
        if (index == -1) return false;
        T[] newItems = new T[items.Length - 1];
        for (int i = 0, j = 0; i < items.Length; i++)
        {
            if (i != index)
                newItems[j++] = items[i];
        }
        items = newItems;
        Dirty = true;
        return true;
    }

    public T FindById(int id)
    {
        for (int i = 0; i < items.Length; i++)
        {
            var prop = items[i]?.GetType().GetProperty("Id");
            if (prop != null && (int?)(prop.GetValue(items[i])) == id)
                return items[i];
        }
        return default(T) ?? throw new InvalidOperationException("No item with the specified ID found.");
    }

    public ITaskCollection<T> Filter(Func<T, bool> predicate)
    {
        var filtered = new ArrayTaskCollection<T>();
        foreach (var item in items)
        {
            if (predicate(item))
                filtered.Add(item);
        }
        return filtered;
    }

    public void Sort(Comparison<T> comparison)
    {
        Array.Sort(items, comparison);
        Dirty = true;
    }

    public R Reduce<R>(R initial, Func<R, T, R> accumulator)
    {
        R result = initial;
        foreach (var item in items)
        {
            result = accumulator(result, item);
        }
        return result;
    }

    public IMyIterator<T> GetIterator()
    {
        return new MyIterator<T>(items);
    }

    public IMyIterator<T> GetEnumerator()
    {
        return GetIterator();
    }

    public T[] ToArray() => items;

    public void Clear() { items = new T[0]; Dirty = true; }

    private class MyIterator<U> : IMyIterator<U>
    {
        private readonly U[] _array;
        private int _index;
        public MyIterator(U[] array)
        {
            _array = array;
            _index = 0;
        }
        public bool HasNext()
        {
            return _index < _array.Length;
        }
        public U Next()
        {
            if (!HasNext()) throw new InvalidOperationException("No more elements.");
            return _array[_index++];
        }
        public void Reset()
        {
            _index = 0;
        }
    }
}