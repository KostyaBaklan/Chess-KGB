using System.Collections.Generic;

namespace Engine.DataStructures
{
    class Node<T>
    {
        public Node(T value)
        {
            Value = value;
        }

        public T Value { get; }
        public Node<T> Next { get; set; }
    }

    public class DynamicSortedList<T>
    {
        private Node<T> _root;
        private readonly IComparer<T> _comparer;

        public DynamicSortedList(IComparer<T> comparer, T root)
        {
            _comparer = comparer;
            _root = new Node<T>(root);
            Count = 1;
        }

        public int Count { get; private set; }

        public T Pop()
        {
            if (Count <= 0) return default(T);
            Node<T> temp = null;
            temp = _root;
            _root = temp.Next;
            Count--;
            return temp.Value;

        }

        public void Push(T item)
        {
            var current = _root;
            Node<T> previous = null;
            while (current != null)
            {
                if (_comparer.Compare(item, current.Value) < 0)
                {
                    if (previous == null)
                    {
                        _root = new Node<T>(item) { Next = _root };
                    }
                    else
                    {
                        var node = new Node<T>(item) { Next = current };
                        previous.Next = node;
                    }

                    break;
                }

                previous = current;
                current = current.Next;
            }

            if (current == null)
            {
                previous.Next = new Node<T>(item);
            }

            Count++;
        }
    }
}
