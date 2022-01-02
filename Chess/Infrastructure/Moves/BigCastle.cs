using System.Runtime.CompilerServices;
using Infrastructure.Interfaces.Position;
using Infrastructure.Models.Enums;

namespace Infrastructure.Moves
{
    public class BigCastle : Move
    {
        public BigCastle(Coordinate from, Coordinate to, FigureKind figure)
            : base(from, to, MoveOperation.BigCastle, figure)
        {
        }
        public override MoveType Type => MoveType.BigCastle;

        #region Overrides of Move

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool IsLegal(IBoard board)
        {
            return base.IsLegal(board) && board.CanDoBigCastle(Figure);
        }

        #endregion
    }
}