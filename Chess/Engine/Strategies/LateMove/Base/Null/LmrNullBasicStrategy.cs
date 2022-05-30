using Engine.Interfaces;
using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters.Basic;

namespace Engine.Strategies.LateMove.Base.Null
{
    public class LmrNullBasicStrategy : LmrNullStrategyBase
    {
        public LmrNullBasicStrategy(short depth, IPosition position) : base(depth, position)
        {
            InitializeSorters(depth, position, new BasicSorter(position, new HistoryComparer()));
        }
    }
}