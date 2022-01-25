using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Strategies.AlphaBeta.Null
{
    public class AlphaBetaNullHistoryDifferenceStrategy : AlphaBetaNullStrategy
    {
        public AlphaBetaNullHistoryDifferenceStrategy(short depth, IPosition position) : base(depth, position, new HistoryDifferenceComparer())
        {
        }
    }
}