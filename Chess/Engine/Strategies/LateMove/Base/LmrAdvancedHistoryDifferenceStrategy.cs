using Engine.Interfaces;
using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;

namespace Engine.Strategies.LateMove.Base
{
    public class LmrAdvancedHistoryDifferenceStrategy : LmrStrategyBase
    {
        public LmrAdvancedHistoryDifferenceStrategy(short depth, IPosition position) : base(depth, position)
        {
            Sorter = new AdvancedSorter(position, new HistoryDifferenceComparer());
        }
    }
}