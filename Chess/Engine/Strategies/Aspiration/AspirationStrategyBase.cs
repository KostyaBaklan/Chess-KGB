using System;
using CommonServiceLocator;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Enums;

namespace Engine.Strategies.Aspiration
{
    public abstract class AspirationStrategyBase : StrategyBase
    {
        protected int AspirationWindow;
        protected int AspirationDepth;
        protected StrategyBase InternalStrategy;

        protected AspirationStrategyBase(short depth, IPosition position) : base(depth, position)
        {
            var configurationProvider = ServiceLocator.Current.GetInstance<IConfigurationProvider>();
            AspirationWindow = configurationProvider.AlgorithmConfiguration
                .AspirationWindow;
            AspirationDepth = configurationProvider.AlgorithmConfiguration
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

            //var d = Math.Max(AspirationDepth, depth / 2);

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

        public override IResult GetResult(int alpha, int beta, int depth, IMove pvMove = null)
        {
            return InternalStrategy.GetResult(alpha, beta, depth, pvMove);
        }

        public override int Search(int alpha, int beta, int depth)
        {
            return InternalStrategy.Search(alpha, beta, depth);
        }

        public override int Size => InternalStrategy.Size;

        #endregion
    }
}
