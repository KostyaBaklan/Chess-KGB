using System.Collections.Generic;
using Engine.Models.Moves;

namespace Engine.Interfaces
{
    public interface IMoveHistoryService
    {
        int GetPly();
        MoveBase GetLastMove();
        void Add(MoveBase move);
        MoveBase Remove();
        bool CanDoBlackCastle();
        bool CanDoWhiteCastle();
        bool CanDoWhiteSmallCastle();
        bool CanDoWhiteBigCastle();
        bool CanDoBlackSmallCastle();
        bool CanDoBlackBigCastle();
        bool IsAdditionalDebutMove(MoveBase move);
        IEnumerable<MoveBase> GetHistory();
        bool IsThreefoldRepetition(ulong board);
        void Add(ulong board);
        void Remove(ulong board);
        bool IsLastMoveWasCheck();
        bool IsLastMoveWasPassed();
        bool IsLastMoveNotReducable();
    }
}
