using Engine.DataStructures;
using Engine.Models.Boards;
using Engine.Models.Enums;

namespace Engine.Interfaces
{
    public interface IBoard
    {
        bool IsEmpty(BitBoard bitBoard);
        bool IsBlackPawn(BitBoard bitBoard);
        bool IsWhitePawn(BitBoard bitBoard);
        bool CanDoBlackSmallCastle();
        bool CanDoWhiteSmallCastle();
        bool CanDoBlackBigCastle();
        bool CanDoWhiteBigCastle();
        bool IsOpposite(BitBoard square, Piece piece);
        int GetValue();
        Piece GetPiece(Square cell);
        Piece GetPiece(int cell);
        bool GetPiece(Square cell, out Piece? piece);
        void SetOver(Square to, bool b);
        bool IsOver(int square);
        void DoWhiteSmallCastle();
        void DoBlackSmallCastle();
        void DoBlackBigCastle();
        void DoWhiteBigCastle();
        void UndoWhiteSmallCastle();
        void UndoBlackSmallCastle();
        void UndoWhiteBigCastle();
        void UndoBlackBigCastle();
        void Remove(Piece victim, Square square);
        void Add(Piece victim, Square square);
        void Move(Piece piece, Square from,Square to);
        Square GetWhiteKingPosition();
        Square GetBlackKingPosition();
        DynamicArray<int> GetPositions(int index);
        ulong GetKey();
        Square[] GetPiecePositions(int piece);
        BitBoard GetOccupied();
        void UpdatePhase();
    }
}