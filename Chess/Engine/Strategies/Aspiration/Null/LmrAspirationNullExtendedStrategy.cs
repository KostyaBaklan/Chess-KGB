using Engine.Interfaces;
using Engine.Strategies.LateMove.Base.Null;

namespace Engine.Strategies.Aspiration.Null
{
    public class LmrAspirationNullExtendedStrategy : AspirationStrategyBase
    {
        public LmrAspirationNullExtendedStrategy(short depth, IPosition position) : base(depth, position)
        {
            InternalStrategy = new LmrNullExtendedStrategy(depth, position);
        }
    }
}