public class ArrayTaskCollection<T> : ITaskCollection<T>
{
    private T[] items = new T[0];
    public int Count => items.Length;

    public void Add(T item)
    {
        T[] newItems = new T[items.Length + 1];
        for (int i = 0; i < items.Length; i++)
            newItems[i] = items[i];
        newItems[items.Length] = item;
        items = newItems;
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

    public T[] ToArray() => items;

    public void Clear() { items = new T[0]; }
}