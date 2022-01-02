using Algorithms.Strategies.AlphaBeta;
using Infrastructure.Interfaces.Position;
using Infrastructure.Sorters.Pv;

namespace Algorithms.Strategies.Iterative.AlphaBeta
{
    public class IterativeFigureStaticStrategy : IterativePvStrategyBase
    {
        public IterativeFigureStaticStrategy(short depth, IPosition position) : base(depth, new PvFigureStaticMoveSorter(), position)
        {
            Strategy = new PvFigureStaticStrategy(depth, position);
        }

        #region Overrides of IterativeStrategyBase

        #endregion
    }
}