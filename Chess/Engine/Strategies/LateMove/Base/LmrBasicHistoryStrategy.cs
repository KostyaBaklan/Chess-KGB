using Engine.Interfaces;
using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;
using Engine.Sorting.Sorters.Basic;

namespace Engine.Strategies.LateMove.Base
{
    public class LmrBasicHistoryStrategy : LmrStrategyBase
    {
        public LmrBasicHistoryStrategy(short depth, IPosition position) : base(depth, position)
        {
            InitializeSorters(depth, position, new BasicSorter(position, new HistoryComparer()));
        }

        #region Overrides of LmrStrategyBase

        #endregion
    }
}