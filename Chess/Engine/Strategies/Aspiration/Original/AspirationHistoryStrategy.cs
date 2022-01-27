using Engine.Interfaces;
using Engine.Strategies.AlphaBeta.Extended;

namespace Engine.Strategies.Aspiration.Original
{
    public class AspirationHistoryStrategy:AspirationStrategyBase
    {
        public AspirationHistoryStrategy(short depth, IPosition position) : base(depth, position)
        {
            InternalStrategy = new AlphaBetaExtendedHistoryStrategy(depth,position);
        }
    }
}
