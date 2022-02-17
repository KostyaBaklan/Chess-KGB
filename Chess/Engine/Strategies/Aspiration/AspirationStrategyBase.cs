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

        protected AspirationStrategyBase(short depth, IPosition position) : base(depth, position)
        {
            var configurationProvider = ServiceLocator.Current.GetInstance<IConfigurationProvider>();
            var configuration = configurationProvider.AlgorithmConfiguration.AspirationConfiguration;
            AspirationWindow = configuration
                .AspirationWindow;
            AspirationDepth = configuration
                .AspirationDepth;
        }

        #region Overrides of StrategyBase

        public override IResult GetResult()
        {
            var depth = Depth;
            if (Position.GetPhase() == Phase.End)
            {
                depth++;
            }

            var d = depth - AspirationDepth;

            var tempResult = InternalStrategy.GetResult(-SearchValue, SearchValue, d);
            var alpha = tempResult.Value - AspirationWindow;
            var beta = tempResult.Value + AspirationWindow;

            var result = InternalStrategy.GetResult(alpha, beta, depth, tempResult.Move);
            if (result.Value >= beta)
            {
                result = InternalStrategy.GetResult(result.Value, SearchValue, depth, result.Move);
            }
            else if (result.Value <= alpha)
            {
                result = InternalStrategy.GetResult(-SearchValue, result.Value, depth, result.Move);
            }

            return result;
        }

        #endregion
    }
}
