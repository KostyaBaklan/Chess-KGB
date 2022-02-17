using Engine.Interfaces;
using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;

namespace Engine.Strategies.LateMove.Base
{
    public class LmrExtendedHistoryDifferenceStrategy : LmrStrategyBase
    {
        public LmrExtendedHistoryDifferenceStrategy(short depth, IPosition position) : base(depth, position)
        {
            Sorter = new ExtendedSorter(position, new HistoryDifferenceComparer());
        }
    }
}