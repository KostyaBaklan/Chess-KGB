using Engine.DataStructures.Moves.Collections.History;
using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Sorting.Sorters.History
{
    public class HistoryTradeSorter : HistoryTradeSorterBase
    {
        public HistoryTradeSorter(IPosition position, IMoveComparer comparer) : base(position, comparer)
        {
            HistoryMoveCollection = new HistoryTradeMoveCollection(comparer);
        }
    }
}