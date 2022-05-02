using Engine.Interfaces;
using Engine.Strategies.LateMove.Deep;

namespace Engine.Strategies.Aspiration.LateMove
{
    public class LmrAspirationDeepAdvancedStrategy : AspirationStrategyBase
    {
        public LmrAspirationDeepAdvancedStrategy(short depth, IPosition position) : base(depth, position)
        {
            InternalStrategy = new LmrDeepAdvancedStrategy(depth, position);
        }
    }
}