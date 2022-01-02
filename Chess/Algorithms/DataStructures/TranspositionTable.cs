using System.Runtime.CompilerServices;
using Algorithms.Models;

namespace Algorithms.DataStructures
{
    public class TranspositionTable : ZobristDictionary<TranspositionEntry>
    {
        public TranspositionTable()
        { }
        public TranspositionTable(int capacity) : base(capacity) { }

        #region Overrides of ZobristDictionary<TranspositionEntry>

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void Replace(ulong key, TranspositionEntry oldItem, TranspositionEntry newItem)
        {
            if (oldItem.Depth < newItem.Depth)
            {
                Table[key] = newItem;
            }
        }

        #endregion
    }
}
