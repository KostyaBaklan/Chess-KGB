using Algorithms.Interfaces;
using Infrastructure.Interfaces.Position;

namespace Algorithms.Strategies.Iterative.Aspiration
{
    public abstract class IterativeAspirationStrategyBase : IterativeStrategyBase
    {
        protected IterativeAspirationStrategyBase(short depth, IPosition position) : base(depth,position)
        {
        }

        protected override IResult Search(int guess, int depth, IResult currentResult)
        {
            var window = 1000;
            var alpha = guess - window;
            var beta = guess + window;
            var result = Strategy.GetResult(alpha, beta, depth, currentResult?.Move, currentResult?.Cut);
            var resultValue = result.Value;

            if (resultValue >= beta)
                return GetResult(resultValue, 100000, Depth);
            if (resultValue <= alpha)
                return GetResult(-100000, resultValue, Depth);
            return result;
        }

        protected override int GetInitialDepth()
        {
            return 4;
        }
    }
}