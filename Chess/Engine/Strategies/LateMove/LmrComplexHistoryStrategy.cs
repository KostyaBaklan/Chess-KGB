using Engine.Interfaces;
using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;

namespace Engine.Strategies.LateMove
{
    public class LmrComplexHistoryStrategy : LmrStrategyBase
    {
        public LmrComplexHistoryStrategy(short depth, IPosition position) : base(depth, position)
        {
            Sorter = new ComplexSorter(position, new HistoryComparer());
        }
    }
}