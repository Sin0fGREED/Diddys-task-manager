using System;

namespace Model
{
    public class BinarySearchTreeTaskCollection : ITaskCollection<TaskItem>
    {
        private class Node
        {
            public TaskItem Data;
            public Node? Left;
            public Node? Right;
            public Node(TaskItem data)
            {
                Data = data;
            }
        }

        private Node? root;
        private int count = 0;
        public bool Dirty { get; set; } = false;
        public int Count => count;

        public void Add(TaskItem item)
        {
            root = Insert(root, item);
            count++;
            Dirty = true;
        }

        private Node Insert(Node? node, TaskItem item)
        {
            if (node == null)
                return new Node(item);
            if (item.Id < node.Data.Id)
                node.Left = Insert(node.Left, item);
            else if (item.Id > node.Data.Id)
                node.Right = Insert(node.Right, item);
            else
                node.Data = item;
            return node;
        }

        public bool Remove(TaskItem item)
        {
            bool removed;
            (root, removed) = Remove(root, item.Id);
            if (removed) { count--; Dirty = true; }
            return removed;
        }

        private (Node?, bool) Remove(Node? node, int id)
        {
            if (node == null) return (null, false);
            if (id < node.Data.Id)
            {
                (node.Left, var removed) = Remove(node.Left, id);
                return (node, removed);
            }
            else if (id > node.Data.Id)
            {
                (node.Right, var removed) = Remove(node.Right, id);
                return (node, removed);
            }
            else
            {
                if (node.Left == null) return (node.Right, true);
                if (node.Right == null) return (node.Left, true);
                Node successor = MinValueNode(node.Right);
                node.Data = successor.Data;
                (node.Right, _) = Remove(node.Right, successor.Data.Id);
                return (node, true);
            }
        }

        private Node MinValueNode(Node node)
        {
            Node current = node;
            while (current.Left != null)
                current = current.Left;
            return current;
        }

        public TaskItem FindById(int id)
        {
            Node? current = root;
            while (current != null)
            {
                if (id == current.Data.Id) return current.Data;
                if (id < current.Data.Id) current = current.Left;
                else current = current.Right;
            }
            throw new InvalidOperationException("No item with the specified ID found.");
        }

        public ITaskCollection<TaskItem> Filter(Func<TaskItem, bool> predicate)
        {
            var filtered = new BinarySearchTreeTaskCollection();
            foreach (var item in ToArray())
            {
                if (predicate(item))
                    filtered.Add(item);
            }
            return filtered;
        }

        public void Sort(Comparison<TaskItem> comparison)
        {
            throw new NotSupportedException("Sort is not supported for BinarySearchTreeTaskCollection.");
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
            return new BSTIterator(ToArray());
        }

        public IMyIterator<TaskItem> GetEnumerator()
        {
            return GetIterator();
        }

        public TaskItem[] ToArray()
        {
            TaskItem[] arr = new TaskItem[count];
            int idx = 0;
            InOrder(root, arr, ref idx);
            return arr;
        }

        private void InOrder(Node? node, TaskItem[] arr, ref int idx)
        {
            if (node == null) return;
            InOrder(node.Left, arr, ref idx);
            arr[idx++] = node.Data;
            InOrder(node.Right, arr, ref idx);
        }

        public void Clear()
        {
            root = null;
            count = 0;
            Dirty = true;
        }

        private class BSTIterator : IMyIterator<TaskItem>
        {
            private readonly TaskItem[] _items;
            private int _index;
            public BSTIterator(TaskItem[] items)
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
