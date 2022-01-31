using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.DataStructures.Moves
{
    public class AdvancedAttackCollection : AttackCollection
    {
        protected readonly List<IMove> LooseTrades;

        public AdvancedAttackCollection(IMoveComparer comparer) : base(comparer)
        {
            LooseTrades = new List<IMove>(16);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddLooseTrade(IMove move)
        {
            LooseTrades.Add(move);
            Count++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Build()
        {
            Moves = new List<IMove>(Count);

            Moves.AddRange(HashMoves);

            Moves.AddRange(WinCaptures);

            Moves.AddRange(Trades);

            Moves.AddRange(LooseTrades);

            Moves.AddRange(LooseCaptures);
        }
    }
}