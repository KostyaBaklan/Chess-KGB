using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Strategies.LateMove.Deep.Null
{
    public class LmrDeepNullExtendedStrategy : LmrDeepNullStrategyBase
    {
        public LmrDeepNullExtendedStrategy(short depth, IPosition position) : base(depth, position)
        {
            InitializeSorters(depth, position, MoveSorterProvider.GetExtended(position, new HistoryComparer()));
        }
    }
}