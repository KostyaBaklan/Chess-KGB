using CommonServiceLocator;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Enums;
using Engine.Strategies.Base;

namespace Engine.Strategies.MTD
{
    public abstract class MtdStrategyBase:ComplexStrategyBase
    {
        protected int DepthOffset;
        protected int NullWindow;

        protected MtdStrategyBase(short depth, IPosition position) : base(depth, position)
        {
            var configurationProvider = ServiceLocator.Current.GetInstance<IConfigurationProvider>();
            DepthOffset = configurationProvider
                .AlgorithmConfiguration.DepthOffset;
            NullWindow = configurationProvider
                .AlgorithmConfiguration.NullWindow;
        }

        public override IResult GetResult()
        {
            var depth = Depth;
            if (Position.GetPhase() == Phase.End)
            {
                depth++;
            }

            IResult result = new Result();
            var depthOffset = depth - DepthOffset;
            for (int d = depthOffset; d <= depth; d+=2)
            {
                result = InternalStrategy.GetResult(-SearchValue, SearchValue, d-2);
                result = GetResult(d, result);
                if (result.GameResult != GameResult.Continue) break;
            }

            return result;
        }

        private IResult GetResult(int depth, IResult last)
        {
            int g = last.Value;
            int lowerBound = short.MinValue;
            int upperBound = short.MaxValue;
            IResult bestResult = last;
            while (lowerBound< upperBound)
            {
                int beta;
                if (g == lowerBound)
                {
                    beta = g + NullWindow;
                }
                else
                {
                    beta = g;
                }

                var result = InternalStrategy.GetResult(beta - NullWindow, beta, depth, last.Move);
                g = result.Value;

                if (g < beta)
                {
                    upperBound = g;
                }
                else
                {
                    lowerBound = g;
                    bestResult = result;
                }
            }

            return bestResult;
        }
    }
}
