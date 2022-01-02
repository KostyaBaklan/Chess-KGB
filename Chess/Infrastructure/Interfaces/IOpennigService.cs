using System.Collections.Generic;
using Infrastructure.Interfaces.Moves;

namespace Infrastructure.Interfaces
{
    public interface IOpennigService
    {
        void Add(IMove move);
        void Remove();
        ICollection<IMove> GetMoves();
    }
}
