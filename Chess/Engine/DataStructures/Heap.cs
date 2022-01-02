using System;
using System.Runtime.CompilerServices;
using Engine.Interfaces;

namespace Engine.DataStructures
{
    public class Heap
    {
        private readonly int _degree;
        private readonly IMove[] _elements;
        private int _size;

        public Heap() : this(2, 128) { }

        public Heap(int degree, int size)
        {
            _degree = degree;
            _elements = new IMove[size];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int Parent(int i)
        {
            return (i - 1) / _degree;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int Left(int i)
        {
            return i * _degree + 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Swap(int i, int j)
        {
            var t = _elements[i];
            _elements[i] = _elements[j];
            _elements[j] = t;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void BubbleUp(int i, int newKey)
        {
            var parent = Parent(i);
            while (i > 0 && newKey.CompareTo(_elements[parent].Value) > 0)
            {
                Swap(i, parent);
                i = parent;
                parent = Parent(parent);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Heapify(int index)
        {
            var left = Left(index);
            if (left >= _size) return;

            var last = Math.Min(left + _degree, _size);
            var largest = index;
            for (var i = left; i < last; i++)
            {
                if (_elements[i].CompareTo(_elements[largest]) > 0)
                {
                    largest = i;
                }
            }

            if (largest == index) return;

            Swap(index, largest);
            Heapify(largest);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetCount()
        {
            return _size;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Insert(IMove element)
        {
            _elements[_size] = element;
            BubbleUp(_size++, element.Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IMove Maximum()
        {
            _size--;
            Swap(0, _size);
            Heapify(0);
            return _elements[_size];
        }
    }
}
