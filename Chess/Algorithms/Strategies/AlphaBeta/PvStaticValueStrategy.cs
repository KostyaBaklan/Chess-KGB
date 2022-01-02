using Infrastructure.Interfaces.Position;
using Infrastructure.Sorters.Pv;

namespace Algorithms.Strategies.AlphaBeta
{
    public class PvStaticValueStrategy : PvStrategyBase
    {
        public PvStaticValueStrategy(short depth, IPosition position)
            : base(depth, position, new PvStaticValueMoveSorter())
        {
        }
    }
}