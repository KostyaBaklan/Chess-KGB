using Algorithms.Strategies.AlphaBeta;
using Infrastructure.Interfaces.Position;
using Infrastructure.Sorters.Pv;

namespace Algorithms.Strategies.Iterative.AlphaBeta
{
    public class IterativeStaticStrategy : IterativePvStrategyBase
    {
        public IterativeStaticStrategy(short depth, IPosition position) : base(depth, new PvStaticMoveSorter(), position)
        {
            Strategy = new PvStaticStrategy(depth, position);
        }

        #region Overrides of IterativeStrategyBase

        #endregion
    }
}