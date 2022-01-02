using System.Collections.Generic;
using Infrastructure.Interfaces.Moves;

namespace Infrastructure.Interfaces
{
    public interface IMoveHistoryService
    {
        int Ply { get; }
        bool CanDoWhiteSmallCastle { get; }
        bool CanDoWhiteBigCastle { get; }
        bool CanDoBlackSmallCastle { get; }
        bool CanDoBlackBigCastle { get; }

        bool IsEmpty();
        IMove GetLastMove();
        void Add(IMove move);
        IMove Remove();
        IEnumerable<IMove> GetHistory();
        bool IsAdditionalDebutMove(IMove move);
    }
}
