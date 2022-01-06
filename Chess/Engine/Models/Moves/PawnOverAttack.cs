using System.Runtime.CompilerServices;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Helpers;

namespace Engine.Models.Moves
{
    public class PawnOverAttack : Attack
    {
        public PawnOverAttack()
        {
            Type = MoveType.EatOver;
        }

        public Piece Victim { get; set; }
        public int VictimSquare { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool IsLegal(IBoard board)
        {
            return board.IsOver(VictimSquare) && (Piece == Piece.WhitePawn
                       ? board.IsBlackPawn(VictimSquare.AsBitBoard())
                       : board.IsWhitePawn(VictimSquare.AsBitBoard()));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool IsLegalAttack(IBoard board)
        {
            return IsLegal(board);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Make(IBoard board, ArrayStack<Piece?> figureHistory)
        {
            board.SetOver(new Square(VictimSquare), false);
            board.Remove(Victim, new Square(VictimSquare));
            board.Move(Piece, From, To);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void UnMake(IBoard board, ArrayStack<Piece?> figureHistory)
        {
            board.SetOver(new Square(VictimSquare), true);
            board.Move(Piece, To, From);
            board.Add(Victim, new Square(VictimSquare));
        }
    }
}