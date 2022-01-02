using System.Runtime.CompilerServices;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Enums;
using Engine.Models.Helpers;

namespace Engine.Models.Moves
{
    public class PawnOverMove : MoveBase
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool IsLegal(IBoard board)
        {
            Type = MoveType.MovePawn;
            if (!board.IsEmpty(EmptyBoard)) return false;

            if (Piece == Piece.WhitePawn)
            {
                var x = To - 1;
                if (x > 23 && board.IsBlackPawn(x.AsBitBoard()))
                {
                    Type = MoveType.Over;
                }

                x = To + 1;
                if (x < 32 && board.IsBlackPawn(x.AsBitBoard()))
                {
                    Type = MoveType.Over;
                }
            }
            else
            {
                var x = To - 1;
                if (x > 31 && board.IsWhitePawn(x.AsBitBoard()))
                {
                    Type = MoveType.Over;
                }

                x = To + 1;
                if (x < 40 && board.IsWhitePawn(x.AsBitBoard()))
                {
                    Type = MoveType.Over;
                }
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Make(IBoard board, ArrayStack<Piece?> figureHistory)
        {
            if (Type == MoveType.Over)
            {
                board.SetOver(To,true );
            }
            board.Move(Piece, From, To);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void UnMake(IBoard board, ArrayStack<Piece?> figureHistory)
        {
            if (Type == MoveType.Over)
            {
                board.SetOver(To, false);
            }
            board.Move(Piece, To, From);
        }
    }
}