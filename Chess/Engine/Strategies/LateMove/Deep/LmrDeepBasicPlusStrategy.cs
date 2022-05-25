using Engine.Interfaces;
using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;
using Engine.Sorting.Sorters.Basic;

namespace Engine.Strategies.LateMove.Deep
{
    public class LmrDeepBasicPlusStrategy : LmrDeepStrategyBase
    {
        public LmrDeepBasicPlusStrategy(short depth, IPosition position) : base(depth, position)
        {
            InitializeSorters(depth, position, new BasicSorterPlus(position, new HistoryComparer()));
        }
    }
}