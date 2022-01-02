using System.Runtime.CompilerServices;
using Infrastructure.Interfaces.Moves;
using Infrastructure.Models.Enums;

namespace Infrastructure.Helpers
{
    public static class MoveExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsCastle(this IMove move)
        {
            return move.Type == MoveType.SmallCastle || move.Type == MoveType.BigCastle;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAttack(this IMove move)
        {
            return move.Type == MoveType.Attack || move.Type == MoveType.PawnOverAttack;
        }
    }
}
