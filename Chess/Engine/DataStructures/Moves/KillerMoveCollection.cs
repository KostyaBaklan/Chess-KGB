using System.Runtime.CompilerServices;
using Engine.Interfaces;
using Engine.Models.Moves;

namespace Engine.DataStructures.Moves
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
            for (int i = 0; i < _capacity; i++)
            {
                if (_moves[i].Equals(move)) return true;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(IMove move)
        {
            _moves[_index % _capacity] = move;
            _index++;
        }
    }
}
