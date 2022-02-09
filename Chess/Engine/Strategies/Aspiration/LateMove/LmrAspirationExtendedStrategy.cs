using Engine.Interfaces;
using Engine.Strategies.LateMove;

namespace Engine.Strategies.Aspiration.LateMove
{
    public class LmrAspirationExtendedStrategy : AspirationStrategyBase
    {
        public LmrAspirationExtendedStrategy(short depth, IPosition position) : base(depth, position)
        {
            InternalStrategy = new LmrExtendedHistoryStrategy(depth, position);
        }
    }
}