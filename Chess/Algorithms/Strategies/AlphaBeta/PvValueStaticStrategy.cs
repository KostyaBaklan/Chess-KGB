using Infrastructure.Interfaces.Position;
using Infrastructure.Sorters.Pv;

namespace Algorithms.Strategies.AlphaBeta
{
    public class PvValueStaticStrategy : PvStrategyBase
    {
        public PvValueStaticStrategy(short depth, IPosition position)
            : base(depth, position, new PvValueStaticMoveSorter())
        {
        }
    }
}