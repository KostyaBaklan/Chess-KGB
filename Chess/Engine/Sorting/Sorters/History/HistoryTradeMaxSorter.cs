using Engine.DataStructures.Moves.Collections.History;
using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Sorting.Sorters.History
{
    public class HistoryTradeMaxSorter : HistoryTradeSorterBase
    {
        public HistoryTradeMaxSorter(IPosition position, IMoveComparer comparer) : base(position, comparer)
        {
            HistoryMoveCollection = new HistoryTradeMaxMoveCollection(comparer);
        }
    }
}