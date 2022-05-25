using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Strategies.LateMove.Base.Null
{
    public class LmrNullExtendedStrategy : LmrNullStrategyBase
    {
        public LmrNullExtendedStrategy(short depth, IPosition position) : base(depth, position)
        {
            InitializeSorters(depth, position, MoveSorterProvider.GetExtended(position, new HistoryComparer()));
        }
    }
}