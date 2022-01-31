using Engine.Interfaces;
using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;

namespace Engine.Strategies.NullMove
{
    public class NmrComplexHistoryDifferenceStrategy : NmrStrategyBase
    {
        public NmrComplexHistoryDifferenceStrategy(short depth, IPosition position) : base(depth, position)
        {
            Sorter = new ComplexSorter(position, new HistoryDifferenceComparer());
        }
    }
}