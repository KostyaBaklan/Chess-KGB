using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.Moves;

namespace Infrastructure.DataStructures
{
    public class KillerMoveSet : IKillerMoveCollection
    {
        private readonly HashSet<IMove> _moves;

        public KillerMoveSet()
        {
            _moves = new HashSet<IMove>(4);
        }

        #region Implementation of IKillerMoveCollection

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(IMove move)
        {
            return _moves.Contains(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(IMove move)
        {
            _moves.Add(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ICollection<IMove> GetMoves()
        {
            return _moves;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ICollection<IMove> GetMoves(IMove cutMove)
        {
            _moves.Add(cutMove);
            return _moves;
        }

        #endregion
    }
}