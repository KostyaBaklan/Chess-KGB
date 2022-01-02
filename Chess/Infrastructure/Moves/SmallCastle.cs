using System.Runtime.CompilerServices;
using Infrastructure.Interfaces.Position;
using Infrastructure.Models.Enums;

namespace Infrastructure.Moves
{
    public class SmallCastle:Move
    {
        public SmallCastle(Coordinate from, Coordinate to, FigureKind figure)
            : base(from, to, MoveOperation.SmallCastle, figure)
        {
        }
        public override MoveType Type => MoveType.SmallCastle;

        #region Overrides of Move

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool IsLegal(IBoard board)
        {
            return base.IsLegal(board) && board.CanDoSmallCastle(Figure);
        }

        #endregion
    }
}