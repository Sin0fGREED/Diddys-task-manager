public class LinkedListTaskCollection<T> : ITaskCollection<T>
{
    private class Node
    {
        public T Data;
        public Node? Next;
        public Node(T data) { Data = data; }
    }
    private Node? head;
    private int count;
    public int Count => count;
    public bool Dirty { get; set; } = false;

    public void Add(T item)
    {
        Node newNode = new Node(item);
        if (head == null)
            head = newNode;
        else
        {
            Node current = head;
            while (current.Next != null)
                current = current.Next;
            current.Next = newNode;
        }
        count++;
        Dirty = true;
    }

    public bool Remove(T item)
    {
        Node? current = head;
        Node? prev = null;
        while (current != null)
        {
            if (current.Data?.Equals(item) == true)
            {
                if (prev == null) head = current.Next;
                else prev.Next = current.Next;
                count--;
                Dirty = true;
                return true;
            }
            prev = current;
            current = current.Next;
        }
        return false;
    }

    public T FindById(int id)
    {
        Node? current = head;
        while (current != null)
        {
            var prop = current.Data?.GetType().GetProperty("Id");
            if (prop != null && (int?)(prop.GetValue(current.Data)) == id)
                return current.Data;
            current = current.Next;
        }
        return default(T) ?? throw new InvalidOperationException("No item with the specified ID found.");
    }

    public T[] ToArray()
    {
        T[] arr = new T[count];
        Node? current = head;
        int i = 0;
        while (current != null)
        {
            arr[i++] = current.Data;
            current = current.Next;
        }
        return arr;
    }

    public ITaskCollection<T> Filter(Func<T, bool> predicate)
    {
        var filtered = new LinkedListTaskCollection<T>();
        Node? current = head;
        while (current != null)
        {
            if (predicate(current.Data))
                filtered.Add(current.Data);
            current = current.Next;
        }
        return filtered;
    }

    public void Sort(Comparison<T> comparison)
    {
        T[] arr = ToArray();
        Array.Sort(arr, comparison);
        Clear();
        foreach (var item in arr)
            Add(item);
        Dirty = true;
    }

    public R Reduce<R>(R initial, Func<R, T, R> accumulator)
    {
        R result = initial;
        Node? current = head;
        while (current != null)
        {
            result = accumulator(result, current.Data);
            current = current.Next;
        }
        return result;
    }

    public IMyIterator<T> GetIterator()
    {
        return new MyIterator(head);
    }

    public IMyIterator<T> GetEnumerator()
    {
        return GetIterator();
    }

    public void Clear() { head = null; count = 0; Dirty = true; }

    private class MyIterator : IMyIterator<T>
    {
        private Node? _current;
        private readonly Node? _head;
        private bool _started;
        public MyIterator(Node? head)
        {
            _head = head;
            _current = head;
            _started = false;
        }
        public bool HasNext()
        {
            if (!_started) return _current != null;
            return _current != null && _current.Next != null;
        }
        public T Next()
        {
            if (!_started)
            {
                _started = true;
                if (_current == null) throw new InvalidOperationException("No more elements.");
                return _current.Data;
            }
            if (_current == null || _current.Next == null) throw new InvalidOperationException("No more elements.");
            _current = _current.Next;
            return _current.Data;
        }
        public void Reset()
        {
            _current = _head;
            _started = false;
        }
    }
}