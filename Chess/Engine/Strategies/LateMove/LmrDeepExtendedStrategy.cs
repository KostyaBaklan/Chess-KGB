using Engine.Interfaces;
using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;

namespace Engine.Strategies.LateMove
{
    public class LmrDeepExtendedStrategy : LmrDeepStrategyBase
    {
        public LmrDeepExtendedStrategy(short depth, IPosition position) : base(depth, position)
        {
            MainSorter = new ExtendedSorter(position, new HistoryComparer());
            InitialSorter = new ExtendedSorter(position, new HistoryDifferenceExtendedComparer());
        }
    }
}