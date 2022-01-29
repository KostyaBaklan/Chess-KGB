using Engine.Interfaces;

namespace Engine.Strategies.Base
{
    public abstract class ComplexStrategyBase: StrategyBase
    {
        protected StrategyBase InternalStrategy;

        protected ComplexStrategyBase(short depth, IPosition position) : base(depth, position)
        {
        }

        #region Overrides of StrategyBase

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
