using System.Runtime.CompilerServices;
using Engine.DataStructures.Moves;
using Engine.DataStructures.Moves.Collections.History;
using Engine.Interfaces;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;

namespace Engine.Sorting.Sorters.History
{
    public abstract class HistorySorter : MoveSorter
    {
        protected HistoryMoveCollection HistoryMoveCollection;

        protected HistorySorter(IPosition position, IMoveComparer comparer) : base(position, comparer)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override MoveBase[] OrderInternal(AttackList attacks, MoveList moves)
        {
            OrderAttacks(HistoryMoveCollection, attacks);

            ProcessMoves(moves);

            return HistoryMoveCollection.Build();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override MoveBase[] OrderInternal(AttackList attacks, MoveList moves,
            MoveBase pvNode)
        {
            if (pvNode is AttackBase attack)
            {
                OrderAttacks(HistoryMoveCollection, attacks, attack);

                ProcessMoves(moves);
            }
            else
            {
                OrderAttacks(HistoryMoveCollection, attacks);

                ProcessMoves(moves, pvNode);
            }

            return HistoryMoveCollection.Build();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract void ProcessMoves(MoveList moves);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract void ProcessMoves(MoveList moves, MoveBase pvNode);
    }
}