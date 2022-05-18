using Engine.Interfaces;
using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;

namespace Engine.Strategies.PVS
{
    public class PvsExtendedStrategy:PvsStrategyBase
    {
        public PvsExtendedStrategy(short depth, IPosition position) : base(depth, position)
        {
            InitializeSorters(depth,position,new ExtendedSorter(position,new HistoryComparer()));
        }
    }
}