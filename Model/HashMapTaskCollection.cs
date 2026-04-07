using System;

namespace Model
{
    public class HashMapTaskCollection : ITaskCollection<TaskItem>
    {
        private const int InitialCapacity = 16;
        private Entry[] _buckets = new Entry[InitialCapacity];
        private int _count = 0;
        public bool Dirty { get; set; } = false;

        private class Entry
        {
            public int Key;
            public TaskItem Value;
            public Entry? Next;
            public Entry(int key, TaskItem value, Entry? next)
            {
                Key = key;
                Value = value;
                Next = next;
            }
        }

        public int Count => _count;

        private int GetBucketIndex(int key)
        {
            return Math.Abs(key.GetHashCode()) % _buckets.Length;
        }

        public void Add(TaskItem item)
        {
            int key = item.Id;
            int index = GetBucketIndex(key);
            Entry? current = _buckets[index];
            while (current != null)
            {
                if (current.Key == key)
                {
                    current.Value = item;
                    Dirty = true;
                    return;
                }
                current = current.Next;
            }
            _buckets[index] = new Entry(key, item, _buckets[index]);
            _count++;
            Dirty = true;
        }

        public bool Remove(TaskItem item)
        {
            int key = item.Id;
            int index = GetBucketIndex(key);
            Entry? current = _buckets[index];
            Entry? prev = null;
            while (current != null)
            {
                if (current.Key == key)
                {
                    if (prev == null)
                    {
                        _buckets[index] = current.Next!;
                    }
                    else
                        prev.Next = current.Next;
                    _count--;
                    Dirty = true;
                    return true;
                }
                prev = current;
                current = current.Next;
            }
            return false;
        }

        public TaskItem FindById(int id)
        {
            int index = GetBucketIndex(id);
            Entry? current = _buckets[index];
            while (current != null)
            {
                if (current.Key == id)
                    return current.Value;
                current = current.Next;
            }
            throw new InvalidOperationException("No item with the specified ID found.");
        }

        public ITaskCollection<TaskItem> Filter(Func<TaskItem, bool> predicate)
        {
            var filtered = new HashMapTaskCollection();
            foreach (var item in ToArray())
            {
                if (predicate(item))
                    filtered.Add(item);
            }
            return filtered;
        }

        public void Sort(Comparison<TaskItem> comparison)
        {
            throw new NotSupportedException("Sort is not supported for HashMapTaskCollection.");
        }

        public R Reduce<R>(R initial, Func<R, TaskItem, R> accumulator)
        {
            R result = initial;
            foreach (var item in ToArray())
            {
                result = accumulator(result, item);
            }
            return result;
        }

        public IMyIterator<TaskItem> GetIterator()
        {
            return new HashMapTaskIterator(ToArray());
        }

        public IMyIterator<TaskItem> GetEnumerator()
        {
            return GetIterator();
        }

        public TaskItem[] ToArray()
        {
            TaskItem[] arr = new TaskItem[_count];
            int idx = 0;
            for (int i = 0; i < _buckets.Length; i++)
            {
                Entry? current = _buckets[i];
                while (current != null)
                {
                    arr[idx++] = current.Value;
                    current = current.Next;
                }
            }
            return arr;
        }

        public void Clear()
        {
            _buckets = new Entry[InitialCapacity];
            _count = 0;
            Dirty = true;
        }

        private class HashMapTaskIterator : IMyIterator<TaskItem>
        {
            private readonly TaskItem[] _items;
            private int _index;
            public HashMapTaskIterator(TaskItem[] items)
            {
                _items = items;
                _index = 0;
            }
            public bool HasNext()
            {
                return _index < _items.Length;
            }
            public TaskItem Next()
            {
                if (!HasNext()) throw new InvalidOperationException("No more elements.");
                return _items[_index++];
            }
            public void Reset()
            {
                _index = 0;
            }
        }
    }
}
