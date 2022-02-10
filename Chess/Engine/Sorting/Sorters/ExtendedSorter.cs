using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Engine.DataStructures.Moves;
using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Sorting.Sorters
{
    public class ExtendedSorter : MoveSorter
    {
        protected ExtendedMoveCollection ExtendedMoveCollection;
        public ExtendedSorter(IPosition position, IMoveComparer comparer) : base(position, comparer)
        {
            ExtendedMoveCollection = new ExtendedMoveCollection(comparer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override IMove[] OrderInternal(IEnumerable<IAttack> attacks, IEnumerable<IMove> moves,
            KillerMoveCollection killerMoveCollection)
        {
            var sortedAttacks = OrderAttacks(attacks);

            OrderAttacks(ExtendedMoveCollection, sortedAttacks);

            foreach (var move in moves)
            {
                if (killerMoveCollection.Contains(move))
                {
                    ExtendedMoveCollection.AddKillerMove(move);
                }
                else if(move.IsCastle()||move.IsPromotion())
                {
                    ExtendedMoveCollection.AddSuggested(move);
                }
                else
                {
                    ExtendedMoveCollection.AddNonCapture(move);
                }
            }

            return ExtendedMoveCollection.Build();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override IMove[] OrderInternal(IEnumerable<IAttack> attacks, IEnumerable<IMove> moves,
            KillerMoveCollection killerMoveCollection,
            IMove pvNode)
        {
            var sortedAttacks = OrderAttacks(attacks);

            if (pvNode is IAttack attack)
            {
                OrderAttacks(ExtendedMoveCollection, sortedAttacks, attack);

                foreach (var move in moves)
                {
                    if (killerMoveCollection.Contains(move))
                    {
                        ExtendedMoveCollection.AddKillerMove(move);
                    }
                    else if (move.IsCastle() || move.IsPromotion())
                    {
                        ExtendedMoveCollection.AddSuggested(move);
                    }
                    else
                    {
                        ExtendedMoveCollection.AddNonCapture(move);
                    }
                }
            }
            else
            {
                OrderAttacks(ExtendedMoveCollection, sortedAttacks);

                foreach (var move in moves)
                {
                    if (move.Equals(pvNode))
                    {
                        ExtendedMoveCollection.AddHashMove(move);
                    }
                    else
                    {
                        if (killerMoveCollection.Contains(move))
                        {
                            ExtendedMoveCollection.AddKillerMove(move);
                        }
                        else if (move.IsCastle() || move.IsPromotion())
                        {
                            ExtendedMoveCollection.AddSuggested(move);
                        }
                        else
                        {
                            ExtendedMoveCollection.AddNonCapture(move);
                        }
                    }
                }
            }

            return ExtendedMoveCollection.Build();
        }
    }
}