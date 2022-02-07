using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Engine.DataStructures.Moves;
using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Sorting.Sorters
{
    public class SimpleMoveSorter : MoveSorter
    {
        public SimpleMoveSorter(IMoveComparer comparer, IPosition position) : base(position)
        {
            Comparer = comparer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override IMoveCollection OrderInternal(IEnumerable<IAttack> attacks, IEnumerable<IMove> moves,
            KillerMoveCollection killerMoveCollection)
        {
            MoveCollection collection = new MoveCollection(Comparer);
            foreach (var attack in attacks)
            {
                collection.AddTrade(attack);
            }

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
            MoveCollection collection = new MoveCollection(Comparer);

            if (pvNode is IAttack a)
            {
                foreach (var attack in attacks)
                {
                    if (attack.Equals(pvNode))
                    {
                        collection.AddHashMove(attack);
                    }
                    else
                    {
                        collection.AddTrade(attack);
                    }
                }

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
            }
            else
            {
                foreach (var attack in attacks)
                {
                    collection.AddTrade(attack);
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
            }

            collection.Build();
            return collection;
        }
    }
}