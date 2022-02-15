using System.Collections.Generic;
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
        protected override IMove[] OrderInternal(IEnumerable<IAttack> attacks, IEnumerable<IMove> moves,
            KillerMoveCollection killerMoveCollection)
        {
            var sortedAttacks = OrderAttacks(attacks);

            OrderAttacks(ExtendedMoveCollection, sortedAttacks);

            ProcessMoves(moves, killerMoveCollection);

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
        private void ProcessMoves(IEnumerable<IMove> moves, KillerMoveCollection killerMoveCollection)
        {
            if (Position.GetTurn() == Turn.White)
            {
                if (MoveHistoryService.CanDoWhiteCastle())
                {
                    if (Position.CanWhitePromote())
                    {
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
                        foreach (var move in moves)
                        {
                            if (killerMoveCollection.Contains(move))
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
                        foreach (var move in moves)
                        {
                            if (killerMoveCollection.Contains(move))
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
                        foreach (var move in moves)
                        {
                            if (killerMoveCollection.Contains(move))
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
                        foreach (var move in moves)
                        {
                            if (killerMoveCollection.Contains(move))
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
                        foreach (var move in moves)
                        {
                            if (killerMoveCollection.Contains(move))
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
                        foreach (var move in moves)
                        {
                            if (killerMoveCollection.Contains(move))
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
        private void ProcessMoves(IEnumerable<IMove> moves, KillerMoveCollection killerMoveCollection, IMove pvNode)
        {
            if (Position.GetTurn() == Turn.White)
            {
                if (MoveHistoryService.CanDoWhiteCastle())
                {
                    if (Position.CanWhitePromote())
                    {
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
                    else
                    {
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
                    else
                    {
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
                                else if ( move.IsPromotion())
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