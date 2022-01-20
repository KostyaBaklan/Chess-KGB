using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.DataStructures.Moves
{
    public class AdvancedAttackCollection : AttackCollection
    {
        protected readonly List<IMove> _looseTrades;

        public AdvancedAttackCollection(IMoveComparer comparer) : base(comparer)
        {
            _looseTrades = new List<IMove>(16);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddLooseTrade(IMove move)
        {
            _looseTrades.Add(move);
            Count++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Build()
        {
            _moves = new List<IMove>(Count);

            _moves.AddRange(_hashMoves);

            _moves.AddRange(_winCaptures);

            _moves.AddRange(_trades);

            _moves.AddRange(_looseTrades);

            _moves.AddRange(_looseCaptures);
        }
    }
}