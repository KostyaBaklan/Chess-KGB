using System.Collections.Generic;
using Engine.Interfaces;

namespace Engine.Sorting.Sorters
{
    public interface IMoveSorter
    {
        IMoveCollection Order(IEnumerable<IMove> attacks, IEnumerable<IMove> moves, IMove pvNode, IMove cutMove);
    }
}