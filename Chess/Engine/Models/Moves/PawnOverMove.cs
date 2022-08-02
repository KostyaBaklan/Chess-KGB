using System.Runtime.CompilerServices;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Enums;

namespace Engine.Models.Moves
{
    public abstract  class PawnOverMove : MoveBase
    {
        public BitBoard OpponentPawns;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool IsReversable()
        {
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void UnMake(IBoard board, ArrayStack<Piece> figureHistory)
        {
            //if (Type == MoveType.Over)
            //{
            //    board.SetOver(To.AsByte(), false);
            //}
            IsEnPassant = false;
            board.Move(Piece, To, From);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool IsLegal(IBoard board)
        {
            return board.IsEmpty(EmptyBoard);
        }
    }

    public class PawnOverWhiteMove: PawnOverMove
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Make(IBoard board, ArrayStack<Piece> figureHistory)
        {
            IsEnPassant = board.IsBlackOver(OpponentPawns);
            board.Move(Piece, From, To);
        }
    }
    public class PawnOverBlackMove : PawnOverMove
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Make(IBoard board, ArrayStack<Piece> figureHistory)
        {
            IsEnPassant = board.IsWhiteOver(OpponentPawns);
            board.Move(Piece, From, To);
        }
    }
}