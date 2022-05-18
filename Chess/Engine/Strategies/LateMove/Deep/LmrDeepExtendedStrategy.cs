using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;

namespace Engine.Strategies.LateMove.Deep
{
    public class LmrDeepExtendedStrategy : LmrDeepStrategyBase
    {
        public LmrDeepExtendedStrategy(short depth, IPosition position, TranspositionTable table = null) : base(depth, position,table)
        {
            InitializeSorters(depth, position, new ExtendedSorter(position, new HistoryComparer()));
        }
    }
}