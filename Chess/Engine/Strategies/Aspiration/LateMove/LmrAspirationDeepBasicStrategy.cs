using Engine.Interfaces;
using Engine.Strategies.LateMove.Deep;

namespace Engine.Strategies.Aspiration.LateMove
{
    public class LmrAspirationDeepBasicStrategy : AspirationStrategyBase
    {
        public LmrAspirationDeepBasicStrategy(short depth, IPosition position) : base(depth, position)
        {
            InternalStrategy = new LmrDeepBasicStrategy(depth, position);
        }
    }
}