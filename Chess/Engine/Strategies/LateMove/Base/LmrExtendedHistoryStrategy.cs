using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;

namespace Engine.Strategies.LateMove.Base
{
    public class LmrExtendedHistoryStrategy : LmrStrategyBase
    {
        public LmrExtendedHistoryStrategy(short depth, IPosition position, TranspositionTable table = null) : base(depth, position,table)
        {
            InitializeSorters(depth, position, new ExtendedSorter(position, new HistoryComparer()));
        }
    }
}
