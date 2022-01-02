using System.Runtime.CompilerServices;
using Engine.Interfaces;

namespace Engine.Models.Moves
{
    public class SimpleAttack : Attack
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool IsLegal(IBoard board)
        {
            return board.IsOpposite(To.AsBitBoard(), Piece);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool IsLegalAttack(IBoard board)
        {
            return true;
        }
    }
}