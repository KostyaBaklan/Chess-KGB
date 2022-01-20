using System.Runtime.CompilerServices;
using CommonServiceLocator;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Enums;

namespace Engine.Models.Helpers
{
    public static class BoardExtensions
    {
        private static readonly IMoveProvider _moveProvider = ServiceLocator.Current.GetInstance<IMoveProvider>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBlackCheckByPawn(this IBoard board, Square from)
        {
            return (_moveProvider.GetAttackPattern(Piece.WhitePawn.AsByte(), from.AsByte()) &
                    board.GetPieceBits(Piece.BlackKing)).Any();
        }
    }
}
