using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Engine.DataStructures
{
    public class ZobristDictionary<T>
    {
        protected readonly Dictionary<ulong, T> Table;

        public ZobristDictionary() : this(1247291)
        {
        }

        public ZobristDictionary(int capacity)
        {
            Table = new Dictionary<ulong, T>(capacity);
        }

        public int Count => Table.Count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(ulong key, T item)
        {
            if (Table.TryGetValue(key, out var t))
            {
                Replace(key, t, item);
            }
            else
            {
                Table[key] = item;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get(ulong key)
        {
            return Table.TryGetValue(key, out var item) ? item : default(T);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGet(ulong key, out T item)
        {
            return Table.TryGetValue(key, out item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void Replace(ulong key, T oldItem, T newItem)
        {
            Table[key] = newItem;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            Table?.Clear();
        }
    }
}
