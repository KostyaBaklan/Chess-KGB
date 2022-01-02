using System.Collections.Generic;

namespace Infrastructure.Interfaces.Moves
{
    public interface IMoveSorter
    {
        IEnumerable<IMove> Order(IEnumerable<IMove> moves, IMove pvNode, IMove cutMove);
    }
}
