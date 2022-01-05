using System.Collections.Generic;
using Engine.Models.Enums;

namespace Engine.Interfaces
{
    public interface IMoveHistoryService
    {
        int GetPly();
        IMove GetLastMove();
        void Add(IMove move);
        IMove Remove();
        bool CanDoBlackCastle();
        bool CanDoWhiteCastle();
        bool CanDoWhiteSmallCastle();
        bool CanDoWhiteBigCastle();
        bool CanDoBlackSmallCastle();
        bool CanDoBlackBigCastle();
        bool IsAdditionalDebutMove(IMove move);
        IEnumerable<IMove> GetHistory();
    }
}
