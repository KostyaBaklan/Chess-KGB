using Engine.Interfaces;
using Engine.Strategies.LateMove.Deep.Null;

namespace Engine.Strategies.Aspiration.Null
{
    public class LmrAspirationDeepNullExtendedStrategy : AspirationStrategyBase
    {
        public LmrAspirationDeepNullExtendedStrategy(short depth, IPosition position) : base(depth, position)
        {
            InternalStrategy = new LmrDeepNullExtendedStrategy(depth, position);
        }
    }
}
