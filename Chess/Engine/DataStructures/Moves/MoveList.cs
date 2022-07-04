using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;

namespace Engine.DataStructures.Moves
{
    public class MoveList:IEnumerable<MoveBase>
    {
        private readonly MoveBase[] _items;

        public MoveList():this(128)
        {
        }

        #region Implementation of IReadOnlyCollection<out IMove>

        public int Count;

        public MoveList(int capacity)
        {
            _items = new MoveBase[capacity];
        }

        public MoveBase this[int i] => _items[i];

        #endregion

        #region Implementation of IEnumerable

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerator<MoveBase> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return _items[i];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(MoveBase move)
        {
            _items[Count++] = move;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            Count = 0;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(MoveBase[] items, int index)
        {
            Array.Copy(_items, 0, items, index, Count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Sort()
        {
            var count = Count;
            if (count < 3) return;

            var comparer = Sorting.Sort.HistoryComparer;

            var capturesCount = Sorting.Sort.SortMinimum[count];

            for (var i = 0; i < capturesCount; i++)
            {
                int index = i;
                var min = _items[i];
                for (int j = i + 1; j < count; j++)
                {
                    if (comparer.Compare(min, _items[j]) < 0) continue;

                    min = _items[j];
                    index = j;
                }

                if (index == i) continue;

                var temp = _items[index];
                _items[index] = _items[i];
                _items[i] = temp;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FullSort()
        {
            Array.Sort(_items,0,Count, Sorting.Sort.HistoryComparer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(MoveList moves)
        {
            Array.Copy(moves._items,0,_items,Count,moves.Count);
            Count += moves.Count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
        {
            return $"Count={Count}";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FullSort(IMoveComparer differenceComparer)
        {
            Array.Sort(_items, 0, Count, differenceComparer);
        }
    }
}
