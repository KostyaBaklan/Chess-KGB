using Engine.Interfaces;
using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;

namespace Engine.Strategies.AlphaBeta.History
{
    public class AlphaBetaSimpleHistoryStrategy : AlphaBetaHistoryStrategy
    {
        public AlphaBetaSimpleHistoryStrategy(short depth, IPosition position) : base(depth, position)
        {
            Sorter = new SimpleMoveSorter(new HistoryComparer(), position);
        }
    }
}