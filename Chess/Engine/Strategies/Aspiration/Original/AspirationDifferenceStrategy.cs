using Engine.Interfaces;
using Engine.Strategies.AlphaBeta.Extended;

namespace Engine.Strategies.Aspiration.Original
{
    public class AspirationDifferenceStrategy : AspirationStrategyBase
    {
        public AspirationDifferenceStrategy(short depth, IPosition position) : base(depth, position)
        {
            InternalStrategy = new AlphaBetaExtendedDifferenceStrategy(depth, position);
        }
    }
}