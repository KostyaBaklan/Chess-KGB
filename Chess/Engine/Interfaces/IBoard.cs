using Engine.DataStructures;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Moves;

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
        bool IsWhiteOpposite(Square square);
        bool IsBlackOpposite(Square square);
        short GetValue();
        int GetStaticValue();
        Piece GetPiece(Square cell);
        //Piece GetPiece(int cell);
        bool GetPiece(Square cell, out Piece? piece);
        void SetOver(byte to, bool b);
        bool IsOver(byte square);
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
        DynamicArray<byte> GetPositions(int index);
        ulong GetKey();
        Square[] GetPiecePositions(byte piece);
        BitBoard GetOccupied();
        BitBoard GetPieceBits(Piece piece);
        BitBoard GetPerimeter();
        Phase UpdatePhase();
        int StaticExchange(AttackBase attack);
        //Piece[] GetBoardSet();
        int GetBlackMaxValue();
        int GetWhiteMaxValue();
        bool CanWhitePromote();
        bool CanBlackPromote();
        BitBoard GetRank(int rank);
    }
}