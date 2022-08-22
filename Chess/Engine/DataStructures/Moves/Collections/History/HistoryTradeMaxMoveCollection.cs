using System.Runtime.CompilerServices;
using CommonServiceLocator;
using Engine.Interfaces.Config;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;

namespace Engine.DataStructures.Moves.Collections.History
{
    public class HistoryTradeMaxMoveCollection : HistoryMoveCollection
    {
        private readonly MoveList _suggested;
        private readonly int _extractMaximum;

        public HistoryTradeMaxMoveCollection(IMoveComparer comparer) : base(comparer)
        {
            _suggested = new MoveList();
            _extractMaximum = ServiceLocator.Current.GetInstance<IConfigurationProvider>().AlgorithmConfiguration.SortingConfiguration
                .ExtractMaximum;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override MoveBase[] Build()
        {
            return _nonCaptures.Count > _extractMaximum ? BuildSuggested() : BuildNonCaptures();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private MoveBase[] BuildSuggested()
        {
            _nonCaptures.ExtractMaximum(_extractMaximum, _suggested);

            var hashMovesCount = HashMoves.Count;
            var winCapturesCount = hashMovesCount + WinCaptures.Count;
            var tradesCount = winCapturesCount + Trades.Count;
            int killersCount = tradesCount + _killers.Count;
            var suggestedCount = killersCount + _suggested.Count;
            var nonCapturesCount = suggestedCount + LooseCaptures.Count;
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
                LooseCaptures.CopyTo(moves, suggestedCount);
                LooseCaptures.Clear();
            }

            if (_nonCaptures.Count > 0)
            {
                _nonCaptures.Sort();
                _nonCaptures.CopyTo(moves, nonCapturesCount);
                _nonCaptures.Clear();
            }

            Count = 0;
            return moves;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private MoveBase[] BuildNonCaptures()
        {
            var hashMovesCount = HashMoves.Count;
            var winCapturesCount = hashMovesCount + WinCaptures.Count;
            var tradesCount = winCapturesCount + Trades.Count;
            int killersCount = tradesCount + _killers.Count;
            var nonCapturesCount = killersCount + _nonCaptures.Count;
            Count = nonCapturesCount + LooseCaptures.Count;

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

            if (_nonCaptures.Count > 0)
            {
                _nonCaptures.FullSort();
                _nonCaptures.CopyTo(moves, killersCount);
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