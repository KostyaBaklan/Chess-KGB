using Engine.Interfaces;
using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;

namespace Engine.Strategies.NullMove
{
    public class NmrAdvancedHistoryStrategy : NmrStrategyBase
    {
        public NmrAdvancedHistoryStrategy(short depth, IPosition position) : base(depth, position)
        {
            InitializeSorters(depth, position, new AdvancedSorter(position, new HistoryComparer()));
        }
    }
}