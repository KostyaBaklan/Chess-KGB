using System.Collections.Generic;

namespace Algorithms.DataStructures
{
    public abstract  class ZobristHashTable<T>
    {
        private readonly ulong _module = 1247291;
        private readonly Dictionary<ulong, T>[] _table;

        protected ZobristHashTable()
        {
            _table = new Dictionary<ulong, T>[_module];
            Count = 0;
        }

        public int Count { get; private set; }

        public void Add(ulong key, T item)
        {
            ulong module = key % _module;
            if (_table[module] == null)
            {
                _table[module] = new Dictionary<ulong, T>();
            }

            if (_table[module].TryGetValue(key, out var t))
            {
                Replace(_table[module], key, t, item);
            }
            else
            {
                _table[module][key] = item;
                Count++;
            }
        }

        public T Get(ulong key)
        {
            var module = key % _module;
            var dictionary = _table[module];
            if (dictionary == null) return default(T);

            return dictionary.TryGetValue(key, out var item) ? item : default(T);
        }

        protected virtual void Replace(Dictionary<ulong, T> dictionary, ulong key, T oldItem, T newItem)
        {
            dictionary[key] = newItem;
        }
    }
}
