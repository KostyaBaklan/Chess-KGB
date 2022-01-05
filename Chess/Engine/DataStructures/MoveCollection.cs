using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.DataStructures
{
    public class MoveCollection:IMoveCollection
    {
        private List<IMove> _moves;
        private readonly List<IMove> _hashMoves;
        private readonly List<IMove> _winCaptures;
        private readonly List<IMove> _trades;
        private readonly List<IMove> _killers;
        private readonly List<IMove> _nonCaptures;
        private readonly List<IMove> _looseCaptures;

        private readonly IMoveComparer _comparer;

        public MoveCollection(IMoveComparer comparer)
        {
            _comparer = comparer;
            _hashMoves = new List<IMove>();
            _winCaptures = new List<IMove>();
            _trades = new List<IMove>();
            _killers = new List<IMove>();
            _nonCaptures = new List<IMove>(48);
            _looseCaptures = new List<IMove>();
        }

        #region Implementation of IMoveCollection

        public int Count { get; private set; }

        public IMove this[int index] => _moves[index];

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddHashMove(IMove move)
        {
            _hashMoves.Add(move);
            Count++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddWinCapture(IMove move)
        {
            _winCaptures.Add(move);
            Count++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddTrade(IMove move)
        {
            _trades.Add(move);
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
        public void AddLooseCapture(IMove move)
        {
            _looseCaptures.Add(move);
            Count++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Build()
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
