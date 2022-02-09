using CommonServiceLocator;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Enums;
using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;
using Engine.Strategies.Base;

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