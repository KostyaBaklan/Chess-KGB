using Engine.Interfaces;
using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;

namespace Engine.Strategies.LateMove.Base
{
    public class LmrAdvancedHistoryStrategy : LmrStrategyBase
    {
        public LmrAdvancedHistoryStrategy(short depth, IPosition position) : base(depth, position)
        {
            Sorter = new AdvancedSorter(position, new HistoryComparer());
        }
    }
}