using Engine.Interfaces;
using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;

namespace Engine.Strategies.LateMove.Deep.Null
{
    public class LmrDeepNullBasicStrategy : LmrDeepNullStrategyBase
    {
        public LmrDeepNullBasicStrategy(short depth, IPosition position) : base(depth, position)
        {
            Sorter = new BasicSorter(position, new HistoryComparer());
        }
    }
}