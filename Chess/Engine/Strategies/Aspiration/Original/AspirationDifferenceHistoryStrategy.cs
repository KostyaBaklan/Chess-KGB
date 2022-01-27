using Engine.Interfaces;
using Engine.Strategies.AlphaBeta.Extended;

namespace Engine.Strategies.Aspiration.Original
{
    public class AspirationDifferenceHistoryStrategy : AspirationStrategyBase
    {
        public AspirationDifferenceHistoryStrategy(short depth, IPosition position) : base(depth, position)
        {
            InternalStrategy = new AlphaBetaExtendedDifferenceHistoryStrategy(depth, position);
        }
    }
}