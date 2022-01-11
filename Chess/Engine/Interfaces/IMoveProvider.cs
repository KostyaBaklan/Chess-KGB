using System.Collections.Generic;
using Engine.Models.Boards;
using Engine.Models.Enums;

namespace Engine.Interfaces
{
    public interface IMoveProvider
    {
        IEnumerable<IAttack> GetAttacks(Piece piece, Square cell, IBoard board);
        IEnumerable<IMove> GetMoves(Piece piece, Square cell, IBoard board);
        bool AnyBlackCheck(IBoard board);
        bool AnyWhiteCheck(IBoard board);
        bool IsUnderAttack(IBoard board, int piece, byte to);
        BitBoard GetAttackPattern(byte piece, int position);
    }
}