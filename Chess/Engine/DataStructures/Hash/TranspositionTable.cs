using System.Runtime.CompilerServices;
using Engine.Models.Transposition;

namespace Engine.DataStructures.Hash
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