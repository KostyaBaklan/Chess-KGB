using System.Runtime.CompilerServices;
using Engine.DataStructures.Moves;
using Engine.DataStructures.Moves.Collections;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;

namespace OpeningTool
{
    internal class OpeningMoveCollection : AttackCollection
    {
        private readonly MoveList _killers;
        private readonly MoveList _nonCaptures;
        private readonly MoveList _suggested;

        public OpeningMoveCollection(IMoveComparer comparer) : base(comparer)
        {
            _killers = new MoveList();
            _nonCaptures = new MoveList();
            _suggested = new MoveList();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddCheck(MoveBase move)
        {
            _suggested.Add(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddKillerMove(MoveBase move)
        {
            _killers.Add(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddNonCapture(MoveBase move)
        {
            _nonCaptures.Add(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddSuggested(MoveBase move)
        {
            _suggested.Add(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override MoveBase[] Build()
        {
            var hashMovesCount = HashMoves.Count;
            var winCapturesCount = hashMovesCount + WinCaptures.Count;
            var tradesCount = winCapturesCount + Trades.Count;
            var killersCount = tradesCount + _killers.Count;
            int checksCount = killersCount + _suggested.Count;
            var nonCapturesCount = checksCount + LooseCaptures.Count;
            Count = nonCapturesCount + _nonCaptures.Count;

            MoveBase[] moves = new MoveBase[Count];

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

            if (_killers.Count > 0)
            {
                _killers.CopyTo(moves, tradesCount);
                _killers.Clear();
            }

            if (_suggested.Count > 0)
            {
                _suggested.CopyTo(moves, killersCount);
                _suggested.Clear();
            }

            if (LooseCaptures.Count > 0)
            {
                LooseCaptures.CopyTo(moves, checksCount);
                LooseCaptures.Clear();
            }

            if (_nonCaptures.Count > 0)
            {
                _nonCaptures.FullSort();
                _nonCaptures.CopyTo(moves, nonCapturesCount);
                _nonCaptures.Clear();
            }

            Count = 0;
            return moves;
        }
    }
}