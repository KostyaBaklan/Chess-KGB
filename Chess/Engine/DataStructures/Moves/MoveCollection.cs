﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.DataStructures.Moves
{
    public class MoveCollection: AttackCollection
    {
        private readonly List<IMove> _hashMoves;
        private readonly List<IMove> _killers;
        private readonly List<IMove> _nonCaptures;

        public MoveCollection(IMoveComparer comparer) : base(comparer)
        {
            _hashMoves = new List<IMove>();
            _killers = new List<IMove>();
            _nonCaptures = new List<IMove>(48);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddHashMove(IMove move)
        {
            _hashMoves.Add(move);
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
        public override void Build()
        {
            _moves = new List<IMove>(Count);

            _moves.AddRange(_hashMoves);

            _moves.AddRange(_winCaptures);

            _moves.AddRange(_trades);

            _moves.AddRange(_killers);

            for (var i = 0; i < Math.Min(5,_nonCaptures.Count - 1); i++)
            {
                int index = i;
                var min = _nonCaptures[i];
                for (int j = i+1; j < _nonCaptures.Count; j++)
                {
                    if (_comparer.Compare(_nonCaptures[j], min) < 0)
                    {
                        min = _nonCaptures[j];
                        index = j;
                    }
                }

                if (index != i)
                {
                    var temp = _nonCaptures[index];
                    _nonCaptures[index] = _nonCaptures[i];
                    _nonCaptures[i] = temp;
                }
            }
            _moves.AddRange(_nonCaptures);

            _moves.AddRange(_looseCaptures);
        }
    }
}