using System.Collections.Generic;
using Engine.Interfaces;

namespace Engine.Sorting.Sorters
{
    public interface IMoveSorter
    {
        IMove[] Order(IEnumerable<IAttack> attacks, IEnumerable<IMove> moves, IMove pvNode);
        IMove[] Order(IEnumerable<IAttack> attacks);
    }
}