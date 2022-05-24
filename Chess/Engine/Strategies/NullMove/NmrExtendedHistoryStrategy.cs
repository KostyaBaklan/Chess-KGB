using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Strategies.NullMove
{
    public class NmrExtendedHistoryStrategy:NmrStrategyBase
    {
        public NmrExtendedHistoryStrategy(short depth, IPosition position) : base(depth, position)
        {
            InitializeSorters(depth, position, MoveSorterProvider.GetExtended(position, new HistoryComparer()));
        }
    }
}