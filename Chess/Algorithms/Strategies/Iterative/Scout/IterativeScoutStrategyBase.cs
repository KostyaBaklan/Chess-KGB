using Infrastructure.Interfaces.Moves;
using Infrastructure.Interfaces.Position;

namespace Algorithms.Strategies.Iterative.Scout
{
    public abstract class IterativeScoutStrategyBase : IterativeStrategyBase
    {
        private int _initialDepth = 5;

        protected IterativeScoutStrategyBase(short depth, IMoveSorter sorter, IPosition position) : base(depth, sorter, position)
        {
        }

        protected override int GetInitialDepth()
        {
            return _initialDepth;
        }

        #region Overrides of IterativeStrategyBase

        protected override int GetInitialGuess()
        {
            return Position.GetValue();
        }

        #endregion
    }
}
