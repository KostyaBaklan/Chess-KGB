using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Enums;
using Engine.Strategies.AlphaBeta.Extended;

namespace Engine.Strategies.IterativeDeeping
{
    public abstract  class IdStrategyBase:StrategyBase
    {
        protected StrategyBase InternalStrategy;

        protected IdStrategyBase(short depth, IPosition position, StrategyBase strategy) : base(depth, position)
        {
            InternalStrategy = strategy;
        }

        #region Overrides of StrategyBase

        public override IResult GetResult()
        {
            var depth = Depth;
            if (Position.GetPhase() == Phase.End)
            {
                depth++;
            }

            var result = GetResult(-SearchValue, SearchValue, 3);
            if (result.GameResult == GameResult.Continue)
            {
                for (int d = 4; d <= depth; d++)
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

        #endregion
    }

    public class IdExtendedHistoryStrategy : IdStrategyBase
    {
        public IdExtendedHistoryStrategy(short depth, IPosition position) 
            : base(depth, position, new AlphaBetaExtendedHistoryStrategy(depth,position))
        {
        }
    }

    public class IdExtendedDifferenceStrategy : IdStrategyBase
    {
        public IdExtendedDifferenceStrategy(short depth, IPosition position)
            : base(depth, position, new AlphaBetaExtendedDifferenceStrategy(depth, position))
        {
        }
    }

    public class IdExtendedDifferenceHistoryStrategy : IdStrategyBase
    {
        public IdExtendedDifferenceHistoryStrategy(short depth, IPosition position)
            : base(depth, position, new AlphaBetaExtendedDifferenceHistoryStrategy(depth, position))
        {
        }
    }
}
