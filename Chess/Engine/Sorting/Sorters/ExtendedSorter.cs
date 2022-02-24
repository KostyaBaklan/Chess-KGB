﻿using System.Runtime.CompilerServices;
using Engine.DataStructures.Moves;
using Engine.Interfaces;
using Engine.Models.Enums;
using Engine.Models.Moves;
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
        protected override MoveBase[] OrderInternal(AttackList attacks, MoveList moves)
        {
            var sortedAttacks = OrderAttacks(attacks);

            OrderAttacks(ExtendedMoveCollection, sortedAttacks);

            ProcessMoves(moves);

            return ExtendedMoveCollection.Build();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override MoveBase[] OrderInternal(AttackList attacks, MoveList moves,
            MoveBase pvNode)
        {
            var sortedAttacks = OrderAttacks(attacks);

            if (pvNode is AttackBase attack)
            {
                OrderAttacks(ExtendedMoveCollection, sortedAttacks, attack);

                ProcessMoves(moves);
            }
            else
            {
                OrderAttacks(ExtendedMoveCollection, sortedAttacks);

                ProcessMoves(moves, pvNode);
            }

            return ExtendedMoveCollection.Build();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessMoves(MoveList moves)
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
                            if (CurrentKillers.Contains(move.Key))
                            {
                                ExtendedMoveCollection.AddKillerMove(move);
                            }
                            else if (move.IsCastle || move.IsPromotion)
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
                            if (CurrentKillers.Contains(move.Key))
                            {
                                ExtendedMoveCollection.AddKillerMove(move);
                            }
                            else if (move.IsCastle)
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
                            if (CurrentKillers.Contains(move.Key))
                            {
                                ExtendedMoveCollection.AddKillerMove(move);
                            }
                            else if (move.IsPromotion)
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
                            if (CurrentKillers.Contains(move.Key))
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
                            if (CurrentKillers.Contains(move.Key))
                            {
                                ExtendedMoveCollection.AddKillerMove(move);
                            }
                            else if (move.IsCastle || move.IsPromotion)
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
                            if (CurrentKillers.Contains(move.Key))
                            {
                                ExtendedMoveCollection.AddKillerMove(move);
                            }
                            else if (move.IsCastle)
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
                            if (CurrentKillers.Contains(move.Key))
                            {
                                ExtendedMoveCollection.AddKillerMove(move);
                            }
                            else if (move.IsPromotion)
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
                            if (CurrentKillers.Contains(move.Key))
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
        private void ProcessMoves(MoveList moves,  MoveBase pvNode)
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
                            if (move.Key == pvNode.Key)
                            {
                                ExtendedMoveCollection.AddHashMove(move);
                            }
                            else
                            {
                                if (CurrentKillers.Contains(move.Key))
                                {
                                    ExtendedMoveCollection.AddKillerMove(move);
                                }
                                else if (move.IsCastle || move.IsPromotion)
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
                            if (move.Key == pvNode.Key)
                            {
                                ExtendedMoveCollection.AddHashMove(move);
                            }
                            else
                            {
                                if (CurrentKillers.Contains(move.Key))
                                {
                                    ExtendedMoveCollection.AddKillerMove(move);
                                }
                                else if (move.IsCastle)
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
                            if (move.Key == pvNode.Key)
                            {
                                ExtendedMoveCollection.AddHashMove(move);
                            }
                            else
                            {
                                if (CurrentKillers.Contains(move.Key))
                                {
                                    ExtendedMoveCollection.AddKillerMove(move);
                                }
                                else if (move.IsPromotion)
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
                            if (move.Key == pvNode.Key)
                            {
                                ExtendedMoveCollection.AddHashMove(move);
                            }
                            else
                            {
                                if (CurrentKillers.Contains(move.Key))
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
                            if (move.Key == pvNode.Key)
                            {
                                ExtendedMoveCollection.AddHashMove(move);
                            }
                            else
                            {
                                if (CurrentKillers.Contains(move.Key))
                                {
                                    ExtendedMoveCollection.AddKillerMove(move);
                                }
                                else if (move.IsCastle || move.IsPromotion)
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
                            if (move.Key == pvNode.Key)
                            {
                                ExtendedMoveCollection.AddHashMove(move);
                            }
                            else
                            {
                                if (CurrentKillers.Contains(move.Key))
                                {
                                    ExtendedMoveCollection.AddKillerMove(move);
                                }
                                else if (move.IsCastle)
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
                            if (move.Key == pvNode.Key)
                            {
                                ExtendedMoveCollection.AddHashMove(move);
                            }
                            else
                            {
                                if (CurrentKillers.Contains(move.Key))
                                {
                                    ExtendedMoveCollection.AddKillerMove(move);
                                }
                                else if (move.IsPromotion)
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
                            if (move.Key == pvNode.Key)
                            {
                                ExtendedMoveCollection.AddHashMove(move);
                            }
                            else
                            {
                                if (CurrentKillers.Contains(move.Key))
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