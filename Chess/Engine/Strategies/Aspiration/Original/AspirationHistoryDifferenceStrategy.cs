using Engine.Interfaces;
using Engine.Strategies.AlphaBeta.Extended;

namespace Engine.Strategies.Aspiration.Original
{
    public class AspirationHistoryDifferenceStrategy : AspirationStrategyBase
    {
        public AspirationHistoryDifferenceStrategy(short depth, IPosition position) : base(depth, position)
        {
            InternalStrategy = new AlphaBetaExtendedHistoryDifferenceStrategy(depth, position);
        }
    }
}