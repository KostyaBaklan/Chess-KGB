using Engine.Interfaces;
using Engine.Strategies.AlphaBeta.Extended;

namespace Engine.Strategies.IterativeDeeping.Extended
{
    public class IdExtendedHistoryStrategy : IdStrategyBase
    {
        public IdExtendedHistoryStrategy(short depth, IPosition position) 
            : base(depth, position, new AlphaBetaExtendedHistoryStrategy(depth,position))
        {
        }
    }
}