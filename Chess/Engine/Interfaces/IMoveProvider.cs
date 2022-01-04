using System.Collections.Generic;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Moves;

namespace Engine.Interfaces
{
    public interface IMoveProvider
    {
        IEnumerable<IMove> GetAttacks(Piece piece, Square cell, IBoard board);
        IEnumerable<IMove> GetMoves(Piece piece, Square cell, IBoard board);
        bool AnyBlackCheck(IBoard board);
        bool AnyWhiteCheck(IBoard board);
        bool IsUnderAttack(IBoard board, int piece, byte to);
        MaterialBalance GetBlackMaterialBalance(IBoard board, byte to);
        MaterialBalance GetWhiteMaterialBalance(IBoard board, byte to);
        BitBoard GetAttackPattern(byte piece, int position);
    }
}