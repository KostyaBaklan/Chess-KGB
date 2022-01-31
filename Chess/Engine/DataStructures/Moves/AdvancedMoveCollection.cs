using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.DataStructures.Moves
{
    public class AdvancedMoveCollection : AttackCollection
    {
        private readonly List<IMove> _killers;
        private readonly List<IMove> _nonCaptures;
        private readonly List<IMove> _checks;
        protected readonly List<IMove> LooseTrades;

        public AdvancedMoveCollection(IMoveComparer comparer) : base(comparer)
        {
            _killers = new List<IMove>();
            _nonCaptures = new List<IMove>(64);
            _checks = new List<IMove>();
            LooseTrades = new List<IMove>(32);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddCheck(IMove move)
        {
            _checks.Add(move);
            Count++;
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
            _nonCaptures.Add(move);
            Count++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddBadMove(IMove move)
        {
            LooseCaptures.Add(move);
            Count++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddSuggested(IMove move)
        {
            _checks.Add(move);
            Count++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddNonSuggested(IMove move)
        {
            LooseTrades.Add(move);
            Count++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Build()
        {
            Moves = new List<IMove>(Count);

            Moves.AddRange(HashMoves);

            Moves.AddRange(WinCaptures);

            Moves.AddRange(Trades);

            Pv = Math.Max(4, Moves.Count);

            Moves.AddRange(_killers);

            Moves.AddRange(_checks);

            Cut = Math.Max(5, Moves.Count);

            if (_nonCaptures.Count > 1)
            {
                var capturesCount = _nonCaptures.Count < 6 ? _nonCaptures.Count / 2 : _nonCaptures.Count / 3;

                All = Cut + capturesCount;

                for (var i = 0; i < capturesCount; i++)
                {
                    int index = i;
                    var min = _nonCaptures[i];
                    for (int j = i + 1; j < _nonCaptures.Count; j++)
                    {
                        if (Comparer.Compare(_nonCaptures[j], min) >= 0) continue;

                        min = _nonCaptures[j];
                        index = j;
                    }

                    if (index == i) continue;

                    var temp = _nonCaptures[index];
                    _nonCaptures[index] = _nonCaptures[i];
                    _nonCaptures[i] = temp;
                }
            }

            Moves.AddRange(_nonCaptures);

            All = Math.Min(All, Moves.Count);

            Late = Moves.Count;

            Moves.AddRange(LooseTrades);

            Moves.AddRange(LooseCaptures);

            Bad = Count;
        }
    }
}