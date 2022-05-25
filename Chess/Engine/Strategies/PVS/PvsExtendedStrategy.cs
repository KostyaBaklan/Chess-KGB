using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Strategies.PVS
{
    public class PvsExtendedStrategy:PvsStrategyBase
    {
        public PvsExtendedStrategy(short depth, IPosition position) : base(depth, position)
        {
            InitializeSorters(depth,position, MoveSorterProvider.GetExtended(position,new HistoryComparer()));
        }
    }
}