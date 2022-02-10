using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Engine.DataStructures.Moves;
using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Sorting.Sorters
{
    public class ExtendedSorter : MoveSorter
    {
        public ExtendedSorter(IPosition position, IMoveComparer comparer) : base(position,comparer)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override IMove[] OrderInternal(IEnumerable<IAttack> attacks, IEnumerable<IMove> moves,
            KillerMoveCollection killerMoveCollection)
        {
            var sortedAttacks = OrderAttacks(attacks);

            OrderAttacks(MoveCollection, sortedAttacks);

            foreach (var move in moves)
            {
                if (killerMoveCollection.Contains(move) || move.IsPromotion())
                {
                    MoveCollection.AddKillerMove(move);
                }
                else
                {
                    MoveCollection.AddNonCapture(move);
                }
            }

            return MoveCollection.Build();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override IMove[] OrderInternal(IEnumerable<IAttack> attacks, IEnumerable<IMove> moves,
            KillerMoveCollection killerMoveCollection,
            IMove pvNode)
        {
            var sortedAttacks = OrderAttacks(attacks);

            if (pvNode is IAttack attack)
            {
                OrderAttacks(MoveCollection, sortedAttacks, attack);

                foreach (var move in moves)
                {
                    if (killerMoveCollection.Contains(move) || move.IsPromotion())
                    {
                        MoveCollection.AddKillerMove(move);
                    }
                    else
                    {
                        MoveCollection.AddNonCapture(move);
                    }
                }
            }
            else
            {
                OrderAttacks(MoveCollection, sortedAttacks);

                foreach (var move in moves)
                {
                    if (move.Equals(pvNode))
                    {
                        MoveCollection.AddHashMove(move);
                    }
                    else
                    {
                        if (killerMoveCollection.Contains(move) || move.IsPromotion())
                        {
                            MoveCollection.AddKillerMove(move);
                        }
                        else
                        {
                            MoveCollection.AddNonCapture(move);
                        }
                    }
                }
            }

            return MoveCollection.Build();
        }
    }
}