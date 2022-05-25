using Engine.Interfaces;
using Engine.Strategies.LateMove.Deep;

namespace Engine.Strategies.Aspiration.LateMove
{
    public class LmrAspirationDeepBasicPlusStrategy : AspirationStrategyBase
    {
        public LmrAspirationDeepBasicPlusStrategy(short depth, IPosition position) : base(depth, position)
        {
            InternalStrategy = new LmrDeepBasicPlusStrategy(depth, position);
        }
    }
}