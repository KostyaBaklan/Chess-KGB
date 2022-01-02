using Algorithms.Strategies.AlphaBeta;
using Infrastructure.Interfaces.Position;
using Infrastructure.Sorters.Pv;

namespace Algorithms.Strategies.Iterative.AlphaBeta
{
    public class IterativeFigureValueStrategy : IterativePvStrategyBase
    {
        public IterativeFigureValueStrategy(short depth, IPosition position) : base(depth, new PvFigureValueMoveSorter(), position)
        {
            Strategy = new PvFigureValueStrategy(depth, position);
        }

        #region Overrides of IterativeStrategyBase

        #endregion
    }
}