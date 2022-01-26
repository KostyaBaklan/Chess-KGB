using Engine.Interfaces;
using Engine.Strategies.AlphaBeta.Extended;

namespace Engine.Strategies.IterativeDeeping.Extended
{
    public class IdExtendedHistoryDifferenceStrategy : IdStrategyBase
    {
        public IdExtendedHistoryDifferenceStrategy(short depth, IPosition position)
            : base(depth, position, new AlphaBetaExtendedHistoryDifferenceStrategy(depth, position))
        {
        }
    }
}