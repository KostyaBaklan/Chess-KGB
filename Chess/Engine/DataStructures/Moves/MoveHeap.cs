using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.DataStructures.Moves
{
    public class MoveHeap : AttackCollection
    {
        private readonly List<IMove> _killers;
        private readonly MaxHeap _nonCaptures;

        public MoveHeap(IMoveComparer comparer) : base(comparer)
        {
            _killers = new List<IMove>();
            _nonCaptures = new MaxHeap(64, comparer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddKillerMove(IMove move)
        {
            _killers.Add(move);
            Count++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddNonCapture(IMove move)
        {
            _nonCaptures.Insert(move);
            Count++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Build()
        {
            _moves = new List<IMove>(Count);

            _moves.AddRange(_hashMoves);

            _moves.AddRange(_winCaptures);

            _moves.AddRange(_trades);

            _moves.AddRange(_killers);

            _nonCaptures.GetOrderedItems(_moves);

            _moves.AddRange(_looseCaptures);
        }
    }
}