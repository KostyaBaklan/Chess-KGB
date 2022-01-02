using Algorithms.Strategies.AlphaBeta;
using Infrastructure.Interfaces.Position;
using Infrastructure.Sorters.Pv;

namespace Algorithms.Strategies.Iterative.AlphaBeta
{
    public class IterativeValueStrategy : IterativePvStrategyBase
    {
        public IterativeValueStrategy(short depth, IPosition position) : base(depth, new PvValueMoveSorter(), position)
        {
            Strategy = new PvValueStrategy(depth, position);
        }
    }
}