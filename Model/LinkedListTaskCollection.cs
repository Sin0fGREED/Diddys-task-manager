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

    public void Clear() { head = null; count = 0; }
}