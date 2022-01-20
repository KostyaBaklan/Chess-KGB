using Engine.Interfaces;
using Engine.Strategies.AlphaBeta.Extended;

namespace Engine.Strategies.IterativeDeeping.Extended
{
    public class IdExtendedDifferenceHistoryStrategy : IdStrategyBase
    {
        public IdExtendedDifferenceHistoryStrategy(short depth, IPosition position)
            : base(depth, position, new AlphaBetaExtendedDifferenceHistoryStrategy(depth, position))
        {
        }
    }
}