using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Engine.DataStructures.Moves;
using Engine.Interfaces;
using Engine.Models.Enums;
using Engine.Sorting.Comparers;

namespace Engine.Sorting.Sorters
{
    public class BasicSorter : MoveSorter
    {
        public BasicSorter(IPosition position, IMoveComparer comparer) : base(position,comparer)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override IMove[] OrderInternal(IEnumerable<IAttack> attacks, IEnumerable<IMove> moves,
            IKillerMoveCollection killerMoveCollection)
        {
            var sortedAttacks = OrderAttacks(attacks);

            OrderAttacks(MoveCollection, sortedAttacks);

            ProcessMoves(moves, killerMoveCollection);

            return MoveCollection.Build();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override IMove[] OrderInternal(IEnumerable<IAttack> attacks, IEnumerable<IMove> moves,
            IKillerMoveCollection killerMoveCollection,
            IMove pvNode)
        {
            var sortedAttacks = OrderAttacks(attacks);

            if (pvNode is IAttack attack)
            {
                OrderAttacks(MoveCollection, sortedAttacks, attack);

                ProcessMoves(moves, killerMoveCollection);
            }
            else
            {
                OrderAttacks(MoveCollection, sortedAttacks);

                ProcessMoves(moves, killerMoveCollection, pvNode);
            }

            return MoveCollection.Build();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessMoves(IEnumerable<IMove> moves, IKillerMoveCollection killerMoveCollection, IMove pvNode)
        {
            if (Position.GetTurn() == Turn.White)
            {
                if (Position.CanWhitePromote())
                {
                    foreach (var move in moves)
                    {
                        if (move.Equals(pvNode))
                        {
                            MoveCollection.AddHashMove(move);
                        }
                        else if (killerMoveCollection.Contains(move.Key) || move.IsPromotion())
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
                    foreach (var move in moves)
                    {
                        if (move.Equals(pvNode))
                        {
                            MoveCollection.AddHashMove(move);
                        }
                        else if (killerMoveCollection.Contains(move.Key))
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
            else
            {
                if (Position.CanBlackPromote())
                {
                    foreach (var move in moves)
                    {
                        if (move.Equals(pvNode))
                        {
                            MoveCollection.AddHashMove(move);
                        }
                        else if (killerMoveCollection.Contains(move.Key) || move.IsPromotion())
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
                    foreach (var move in moves)
                    {
                        if (move.Equals(pvNode))
                        {
                            MoveCollection.AddHashMove(move);
                        }
                        else if (killerMoveCollection.Contains(move.Key))
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
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessMoves(IEnumerable<IMove> moves, IKillerMoveCollection killerMoveCollection)
        {
            if (Position.GetTurn() == Turn.White)
            {
                if (Position.CanWhitePromote())
                {
                    foreach (var move in moves)
                    {
                        if (killerMoveCollection.Contains(move.Key) || move.IsPromotion())
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
                    foreach (var move in moves)
                    {
                        if (killerMoveCollection.Contains(move.Key))
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
            else
            {
                if (Position.CanBlackPromote())
                {
                    foreach (var move in moves)
                    {
                        if (killerMoveCollection.Contains(move.Key) || move.IsPromotion())
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
                    foreach (var move in moves)
                    {
                        if (killerMoveCollection.Contains(move.Key))
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
        }
    }
}