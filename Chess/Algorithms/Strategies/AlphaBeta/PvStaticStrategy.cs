using Infrastructure.Interfaces.Position;
using Infrastructure.Sorters.Pv;

namespace Algorithms.Strategies.AlphaBeta
{
    public class PvStaticStrategy : PvStrategyBase
    {
        public PvStaticStrategy(short depth, IPosition position)
            : base(depth, position, new PvStaticMoveSorter())
        {
        }
    }
}