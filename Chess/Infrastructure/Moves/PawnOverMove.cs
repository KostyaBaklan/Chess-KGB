using System.Runtime.CompilerServices;
using Infrastructure.Interfaces.Position;
using Infrastructure.Models.Enums;

namespace Infrastructure.Moves
{
    public class PawnOverMove : Move
    {
        public PawnOverMove(Coordinate from, Coordinate to, FigureKind figure)
            : base(from, to, MoveOperation.MovePawn, figure)
        {
        }
        public override MoveType Type => MoveType.PawnOverMove;

        #region Overrides of Move

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool IsLegal(IBoard board)
        {
            if (!base.IsLegal(board)) return false;

            var x = To.X - 1;
            if (x > -1)
            {
                if (Figure == FigureKind.WhitePawn)
                {
                    if (board[(byte) (x * 8 + To.Y)] == FigureKind.BlackPawn)
                    {
                        Operation = MoveOperation.Over;
                        return true;
                    }
                }
                else
                {
                    if (board[(byte) (x * 8 + To.Y)] == FigureKind.WhitePawn)
                    {
                        Operation = MoveOperation.Over;
                        return true;
                    }
                }
            }
            x = To.X + 1;
            if (x < 8)
            {
                if (Figure == FigureKind.WhitePawn)
                {
                    if (board[(byte) (x * 8 + To.Y)] != FigureKind.BlackPawn) return true;
                    Operation = MoveOperation.Over;
                    return true;
                }

                if (board[(byte) (x * 8 + To.Y)] != FigureKind.WhitePawn) return true;
                Operation = MoveOperation.Over;
                return true;
            }
            return true;
        }

        #endregion
    }
}