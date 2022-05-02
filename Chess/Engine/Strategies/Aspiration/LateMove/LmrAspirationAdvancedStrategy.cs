using Engine.Interfaces;
using Engine.Strategies.LateMove.Base;

namespace Engine.Strategies.Aspiration.LateMove
{
    public class LmrAspirationAdvancedStrategy : AspirationStrategyBase
    {
        public LmrAspirationAdvancedStrategy(short depth, IPosition position) : base(depth, position)
        {
            InternalStrategy = new LmrAdvancedHistoryStrategy(depth, position);
        }
    }
}