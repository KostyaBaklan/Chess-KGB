using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Strategies.AlphaBeta.Null.Extended
{
    public class AlphaBetaNullDifferenceHistoryStrategy : AlphaBetaExtendedNullStrategy
    {
        public AlphaBetaNullDifferenceHistoryStrategy(short depth, IPosition position) : base(depth, position, new DifferenceHistoryComparer())
        {
        }
    }
}