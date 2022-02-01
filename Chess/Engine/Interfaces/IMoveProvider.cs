using System.Collections.Generic;
using Engine.Models.Boards;
using Engine.Models.Enums;

namespace Engine.Interfaces
{
    public interface IMoveProvider
    {
        IMove Get(short key);
        IEnumerable<IMove> GetAll();
        IEnumerable<IAttack> GetAttacks(Piece piece, Square cell, IBoard board);
        IEnumerable<IAttack> GetAttacks(Piece piece, int @from, IBoard board);
        IEnumerable<IAttack> GetAttacks(Piece piece, Square from, int to);
        IEnumerable<IAttack> GetAttacks(Piece piece, int from, int to);
        IEnumerable<IMove> GetMoves(Piece piece, Square cell, IBoard board);
        bool AnyBlackCheck(IBoard board);
        bool AnyWhiteCheck(IBoard board);
        bool IsUnderAttack(IBoard board, int piece, byte to);
        BitBoard GetAttackPattern(byte piece, int position);
        bool IsWhiteUnderAttack(IBoard board, Square square);
        bool IsBlackUnderAttack(IBoard board, Square square);
    }
}