using Infrastructure.Interfaces.Position;
using Infrastructure.Sorters.Pv;

namespace Algorithms.Strategies.AlphaBeta
{
    public class PvValueStrategy : PvStrategyBase
    {
        public PvValueStrategy(short depth, IPosition position)
            : base(depth, position, new PvValueMoveSorter())
        {
        }
    }
}