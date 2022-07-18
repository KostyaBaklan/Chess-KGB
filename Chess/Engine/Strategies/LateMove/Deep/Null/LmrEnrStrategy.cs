using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Strategies.LateMove.Deep.Null
{
    public sealed class LmrEnrStrategy : LmrEnrStrategyBase
    {
        public LmrEnrStrategy(short depth, IPosition position, TranspositionTable table = null) : base(depth, position, table)
        {
            InitializeSorters(depth, position, MoveSorterProvider.GetExtended(position, new HistoryComparer()));
        }
    }
}