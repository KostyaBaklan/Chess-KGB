using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Engine.DataStructures.Moves;
using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Sorting.Sorters
{
    public class ExtendedSorter : MoveSorter
    {
        public ExtendedSorter(IPosition position, IMoveComparer comparer) : base(position)
        {
            Comparer = comparer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override IMoveCollection OrderInternal(IEnumerable<IAttack> attacks, IEnumerable<IMove> moves,
            KillerMoveCollection killerMoveCollection)
        {
            var sortedAttacks = OrderAttacks(attacks);

            MoveCollection collection = new MoveCollection(Comparer);

            OrderAttacks(collection, sortedAttacks);

            foreach (var move in moves)
            {
                if (killerMoveCollection.Contains(move) || move.IsPromotion())
                {
                    collection.AddKillerMove(move);
                }
                else
                {
                    collection.AddNonCapture(move);
                }
            }

            collection.Build();
            return collection;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override IMoveCollection OrderInternal(IEnumerable<IAttack> attacks, IEnumerable<IMove> moves,
            KillerMoveCollection killerMoveCollection,
            IMove pvNode)
        {
            var sortedAttacks = OrderAttacks(attacks);

            MoveCollection collection = new MoveCollection(Comparer);

            if (pvNode is IAttack attack)
            {
                OrderAttacks(collection, sortedAttacks, attack);
            }
            else
            {
                OrderAttacks(collection, sortedAttacks);
            }

            foreach (var move in moves)
            {
                if (move.Equals(pvNode))
                {
                    collection.AddHashMove(move);
                }
                else
                {
                    if (killerMoveCollection.Contains(move) || move.IsPromotion())
                    {
                        collection.AddKillerMove(move);
                    }
                    else
                    {
                        collection.AddNonCapture(move);
                    }
                }
            }

            collection.Build();
            return collection;
        }
    }
}