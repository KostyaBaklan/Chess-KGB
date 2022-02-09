using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Interfaces;
using Engine.Strategies.LateMove;

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
