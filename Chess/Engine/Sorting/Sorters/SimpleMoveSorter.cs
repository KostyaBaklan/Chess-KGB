using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Engine.DataStructures.Moves;
using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Sorting.Sorters
{
    public class SimpleMoveSorter : MoveSorter
    {
        public SimpleMoveSorter(IMoveComparer comparer, IPosition position) : base(position,comparer)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override IMove[] OrderInternal(IEnumerable<IAttack> attacks, IEnumerable<IMove> moves,
            KillerMoveCollection killerMoveCollection)
        {
            foreach (var attack in attacks)
            {
                MoveCollection.AddTrade(attack);
            }

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
            if (pvNode is IAttack)
            {
                foreach (var attack in attacks)
                {
                    if (attack.Equals(pvNode))
                    {
                        MoveCollection.AddHashMove(attack);
                    }
                    else
                    {
                        MoveCollection.AddTrade(attack);
                    }
                }

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
                foreach (var attack in attacks)
                {
                    MoveCollection.AddTrade(attack);
                }

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