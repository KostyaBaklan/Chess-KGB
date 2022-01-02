using System;
using Algorithms.Interfaces;
using Infrastructure.Interfaces.Moves;
using Infrastructure.Interfaces.Position;

namespace Algorithms.Strategies.Iterative.Mtd
{
    public abstract class MtStrategyBase : IterativeStrategyBase
    {
        protected MtStrategyBase(short depth, IMoveSorter sorter, IPosition position) : base(depth, sorter, position)
        {
        }

        #region Overrides of IterativeStrategyBase

        protected override int GetInitialGuess()
        {
            return Position.GetValue();
        }

        protected override int GetInitialDepth()
        {
            return Math.Max(4, Depth - 3);
            //int d = 2;
            //if (Depth % 2 == 1) d++;
            //return d;
        }

        protected override int GetGuess(IResult currentResult)
        {
            return Position.GetValue();
        }

        #endregion
    }
}