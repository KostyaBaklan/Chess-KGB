using Infrastructure.Interfaces.Moves;
using Infrastructure.Interfaces.Position;

namespace Algorithms.Strategies.Iterative.AlphaBeta
{
    public abstract class IterativePvStrategyBase : IterativeStrategyBase
    {
        protected IterativePvStrategyBase(short depth, IMoveSorter sorter, IPosition position) : base(depth, sorter, position)
        {
        }

        protected override int GetInitialDepth()
        {
            return 4;
        }
    }
}