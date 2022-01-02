using System.Runtime.CompilerServices;
using Infrastructure.Interfaces.Position;
using Infrastructure.Models.Enums;

namespace Infrastructure.Moves
{
    public class PawnOverAttack:Attack
    {
        public PawnOverAttack(Coordinate from, Coordinate to, FigureKind figure)
            : base(from, to, MoveOperation.EatOver, figure)
        {
        }
        public override MoveType Type => MoveType.PawnOverAttack;

        #region Overrides of Attack

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool IsLegal(IBoard board)
        {
            return board.IsEmpty(To.Key) && board.CanAttackOver(To.X,From.Y, Figure);
        }

        #endregion
    }
}