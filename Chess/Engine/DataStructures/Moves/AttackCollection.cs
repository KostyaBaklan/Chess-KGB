using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.DataStructures.Moves
{
    public class AttackCollection : MoveCollectionBase
    {
        protected readonly List<IMove> _winCaptures;
        protected readonly List<IMove> _trades;
        protected readonly List<IMove> _looseCaptures;

        public AttackCollection(IMoveComparer comparer) : base(comparer)
        {
            _winCaptures = new List<IMove>();
            _trades = new List<IMove>();
            _looseCaptures = new List<IMove>();
        }

        #region Implementation of IMoveCollection

        #endregion

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
        public void AddLooseCapture(IMove move)
        {
            _looseCaptures.Add(move);
            Count++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Build()
        {
            _moves = new List<IMove>(Count);

            _moves.AddRange(_winCaptures);

            _moves.AddRange(_trades);

            _moves.AddRange(_looseCaptures);
        }
    }
}