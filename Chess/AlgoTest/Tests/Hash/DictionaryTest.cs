using System.Collections.Generic;
using Algorithms.DataStructures;

namespace AlgoTest.Tests.Hash
{
    class DictionaryTest:HashTableTest
    {
        private readonly ZDictionary _dictionary;

        public DictionaryTest()
        {
            _dictionary = new ZDictionary();
        }

        #region Overrides of HashTableTest

        public override int Count => _dictionary.Count;

        protected override void ExecuteInternal(List<ulong> list)
        {
            foreach (var i in list)
            {
                var o = _dictionary.Get(i);
                _dictionary.Add(i, o ?? new object());
            }
        }

        #endregion
    }

    class ZDictionary : ZobristDictionary<object> {
    }
}