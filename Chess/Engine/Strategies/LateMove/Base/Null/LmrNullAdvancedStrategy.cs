using Engine.Interfaces;
using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;

namespace Engine.Strategies.LateMove.Base.Null
{
    public class LmrNullAdvancedStrategy : LmrNullStrategyBase
    {
        public LmrNullAdvancedStrategy(short depth, IPosition position) : base(depth, position)
        {
            MainSorter = new AdvancedSorter(position, new HistoryComparer());
            InitialSorter = new AdvancedSorter(position, new HistoryDifferenceExtendedComparer());
        }
    }
}