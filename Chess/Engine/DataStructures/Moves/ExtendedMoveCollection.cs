using System.Runtime.CompilerServices;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;

namespace Engine.DataStructures.Moves
{
    public abstract class ExtendedMoveCollection : AttackCollection
    {
        protected readonly MoveList _killers;
        protected readonly MoveList _nonCaptures;
        protected readonly MoveList _suggested;

        protected ExtendedMoveCollection(IMoveComparer comparer) : base(comparer)
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
    }
    public class ExtendedKillerMoveCollection : ExtendedMoveCollection
    {
        public ExtendedKillerMoveCollection(IMoveComparer comparer) : base(comparer)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override MoveBase[] Build()
        {
            var hashMovesCount = HashMoves.Count;
            var winCapturesCount = hashMovesCount + WinCaptures.Count;
            var tradesCount = winCapturesCount + Trades.Count;
            int killersCount = tradesCount + _killers.Count;
            var suggestedCount = killersCount + _suggested.Count;
            var nonCapturesCount = suggestedCount + LooseCaptures.Count;
            Count = nonCapturesCount + _nonCaptures.Count;

            MoveBase[] moves = new MoveBase[Count];

            if (suggestedCount > 0)
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
                    LooseCaptures.CopyTo(moves, suggestedCount);
                    LooseCaptures.Clear();
                }

                if (_nonCaptures.Count > 0)
                {
                    _nonCaptures.Sort(Comparer);
                    _nonCaptures.CopyTo(moves, nonCapturesCount);
                    _nonCaptures.Clear();
                }
            }
            else
            {
                var capturesCount = _nonCaptures.Count;
                if (capturesCount > 0)
                {
                    _nonCaptures.Sort(Comparer);
                    _nonCaptures.CopyTo(moves, 0);
                    _nonCaptures.Clear();
                }
                if (LooseCaptures.Count > 0)
                {
                    LooseCaptures.CopyTo(moves, capturesCount);
                    LooseCaptures.Clear();
                }
            }
            Count = 0;
            return moves;
        }
    }
    public class ExtendedSuggestedMoveCollection : ExtendedMoveCollection
    {
        public ExtendedSuggestedMoveCollection(IMoveComparer comparer) : base(comparer)
        { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override MoveBase[] Build()
        {
            var hashMovesCount = HashMoves.Count;
            var winCapturesCount = hashMovesCount + WinCaptures.Count;
            var tradesCount = winCapturesCount + Trades.Count;
            var suggestedCount = tradesCount + _suggested.Count;
            int killersCount = suggestedCount + _killers.Count;
            var nonCapturesCount = killersCount + LooseCaptures.Count;
            Count = nonCapturesCount + _nonCaptures.Count;

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
                    _nonCaptures.Sort(Comparer);
                    _nonCaptures.CopyTo(moves, nonCapturesCount);
                    _nonCaptures.Clear();
                }
            }
            else
            {
                var capturesCount = _nonCaptures.Count;
                if (capturesCount > 0)
                {
                    _nonCaptures.Sort(Comparer);
                    _nonCaptures.CopyTo(moves, 0);
                    _nonCaptures.Clear();
                }
                if (LooseCaptures.Count > 0)
                {
                    LooseCaptures.CopyTo(moves, capturesCount);
                    LooseCaptures.Clear();
                }
            }
            Count = 0;
            return moves;
        }
    }
}