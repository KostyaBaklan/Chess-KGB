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
        protected readonly KillerMoveCollection[] Moves;
        protected readonly IMoveHistoryService MoveHistoryService;
        protected IMoveComparer Comparer;

        protected MoveSorter()
        {
            Moves = new KillerMoveCollection[256];
            for (var i = 0; i < Moves.Length; i++)
            {
                Moves[i] = new KillerMoveCollection();
            }

            MoveHistoryService = ServiceLocator.Current.GetInstance<IMoveHistoryService>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<IMove> Order(IEnumerable<IMove> moves, IMove pvNode, IMove cutMove)
        {
            //var enumerable = moves.ToArray();

            //var staticSort = enumerable.ToList();
            //staticSort.Sort(new StaticComparer());

            //var differenceSort = enumerable.ToList();
            //differenceSort.Sort(new DifferenceComparer());

            int depth = MoveHistoryService.GetPly();
            if (depth < 0) return moves;

            if (pvNode != null)
            {
                return OrderInternal(moves, Moves[depth], pvNode, cutMove);
            }

            if (cutMove != null)
                return OrderInternal(moves, Moves[depth], cutMove);
            return OrderInternal(moves, Moves[depth]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Add(IMove move)
        {
            int depth = MoveHistoryService.GetPly();
            Moves[depth].Add(move);
        }

        protected abstract IEnumerable<IMove> OrderInternal(IEnumerable<IMove> moves, KillerMoveCollection killerMoveCollection);
        protected abstract IEnumerable<IMove> OrderInternal(IEnumerable<IMove> moves, KillerMoveCollection killerMoveCollection, IMove cutMove);
        protected abstract IEnumerable<IMove> OrderInternal(IEnumerable<IMove> moves, KillerMoveCollection move, IMove pvNode, IMove cutMove);
    }
}