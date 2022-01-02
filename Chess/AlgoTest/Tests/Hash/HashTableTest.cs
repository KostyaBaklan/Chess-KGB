using System;
using System.Collections.Generic;

namespace AlgoTest.Tests.Hash
{
    abstract class HashTableTest : IHashTableTest
    {
        protected TimeSpan Total, Current;

        #region Implementation of IHashTableTest

        public void Execute(List<ulong> list)
        {
            var now = DateTime.Now;
            ExecuteInternal(list);
            Current = DateTime.Now - now;
            Total += Current;
            Console.WriteLine($"{GetType().Name} => Current = {Current}, Total = {Total}, Count = {Count}");
        }

        public abstract int Count { get; }

        protected abstract void ExecuteInternal(List<ulong> list);

        #endregion
    }
}