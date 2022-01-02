using Algorithms.Interfaces;
using Algorithms.Strategies.Base;
using Infrastructure.Interfaces.Moves;
using Infrastructure.Interfaces.Position;

namespace Algorithms.Strategies.Aspiration
{
    public class AspirationStrategyBase : AlphaBetaStrategyBase
    {
        public AspirationStrategyBase(short depth, IMoveSorter sorter, IPosition position) : base(depth, sorter, position)
        {
        }

        #region Overrides of AlphaBetaStrategyBase

        public override IResult GetResult()
        {
            var value = Position.GetValue();
            var window = 1000;
            var alpha = value - window;
            var beta = value + window;

            var result = GetResult(alpha, beta, Depth);
            var resultValue = result.Value;

            if (resultValue >= beta)
                return GetResult(resultValue, 100000, Depth);
            if(resultValue <=alpha)
                return GetResult(-100000, resultValue, Depth);
            return result;
        }

        #endregion
    }
}