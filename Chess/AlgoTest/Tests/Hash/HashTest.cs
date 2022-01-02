using System.Collections.Generic;

namespace AlgoTest.Tests.Hash
{
    class HashTest:HashTableTest
    {
        private readonly ObjectTable _dictionary;

        public HashTest()
        {
            _dictionary = new ObjectTable();
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
}