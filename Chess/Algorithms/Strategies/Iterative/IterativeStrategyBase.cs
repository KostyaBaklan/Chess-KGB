using Algorithms.Interfaces;
using Algorithms.Strategies.Base;
using Infrastructure.Interfaces.Moves;
using Infrastructure.Interfaces.Position;

namespace Algorithms.Strategies.Iterative
{
    public abstract class IterativeStrategyBase : StrategyBase
    {
        protected AlphaBetaStrategyBase Strategy;

        protected IterativeStrategyBase(short depth, IMoveSorter sorter, IPosition position) : base(depth, sorter, position)
        {
        }

        protected IterativeStrategyBase(short depth,  IPosition position) : base(depth, position)
        {
        }

        #region Overrides of StrategyBase

        public override IResult GetResult(int alpha, int beta, int depth, IMove pv = null, IMove cut = null)
        {
            return Strategy.GetResult(alpha, beta, depth,pv,cut);
        }

        public override int Size => Strategy.Size;

        public override IResult GetResult()
        {
            int guess = GetInitialGuess();
            IResult currentResult = null;
            var d = GetInitialDepth();

            for (int depth = d; depth <= Depth; depth++)
            {
                currentResult = Search(guess, depth, currentResult);
                guess = GetGuess(currentResult);
            }

            return currentResult;
        }

        protected virtual int GetInitialGuess()
        {
            return 0;
        }

        protected virtual int GetGuess(IResult currentResult)
        {
            return currentResult.Value;
        }

        protected virtual IResult Search(int guess, int depth, IResult currentResult)
        {
            return Strategy.GetResult(-100000, 100000, depth, currentResult?.Move, currentResult?.Cut);
        }

        protected abstract int GetInitialDepth();

        public override int Search(int alpha, int beta, int depth)
        {
            return Strategy.Search(alpha, beta, depth);
        }

        #endregion
    }
}
