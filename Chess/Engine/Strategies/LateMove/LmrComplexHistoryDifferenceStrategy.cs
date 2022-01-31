using Engine.Interfaces;
using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;

namespace Engine.Strategies.LateMove
{
    public class LmrComplexHistoryDifferenceStrategy : LmrStrategyBase
    {
        public LmrComplexHistoryDifferenceStrategy(short depth, IPosition position) : base(depth, position)
        {
            Sorter = new ComplexSorter(position, new HistoryDifferenceComparer());
        }
    }
}