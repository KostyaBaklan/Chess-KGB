using Algorithms.Interfaces;
using Infrastructure.Interfaces.Moves;
using Infrastructure.Interfaces.Position;

namespace Algorithms.Strategies.Iterative.Mtd
{
    public class MtdStrategyBase: MtStrategyBase
    {
        protected MtdStrategyBase(short depth, IMoveSorter sorter, IPosition position) : base(depth, sorter, position)
        {
        }

        protected override IResult Search(int guess, int depth, IResult currentResult)
        {
            var step = 125;
            int g = guess;

            int upper = int.MaxValue;
            int lower = int.MinValue;

            int iteration = 0;

            while (lower < upper)
            {
                if (iteration > 2)
                {

                }
                int beta;
                if (g == lower)
                {
                    beta = g + step;
                }
                else
                {
                    beta = g;
                }

                currentResult = Strategy.GetResult(beta - step, beta, depth,currentResult?.Move, currentResult?.Cut);
                g = currentResult.Value;
                iteration++;

                if (g < beta)
                {
                    upper = g;
                }
                else
                {
                    lower = g;
                }
            }

            return currentResult;
        }
    }
}
