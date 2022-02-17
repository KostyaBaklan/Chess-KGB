using Engine.Interfaces;
using Engine.Strategies.LateMove;
using Engine.Strategies.LateMove.Deep;

namespace Engine.Strategies.Aspiration.LateMove
{
    public class LmrAspirationDeepExtendedStrategy: AspirationStrategyBase
    {
        public LmrAspirationDeepExtendedStrategy(short depth, IPosition position) : base(depth, position)
        {
            InternalStrategy = new LmrDeepExtendedStrategy(depth, position);
        }
    }
}
