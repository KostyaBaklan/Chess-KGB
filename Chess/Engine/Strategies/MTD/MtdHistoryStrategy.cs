using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Interfaces;
using Engine.Strategies.AlphaBeta.Extended;

namespace Engine.Strategies.MTD
{
    public class MtdHistoryStrategy : MtdStrategyBase
    {
        public MtdHistoryStrategy(short depth, IPosition position) : base(depth, position)
        {
            InternalStrategy = new AlphaBetaExtendedHistoryStrategy(depth,position);
        }
    }
}
