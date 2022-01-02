using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.Moves;
using Infrastructure.Models.Enums;
using Infrastructure.Moves;

namespace Infrastructure.DataStructures
{
    public class KillerMoveCollection : IKillerMoveCollection
    {
        private int capacity = 3;
        private int _index;
        private readonly IMove[] _moves;
        private static readonly IMove _default = new Move(new Coordinate(0,0),new Coordinate(0,0),MoveOperation.Over,FigureKind.BlackKing);

        public KillerMoveCollection()
        {
            _moves = new IMove[capacity];
            for (int i = 0; i < capacity; i++)
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

            _moves[_index % capacity] = move;
            _index++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ICollection<IMove> GetMoves()
        {
            HashSet<IMove> set = new HashSet<IMove>(capacity);
            foreach (var move in _moves.Where(move => !move.Equals(_default)))
            {
                set.Add(move);
            }

            return set;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ICollection<IMove> GetMoves(IMove cutMove)
        {
            HashSet<IMove> set = new HashSet<IMove>(capacity+1);
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