using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;

namespace Engine.Strategies.MultiCut
{
    public class MultiCutComplexHistoryDifferenceStrategy : MultiCutStrategyBase
    {
        public MultiCutComplexHistoryDifferenceStrategy(short depth, IPosition position, TranspositionTable table = null)
            : base(depth, position, table)
        {
            Sorter = new ComplexSorter(position, new HistoryDifferenceComparer());
        }
    }
}