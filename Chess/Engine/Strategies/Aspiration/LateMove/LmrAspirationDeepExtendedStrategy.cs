using Engine.Interfaces;
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
    public class LmrAspirationDeepBasicStrategy : AspirationStrategyBase
    {
        public LmrAspirationDeepBasicStrategy(short depth, IPosition position) : base(depth, position)
        {
            InternalStrategy = new LmrDeepBasicStrategy(depth, position);
        }
    }
}
