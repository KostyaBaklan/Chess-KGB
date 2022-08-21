using System.Runtime.CompilerServices;
using Engine.DataStructures.Moves;
using Engine.Interfaces;
using Engine.Models.Enums;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;

namespace OpeningTool
{
    internal class OpeningSorter : MoveSorter
    {
        protected OpeningMoveCollection ExtendedMoveCollection;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override MoveBase[] OrderInternal(AttackList attacks, MoveList moves)
        {
            OrderAttacks(ExtendedMoveCollection, attacks);

            ProcessMoves(moves);

            return ExtendedMoveCollection.Build();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override MoveBase[] OrderInternal(AttackList attacks, MoveList moves,
            MoveBase pvNode)
        {
            if (pvNode is AttackBase attack)
            {
                OrderAttacks(ExtendedMoveCollection, attacks, attack.Key);

                ProcessMoves(moves);
            }
            else
            {
                OrderAttacks(ExtendedMoveCollection, attacks);

                ProcessMoves(moves, pvNode.Key);
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
        private void ProcessMoves(MoveList moves, short pvNode)
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
                            if (move.Key == pvNode)
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
                            if (move.Key == pvNode)
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
                            if (move.Key == pvNode)
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
                            if (move.Key == pvNode)
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
                            if (move.Key == pvNode)
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
                            if (move.Key == pvNode)
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
                            if (move.Key == pvNode)
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
                            if (move.Key == pvNode)
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

        public OpeningSorter(IPosition position, IMoveComparer comparer) : base(position, comparer)
        {
            ExtendedMoveCollection = new OpeningMoveCollection(comparer);
        }
    }
}