using System.Collections.Generic;
using Infrastructure.Interfaces.Moves;

namespace Infrastructure.Interfaces
{
    public interface IKillerMoveCollection
    {
        bool Contains(IMove move);
        void Add(IMove move);
        ICollection<IMove> GetMoves();
        ICollection<IMove> GetMoves(IMove cutMove);
    }
}