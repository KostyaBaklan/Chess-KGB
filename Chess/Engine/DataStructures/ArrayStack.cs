using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Engine.DataStructures
{
    public class ArrayStack<T>
    {
        private readonly T[] _items;

        public ArrayStack():this(256)
        {
        }

        public ArrayStack(int size)
        {
            Count = 0;
            _items = new T[size];
        }

        public int Count { get; private set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(T item)
        {
            _items[Count++] = item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Pop()
        {
            return _items[--Count];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Peek()
        {
            return Count == 0 ? default(T) : _items[Count - 1];
        }

        public T this[int i]
        {
            get { return _items[i]; }
        }

        public IEnumerable<T> Items()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return _items[i];
            }
        }
    }
}
