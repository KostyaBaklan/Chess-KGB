using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Engine.Interfaces;
using Engine.Models.Moves;

namespace Engine.DataStructures
{
    public class KillerMoveCollection
    {
        private readonly int _capacity = 2;
        private int _index;
        private readonly IMove[] _moves;
        private static readonly IMove _default = new Move();

        public KillerMoveCollection()
        {
            _moves = new IMove[_capacity];
            for (int i = 0; i < _capacity; i++)
            {
                _moves[i] = _default;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(IMove move)
        {
            return _moves.Contains(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(IMove move)
        {
            if (_moves.Contains(move)) return;

            _moves[_index % _capacity] = move;
            _index++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ICollection<IMove> GetMoves()
        {
            HashSet<IMove> set = new HashSet<IMove>(_capacity);
            foreach (var move in _moves.Where(move => !move.Equals(_default)))
            {
                set.Add(move);
            }

            return set;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ICollection<IMove> GetMoves(IMove cutMove)
        {
            HashSet<IMove> set = new HashSet<IMove>(_capacity + 1);
            foreach (var move in _moves.Where(move => !move.Equals(_default)))
            {
                set.Add(move);
            }

            if (cutMove != null)
            {
                set.Add(cutMove);
            }

            return set;
        }
    }
}
