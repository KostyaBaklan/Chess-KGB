using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CommonServiceLocator;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Sorting.Sorters
{
    public abstract class MoveSorter : IMoveSorter
    {
        protected readonly KillerMoveCollection[] _moves;
        protected readonly IMoveHistoryService _moveHistoryService;
        protected IMoveComparer Comparer;

        protected MoveSorter()
        {
            _moves = new KillerMoveCollection[256];
            for (var i = 0; i < _moves.Length; i++)
            {
                _moves[i] = new KillerMoveCollection();
            }

            _moveHistoryService = ServiceLocator.Current.GetInstance<IMoveHistoryService>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<IMove> Order(IEnumerable<IMove> moves, IMove pvNode, IMove cutMove)
        {
            int depth = _moveHistoryService.GetPly();
            if (depth < 0) return moves;

            if (pvNode != null)
            {
                return OrderInternal(moves, _moves[depth], pvNode, cutMove);
            }

            if (cutMove != null)
                return OrderInternal(moves, _moves[depth], cutMove);
            return OrderInternal(moves, _moves[depth]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Add(IMove move)
        {
            int depth = _moveHistoryService.GetPly();
            _moves[depth].Add(move);
        }

        protected abstract IEnumerable<IMove> OrderInternal(IEnumerable<IMove> moves, KillerMoveCollection killerMoveCollection);
        protected abstract IEnumerable<IMove> OrderInternal(IEnumerable<IMove> moves, KillerMoveCollection killerMoveCollection, IMove cutMove);
        protected abstract IEnumerable<IMove> OrderInternal(IEnumerable<IMove> moves, KillerMoveCollection move, IMove pvNode, IMove cutMove);
    }
}