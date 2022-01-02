using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Engine.DataStructures
{
    public class ArrayStack<T>
    {
        private int _current;
        private readonly T[] _items;

        public ArrayStack()
        {
            _current = 0;
            _items = new T[256];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(T item)
        {
            _items[_current++] = item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Pop()
        {
            return _items[--_current];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Peek()
        {
            return _current == 0 ? default(T) : _items[_current - 1];
        }

        public T this[int i]
        {
            get { return _items[i]; }
        }

        public IEnumerable<T> Items()
        {
            for (int i = 0; i < _current; i++)
            {
                yield return _items[i];
            }
        }
    }
}
