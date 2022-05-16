using System.Runtime.CompilerServices;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;

namespace Engine.DataStructures.Moves
{
    public class InitialMoveCollection : AttackCollection
    {
        private readonly MoveList _killers;
        private readonly MoveList _nonCaptures;
        private readonly MoveList _notSuggested;
        private readonly MoveList _suggested;
        private readonly MoveList _bad;

        public InitialMoveCollection(IMoveComparer comparer) : base(comparer)
        {
            _killers = new MoveList();
            _nonCaptures = new MoveList();
            _notSuggested = new MoveList();
            _suggested = new MoveList();
            _bad = new MoveList();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddKillerMove(MoveBase move)
        {
            _killers.Add(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddSuggested(MoveBase move)
        {
            _suggested.Add(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddNonCapture(MoveBase move)
        {
            _nonCaptures.Add(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddNonSuggested(MoveBase move)
        {
            _notSuggested.Add(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddBad(MoveBase move)
        {
            _bad.Add(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override MoveBase[] Build()
        {
            var hashMovesCount = HashMoves.Count;
            var winCapturesCount = hashMovesCount + WinCaptures.Count;
            var tradesCount = winCapturesCount + Trades.Count;
            var suggestedCount = tradesCount + _suggested.Count;
            var killersCount = suggestedCount + _killers.Count;
            var looseCapturesCount = killersCount + LooseCaptures.Count;
            var nonCapturesCount = looseCapturesCount + _nonCaptures.Count;
            var notSuggestedCount = nonCapturesCount + _notSuggested.Count;
            Count = notSuggestedCount + _bad.Count;

            MoveBase[] moves = new MoveBase[Count];
            if (killersCount > 0)
            {
                if (hashMovesCount > 0)
                {
                    HashMoves.CopyTo(moves, 0);
                    HashMoves.Clear();
                }

                if (WinCaptures.Count > 0)
                {
                    WinCaptures.CopyTo(moves, hashMovesCount);
                    WinCaptures.Clear();
                }

                if (Trades.Count > 0)
                {
                    Trades.CopyTo(moves, winCapturesCount);
                    Trades.Clear();
                }

                if (_suggested.Count > 0)
                {
                    _suggested.FullSort(Comparer);
                    _suggested.CopyTo(moves, tradesCount);
                    _suggested.Clear();
                }

                if (_killers.Count > 0)
                {
                    _killers.CopyTo(moves, suggestedCount);
                    _killers.Clear();
                }

                if (LooseCaptures.Count > 0)
                {
                    LooseCaptures.CopyTo(moves, killersCount);
                    LooseCaptures.Clear();
                }

                if (_nonCaptures.Count > 0)
                {
                    _nonCaptures.FullSort(Comparer);
                    _nonCaptures.CopyTo(moves, looseCapturesCount);
                    _nonCaptures.Clear();
                }

                if (_notSuggested.Count > 0)
                {
                    _notSuggested.FullSort(Comparer);
                    _notSuggested.CopyTo(moves, nonCapturesCount);
                    _notSuggested.Clear();
                }

                if (_bad.Count > 0)
                {
                    _bad.CopyTo(moves, notSuggestedCount);
                    _bad.Clear();
                } 
            }
            else
            {
                var capturesCount = _nonCaptures.Count;
                if (capturesCount > 0)
                {
                    _nonCaptures.FullSort(Comparer);
                    _nonCaptures.CopyTo(moves, 0);
                    _nonCaptures.Clear();
                }
                if (LooseCaptures.Count > 0)
                {
                    LooseCaptures.CopyTo(moves, capturesCount);
                    LooseCaptures.Clear();
                }

                if (_notSuggested.Count > 0)
                {
                    _notSuggested.FullSort(Comparer);
                    _notSuggested.CopyTo(moves, nonCapturesCount);
                    _notSuggested.Clear();
                }

                if (_bad.Count > 0)
                {
                    _bad.CopyTo(moves, notSuggestedCount);
                    _bad.Clear();
                }
            }

            Count = 0;
            return moves;
        }
    }
}