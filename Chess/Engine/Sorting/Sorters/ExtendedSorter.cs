using System.Runtime.CompilerServices;
using Engine.DataStructures.Moves;
using Engine.Interfaces;
using Engine.Models.Enums;
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
        protected override IMove[] OrderInternal(AttackList attacks, MoveList moves,
            IKillerMoveCollection killerMoveCollection)
        {
            var sortedAttacks = OrderAttacks(attacks);

            OrderAttacks(ExtendedMoveCollection, sortedAttacks);

            ProcessMoves(moves, killerMoveCollection);

            return ExtendedMoveCollection.Build();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override IMove[] OrderInternal(AttackList attacks, MoveList moves,
            IKillerMoveCollection killerMoveCollection,
            IMove pvNode)
        {
            var sortedAttacks = OrderAttacks(attacks);

            if (pvNode is IAttack attack)
            {
                OrderAttacks(ExtendedMoveCollection, sortedAttacks, attack);

                ProcessMoves(moves, killerMoveCollection);
            }
            else
            {
                OrderAttacks(ExtendedMoveCollection, sortedAttacks);

                ProcessMoves(moves, killerMoveCollection, pvNode);
            }

            return ExtendedMoveCollection.Build();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessMoves(MoveList moves, IKillerMoveCollection killerMoveCollection)
        {
            if (Position.GetTurn() == Turn.White)
            {
                if (MoveHistoryService.CanDoWhiteCastle())
                {
                    if (Position.CanWhitePromote())
                    {
                        for (var index = 0; index < moves.Count; index++)
                        {
                            var move = moves[index];
                            if (killerMoveCollection.Contains(move.Key))
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
                        for (var index = 0; index < moves.Count; index++)
                        {
                            var move = moves[index];
                            if (killerMoveCollection.Contains(move.Key))
                            {
                                ExtendedMoveCollection.AddKillerMove(move);
                            }
                            else if (move.IsCastle())
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
                else
                {
                    if (Position.CanWhitePromote())
                    {
                        for (var index = 0; index < moves.Count; index++)
                        {
                            var move = moves[index];
                            if (killerMoveCollection.Contains(move.Key))
                            {
                                ExtendedMoveCollection.AddKillerMove(move);
                            }
                            else if (move.IsPromotion())
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
                        for (var index = 0; index < moves.Count; index++)
                        {
                            var move = moves[index];
                            if (killerMoveCollection.Contains(move.Key))
                            {
                                ExtendedMoveCollection.AddKillerMove(move);
                            }
                            else
                            {
                                ExtendedMoveCollection.AddNonCapture(move);
                            }
                        }
                    }
                }
            }
            else
            {
                if (MoveHistoryService.CanDoBlackCastle())
                {
                    if (Position.CanBlackPromote())
                    {
                        for (var index = 0; index < moves.Count; index++)
                        {
                            var move = moves[index];
                            if (killerMoveCollection.Contains(move.Key))
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
                        for (var index = 0; index < moves.Count; index++)
                        {
                            var move = moves[index];
                            if (killerMoveCollection.Contains(move.Key))
                            {
                                ExtendedMoveCollection.AddKillerMove(move);
                            }
                            else if (move.IsCastle())
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
                else
                {
                    if (Position.CanBlackPromote())
                    {
                        for (var index = 0; index < moves.Count; index++)
                        {
                            var move = moves[index];
                            if (killerMoveCollection.Contains(move.Key))
                            {
                                ExtendedMoveCollection.AddKillerMove(move);
                            }
                            else if (move.IsPromotion())
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
                        for (var index = 0; index < moves.Count; index++)
                        {
                            var move = moves[index];
                            if (killerMoveCollection.Contains(move.Key))
                            {
                                ExtendedMoveCollection.AddKillerMove(move);
                            }
                            else
                            {
                                ExtendedMoveCollection.AddNonCapture(move);
                            }
                        }
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessMoves(MoveList moves, IKillerMoveCollection killerMoveCollection, IMove pvNode)
        {
            if (Position.GetTurn() == Turn.White)
            {
                if (MoveHistoryService.CanDoWhiteCastle())
                {
                    if (Position.CanWhitePromote())
                    {
                        for (var index = 0; index < moves.Count; index++)
                        {
                            var move = moves[index];
                            if (move.Equals(pvNode))
                            {
                                ExtendedMoveCollection.AddHashMove(move);
                            }
                            else
                            {
                                if (killerMoveCollection.Contains(move.Key))
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
                    else
                    {
                        for (var index = 0; index < moves.Count; index++)
                        {
                            var move = moves[index];
                            if (move.Equals(pvNode))
                            {
                                ExtendedMoveCollection.AddHashMove(move);
                            }
                            else
                            {
                                if (killerMoveCollection.Contains(move.Key))
                                {
                                    ExtendedMoveCollection.AddKillerMove(move);
                                }
                                else if (move.IsCastle())
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
                }
                else
                {
                    if (Position.CanWhitePromote())
                    {
                        for (var index = 0; index < moves.Count; index++)
                        {
                            var move = moves[index];
                            if (move.Equals(pvNode))
                            {
                                ExtendedMoveCollection.AddHashMove(move);
                            }
                            else
                            {
                                if (killerMoveCollection.Contains(move.Key))
                                {
                                    ExtendedMoveCollection.AddKillerMove(move);
                                }
                                else if (move.IsPromotion())
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
                    else
                    {
                        for (var index = 0; index < moves.Count; index++)
                        {
                            var move = moves[index];
                            if (move.Equals(pvNode))
                            {
                                ExtendedMoveCollection.AddHashMove(move);
                            }
                            else
                            {
                                if (killerMoveCollection.Contains(move.Key))
                                {
                                    ExtendedMoveCollection.AddKillerMove(move);
                                }
                                else
                                {
                                    ExtendedMoveCollection.AddNonCapture(move);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if (MoveHistoryService.CanDoBlackCastle())
                {
                    if (Position.CanBlackPromote())
                    {
                        for (var index = 0; index < moves.Count; index++)
                        {
                            var move = moves[index];
                            if (move.Equals(pvNode))
                            {
                                ExtendedMoveCollection.AddHashMove(move);
                            }
                            else
                            {
                                if (killerMoveCollection.Contains(move.Key))
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
                    else
                    {
                        for (var index = 0; index < moves.Count; index++)
                        {
                            var move = moves[index];
                            if (move.Equals(pvNode))
                            {
                                ExtendedMoveCollection.AddHashMove(move);
                            }
                            else
                            {
                                if (killerMoveCollection.Contains(move.Key))
                                {
                                    ExtendedMoveCollection.AddKillerMove(move);
                                }
                                else if (move.IsCastle())
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
                }
                else
                {
                    if (Position.CanBlackPromote())
                    {
                        for (var index = 0; index < moves.Count; index++)
                        {
                            var move = moves[index];
                            if (move.Equals(pvNode))
                            {
                                ExtendedMoveCollection.AddHashMove(move);
                            }
                            else
                            {
                                if (killerMoveCollection.Contains(move.Key))
                                {
                                    ExtendedMoveCollection.AddKillerMove(move);
                                }
                                else if (move.IsPromotion())
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
                    else
                    {
                        for (var index = 0; index < moves.Count; index++)
                        {
                            var move = moves[index];
                            if (move.Equals(pvNode))
                            {
                                ExtendedMoveCollection.AddHashMove(move);
                            }
                            else
                            {
                                if (killerMoveCollection.Contains(move.Key))
                                {
                                    ExtendedMoveCollection.AddKillerMove(move);
                                }
                                else
                                {
                                    ExtendedMoveCollection.AddNonCapture(move);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}