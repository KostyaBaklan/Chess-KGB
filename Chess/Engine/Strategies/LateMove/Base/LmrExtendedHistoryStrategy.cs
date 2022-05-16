using Engine.Interfaces;
using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;

namespace Engine.Strategies.LateMove.Base
{
    public class LmrExtendedHistoryStrategy : LmrStrategyBase
    {
        public LmrExtendedHistoryStrategy(short depth, IPosition position) : base(depth, position)
        {
            InitializeSorters(depth, position, new ExtendedSorter(position, new HistoryComparer()));
        }
    }
}
