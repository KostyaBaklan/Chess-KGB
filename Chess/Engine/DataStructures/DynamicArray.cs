﻿using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Engine.DataStructures
{
    public class DynamicArray<T>:IReadOnlyCollection<T>
    {
        private readonly T[] _items;

        public DynamicArray() : this(32) { }

        private DynamicArray(int capacity)
        {
            Count = 0;
            _items = new T[capacity];
        }

        public T this[int i]
        {
            get { return _items[i]; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T item)
        {
            _items[Count++] = item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            Count = 0;
        }

        #region Implementation of IEnumerable

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return _items[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Implementation of IReadOnlyCollection<out T>

        public int Count { get; private set; }

        #endregion
    }
}