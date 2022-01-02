using System.Collections.Generic;
using Infrastructure.Interfaces.Moves;

namespace Infrastructure.Sorters
{
    public class DefaultMoveSorter : IMoveSorter
    {
        #region Implementation of IMoveSorter

        public IEnumerable<IMove> Order(IEnumerable<IMove> moves, IMove pvNode, IMove cutMove)
        {
            return moves;
        }

        #endregion
    }
}