using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Interfaces.Moves;

namespace Infrastructure.DataStructures
{
    public class Heap
    {
        private IMove[] _items;
        private int _size;

        public Heap(IEnumerable<IMove> moves)
        {
            _items = moves.ToArray();
            _size = _items.Length;
            for (var i = _size / 2; i >= 0; i--)
            {
                Heapify(i);
            }
        }

        private void Insert(IEnumerable<IMove> moves)
        {
            _size = 0;
            _items = new IMove[32];
            foreach (var move in moves)
            {
                if (_items.Length == _size)
                {
                    _items = Resize();
                }

                _items[_size] = move;
                int i = _size;
                _size++;

                var parent = (i - 1) / 2;
                while (i > 0 && move.History > _items[parent].History)
                {
                    var t = _items[i];
                    _items[i] = _items[parent];
                    _items[parent] = t;
                    i = parent;
                    parent = (parent - 1) / 2;
                }
            }
        }

        private IMove[] Resize()
        {
            var array = new IMove[2 * _size];
            for (var i = 0; i < _size; i++)
                array[i] = _items[i];
            return array;
        }

        private void Build(IEnumerable<IMove> moves)
        {
            _items = moves.ToArray();
            _size = _items.Length;
            for (var i = _size / 2; i >= 0; i--)
            {
                Heapify(i);
            }
        }

        private void Heapify(int index)
        {
            var left = index*2+1;
            if (left >= _size) return;

            var last = left + 2;
            last = Math.Min(last, _size);
            var largest = index;
            for (var i = left; i < last; i++)
            {
                if (_items[i].History > _items[largest].History)
                {
                    largest = i;
                }
            }

            if (largest == index) return;

            var t = _items[index];
            _items[index] = _items[largest];
            _items[largest] = t;

            Heapify(largest);
        }

        private IMove Maximum()
        {
            _size--;
            IMove max = _items[0];
            _items[0] = _items[_size];

            Heapify(0);
            return max;
        }

        public IEnumerable<IMove> Sort()
        {
            while (_size>1)
            {
                yield return Maximum();
            }

            yield return _items[0];
        }
    }
}
