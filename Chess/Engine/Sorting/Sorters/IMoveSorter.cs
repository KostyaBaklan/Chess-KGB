using System.Collections.Generic;
using Engine.Interfaces;

namespace Engine.Sorting.Sorters
{
    public interface IMoveSorter
    {
        IEnumerable<IMove> Order(IEnumerable<IMove> moves, IMove pvNode, IMove cutMove);
        IEnumerable<IMove> Order(IEnumerable<IMove> attacks, IEnumerable<IMove> moves, IMove pvNode, IMove cutMove);
    }
}