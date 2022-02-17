using Engine.Interfaces;
using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;

namespace Engine.Strategies.LateMove.Base
{
    public class LmrBasicHistoryStrategy : LmrStrategyBase
    {
        public LmrBasicHistoryStrategy(short depth, IPosition position) : base(depth, position)
        {
            MainSorter = new BasicSorter(position, new HistoryComparer());
            InitialSorter = new BasicSorter(position, new HistoryDifferenceExtendedComparer());
        }
    }
}