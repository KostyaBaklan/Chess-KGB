using Engine.Interfaces;
using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;

namespace Engine.Strategies.PVS.Memory
{
    public class PvsMemoryDifferenceStrategy : PvsMemoryStrategyBase
    {
        public PvsMemoryDifferenceStrategy(short depth, IPosition position) : base(depth, position)
        {
            Sorter = new ExtendedSorter(position, new DifferenceComparer());
        }
    }
}