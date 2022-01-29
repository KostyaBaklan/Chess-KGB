using Engine.Interfaces;
using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;

namespace Engine.Strategies.PVS.Memory
{
    public class PvsMemoryHistoryStrategy : PvsMemoryStrategyBase
    {
        public PvsMemoryHistoryStrategy(short depth, IPosition position) : base(depth, position)
        {
            Sorter = new ExtendedSorter(position, new HistoryComparer());
        }
    }
}