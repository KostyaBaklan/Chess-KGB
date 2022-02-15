using System.Runtime.CompilerServices;
using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.DataStructures.Moves
{
    public class ExtendedMoveCollection : AttackCollection
    {
        private readonly MoveList _killers;
        private readonly MoveList _nonCaptures;
        private readonly MoveList _suggested;

        public ExtendedMoveCollection(IMoveComparer comparer) : base(comparer)
        {
            _killers = new MoveList();
            _nonCaptures = new MoveList();
            _suggested = new MoveList();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddCheck(IMove move)
        {
            _suggested.Add(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddKillerMove(IMove move)
        {
            _killers.Add(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddNonCapture(IMove move)
        {
            _nonCaptures.Add(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddSuggested(IMove move)
        {
            _suggested.Add(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override IMove[] Build()
        {
            var hashMovesCount = HashMoves.Count;
            var winCapturesCount = hashMovesCount + WinCaptures.Count;
            var tradesCount = winCapturesCount + Trades.Count;
            var killersCount = tradesCount + _killers.Count;
            int checksCount = killersCount+_suggested.Count;
            var nonCapturesCount = checksCount + _nonCaptures.Count;
            Count = nonCapturesCount + LooseCaptures.Count;

            IMove[] moves = new IMove[Count];

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

            if (_nonCaptures.Count > 0)
            {
                _nonCaptures.Sort(Comparer);
                _nonCaptures.CopyTo(moves, checksCount);
                _nonCaptures.Clear();
            }

            if (LooseCaptures.Count > 0)
            {
                LooseCaptures.CopyTo(moves, nonCapturesCount);
                LooseCaptures.Clear();
            }

            Count = 0;
            return moves;
        }
    }
}