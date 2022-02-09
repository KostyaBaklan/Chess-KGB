using System;
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
            Moves = new List<IMove>(Count);

            Moves.AddRange(HashMoves);

            Moves.AddRange(WinCaptures);

            Moves.AddRange(Trades);

            Moves.AddRange(_killers);

            _nonCaptures.GetOrderedItems(Moves);

            Moves.AddRange(LooseCaptures);
        }
    }
}