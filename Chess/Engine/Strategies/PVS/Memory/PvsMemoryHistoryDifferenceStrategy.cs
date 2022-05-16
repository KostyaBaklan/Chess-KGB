using Engine.Interfaces;
using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;

namespace Engine.Strategies.PVS.Memory
{
    public class PvsMemoryHistoryDifferenceStrategy : PvsMemoryStrategyBase
    {
        public PvsMemoryHistoryDifferenceStrategy(short depth, IPosition position) : base(depth, position)
        {
            InitializeSorters(depth, position, new ExtendedSorter(position, new HistoryDifferenceComparer()));
        }
    }
}