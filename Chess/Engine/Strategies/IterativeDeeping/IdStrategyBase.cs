using CommonServiceLocator;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Enums;
using Engine.Strategies.Base;

namespace Engine.Strategies.IterativeDeeping
{
    public abstract  class IdStrategyBase: ComplexStrategyBase
    {
        private readonly int _initialDepth;

        protected IdStrategyBase(short depth, IPosition position, StrategyBase strategy) : base(depth, position)
        {
            InternalStrategy = strategy;
            _initialDepth = ServiceLocator.Current.GetInstance<IConfigurationProvider>()
                .AlgorithmConfiguration.IterativeDeepingConfiguration.InitialDepth;
        }

        #region Overrides of StrategyBase

        public override IResult GetResult()
        {
            var depth = Depth;
            if (Position.GetPhase() == Phase.End)
            {
                depth++;
            }

            var result = GetResult(-SearchValue, SearchValue, _initialDepth);
            if (result.GameResult == GameResult.Continue)
            {
                for (int d = _initialDepth + 1; d <= depth; d++)
                {
                    result = GetResult(-SearchValue, SearchValue, d, result.Move);
                    if (result.GameResult != GameResult.Continue) break;
                }
            }
            return result;
        }

        #endregion
    }
}
