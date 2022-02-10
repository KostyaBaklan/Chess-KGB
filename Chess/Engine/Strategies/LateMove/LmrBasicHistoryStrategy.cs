using Engine.Interfaces;
using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;

namespace Engine.Strategies.LateMove
{
    public class LmrBasicHistoryStrategy : LmrStrategyBase
    {
        public LmrBasicHistoryStrategy(short depth, IPosition position) : base(depth, position)
        {
            Sorter = new BasicSorter(position, new HistoryComparer());
        }
    }
}