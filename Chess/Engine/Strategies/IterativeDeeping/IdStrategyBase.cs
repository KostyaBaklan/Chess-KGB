using CommonServiceLocator;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Enums;

namespace Engine.Strategies.IterativeDeeping
{
    public abstract  class IdStrategyBase:StrategyBase
    {
        protected StrategyBase InternalStrategy;
        private readonly int _initialDepth;

        protected IdStrategyBase(short depth, IPosition position, StrategyBase strategy) : base(depth, position)
        {
            InternalStrategy = strategy;
            _initialDepth = ServiceLocator.Current.GetInstance<IConfiguration>().InitialDepth;
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
