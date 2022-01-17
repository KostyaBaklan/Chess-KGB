using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Strategies.AlphaBeta.Null
{
    public class AlphaBetaNullDifferenceHistoryStrategy : AlphaBetaNullStrategy
    {
        public AlphaBetaNullDifferenceHistoryStrategy(short depth, IPosition position) : base(depth, position, new DifferenceHistoryComparer())
        {
        }
    }
}