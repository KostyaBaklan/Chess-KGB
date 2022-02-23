using System.Collections.Generic;
using Engine.DataStructures.Moves;
using Engine.Models.Boards;
using Engine.Models.Enums;

namespace Engine.Interfaces
{
    public interface IMoveProvider
    {
        int MovesCount { get; }
        IMove Get(short key);
        IEnumerable<IMove> GetAll();
        IEnumerable<IAttack> GetAttacks(Piece piece, Square cell);
        void GetAttacks(byte piece, Square cell, AttackList attackList);
        void GetAttacks(Piece piece, byte @from, AttackList attackList);
        //IEnumerable<IAttack> GetAttacks(Piece piece, int @from);
        bool AnyLegalAttacksTo(Piece piece, Square from, int to);
        IEnumerable<IAttack> GetAttacks(Piece piece, int from, int to);
        IEnumerable<IMove> GetMoves(Piece piece, Square cell);
        void GetMoves(byte piece, Square cell, MoveList moveList);
        bool AnyBlackCheck();
        bool AnyWhiteCheck();
        bool IsUnderAttack(int piece, byte to);
        BitBoard GetAttackPattern(byte piece, int position);
        bool IsWhiteUnderAttack(Square square);
        bool IsBlackUnderAttack(Square square);
        void SetBoard(IBoard board);
    }
}