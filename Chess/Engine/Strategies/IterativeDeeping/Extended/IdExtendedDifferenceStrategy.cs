using Engine.Interfaces;
using Engine.Strategies.AlphaBeta.Extended;

namespace Engine.Strategies.IterativeDeeping.Extended
{
    public class IdExtendedDifferenceStrategy : IdStrategyBase
    {
        public IdExtendedDifferenceStrategy(short depth, IPosition position)
            : base(depth, position, new AlphaBetaExtendedDifferenceStrategy(depth, position))
        {
        }
    }
}