using Algorithms.Interfaces;
using Algorithms.Strategies.AlphaBeta;
using Infrastructure.Interfaces.Moves;
using Infrastructure.Interfaces.Position;
using Infrastructure.Sorters.Pv;

namespace Algorithms.Strategies.Iterative.Mtd
{
    public class MtDualStrategyBase : MtStrategyBase
    {
        public MtDualStrategyBase(short depth, IMoveSorter sorter, IPosition position) : base(depth, sorter, position)
        {
        }

        #region Overrides of IterativeStrategyBase

        protected override IResult Search(int guess, int depth, IResult currentResult)
        {
            var step = 125;
            int g = guess;
            int upper = int.MaxValue;
            int lower = int.MinValue;
            int iteration = 0;

            while (lower < upper)
            {
                int beta;
                if (g == upper)
                {
                    beta = g - step;
                }
                else
                {
                    beta = g;
                }

                currentResult = Strategy.GetResult(beta, beta + step, depth, currentResult?.Move, currentResult?.Cut);
                g = currentResult.Value;
                iteration++;

                if (g > beta)
                {
                    lower = g;
                }
                else
                {
                    upper = g;
                }

                if (iteration > 2)
                {

                }
            }

            return currentResult;
        }

        #endregion
    }

    public class MtDualValueStrategy: MtDualStrategyBase
    {
        public MtDualValueStrategy(short depth, IPosition position) : base(depth, new PvValueMoveSorter(), position)
        {
            Strategy = new PvValueStrategy(depth, position);
        }
    }
}