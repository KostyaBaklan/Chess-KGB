using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.DataStructures.Moves
{
    public class AttackCollection : MoveCollectionBase
    {
        protected readonly List<IMove> WinCaptures;
        protected readonly List<IMove> Trades;
        protected readonly List<IMove> LooseCaptures;
        protected readonly List<IMove> HashMoves;

        public AttackCollection(IMoveComparer comparer) : base(comparer)
        {
            WinCaptures = new List<IMove>();
            Trades = new List<IMove>();
            LooseCaptures = new List<IMove>();
            HashMoves = new List<IMove>(1);
        }

        #region Implementation of IMoveCollection

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddWinCapture(IMove move)
        {
            WinCaptures.Add(move);
            Count++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddTrade(IMove move)
        {
            Trades.Add(move);
            Count++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddLooseCapture(IMove move)
        {
            LooseCaptures.Add(move);
            Count++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddHashMove(IMove move)
        {
            HashMoves.Add(move);
            Count++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Build()
        {
            Moves = new List<IMove>(Count);

            Moves.AddRange(HashMoves);

            Moves.AddRange(WinCaptures);

            Moves.AddRange(Trades);

            Moves.AddRange(LooseCaptures);
        }
    }
}