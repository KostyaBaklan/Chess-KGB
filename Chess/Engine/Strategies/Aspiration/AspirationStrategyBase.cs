using System;
using CommonServiceLocator;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Enums;
using Engine.Strategies.Base;

namespace Engine.Strategies.Aspiration
{
    public abstract class AspirationStrategyBase : ComplexStrategyBase
    {
        protected int AspirationWindow;
        protected int AspirationDepth;
        protected int AspirationMinDepth;
        protected int AspirationIterations;

        protected AspirationStrategyBase(short depth, IPosition position) : base(depth, position)
        {
            var configurationProvider = ServiceLocator.Current.GetInstance<IConfigurationProvider>();
            var configuration = configurationProvider.AlgorithmConfiguration.AspirationConfiguration;
            AspirationWindow = configuration
                .AspirationWindow;
            AspirationDepth = configuration
                .AspirationDepth;
            AspirationMinDepth = configuration
                .AspirationMinDepth;
            AspirationIterations = configuration
                .AspirationIterations;
        }


        #region Overrides of StrategyBase

        public override IResult GetResult()
        {
            if (Position.GetPhase() == Phase.End)
            {
                return EndGameStrategy.GetResult(-SearchValue, SearchValue, Math.Min(Depth + 1, MaxEndGameDepth));
            }

            var depth = Depth;
            var t = depth - AspirationDepth * AspirationIterations;

            var result = InternalStrategy.GetResult(-SearchValue, SearchValue, t);
            for (int d = t + AspirationDepth; d <= depth; d += AspirationDepth)
            {
                var alpha = result.Value - AspirationWindow;
                var beta = result.Value + AspirationWindow;
                result = InternalStrategy.GetResult(alpha, beta, d, result.Move);
                if (result.Value >= beta)
                {
                    result = InternalStrategy.GetResult(result.Value, SearchValue, d, result.Move);
                }
                else if (result.Value <= alpha)
                {
                    result = InternalStrategy.GetResult(-SearchValue, result.Value, d, result.Move);
                }
            }

            return result;
        }

        #endregion
    }
}

