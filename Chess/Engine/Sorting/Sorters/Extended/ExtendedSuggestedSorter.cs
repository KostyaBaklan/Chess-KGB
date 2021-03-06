using System.Runtime.CompilerServices;
using Engine.DataStructures.Moves;
using Engine.DataStructures.Moves.Collections.Extended;
using Engine.Interfaces;
using Engine.Models.Enums;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;

namespace Engine.Sorting.Sorters.Extended
{
    public class ExtendedSuggestedSorter : ExtendedSorter
    {
        public ExtendedSuggestedSorter(IPosition position, IMoveComparer comparer) : base(position, comparer)
        {
            ExtendedMoveCollection = new ExtendedSuggestedMoveCollection(comparer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void ProcessMoves(MoveList moves)
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
                            if (move.IsCastle|| move.IsPromotion)
                            {
                                ExtendedMoveCollection.AddSuggested(move);
                            }
                            else if (CurrentKillers.Contains(move.Key))
                            {
                                ExtendedMoveCollection.AddKillerMove(move);
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
                            if (move.IsCastle)
                            {
                                ExtendedMoveCollection.AddSuggested(move);
                            }
                            else if (CurrentKillers.Contains(move.Key))
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
                else
                {
                    if (Position.CanWhitePromote())
                    {
                        for (var index = 0; index < moves.Count; index++)
                        {
                            var move = moves[index];
                            if (move.IsPromotion)
                            {
                                ExtendedMoveCollection.AddSuggested(move);
                            }
                            else if (CurrentKillers.Contains(move.Key))
                            {
                                ExtendedMoveCollection.AddKillerMove(move);
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
                            if (move.IsCastle|| move.IsPromotion)
                            {
                                ExtendedMoveCollection.AddSuggested(move);
                            }
                            else if (CurrentKillers.Contains(move.Key))
                            {
                                ExtendedMoveCollection.AddKillerMove(move);
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
                            if (move.IsCastle)
                            {
                                ExtendedMoveCollection.AddSuggested(move);
                            }
                            else if (CurrentKillers.Contains(move.Key))
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
                else
                {
                    if (Position.CanBlackPromote())
                    {
                        for (var index = 0; index < moves.Count; index++)
                        {
                            var move = moves[index];
                            if (move.IsPromotion)
                            {
                                ExtendedMoveCollection.AddSuggested(move);
                            }
                            else if (CurrentKillers.Contains(move.Key))
                            {
                                ExtendedMoveCollection.AddKillerMove(move);
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
        protected override void ProcessMoves(MoveList moves, MoveBase pvNode)
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
                                if (move.IsCastle|| move.IsPromotion)
                                {
                                    ExtendedMoveCollection.AddSuggested(move);
                                }
                                else if (CurrentKillers.Contains(move.Key))
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
                                if (move.IsCastle)
                                {
                                    ExtendedMoveCollection.AddSuggested(move);
                                }
                                else if (CurrentKillers.Contains(move.Key))
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
                                if (move.IsPromotion)
                                {
                                    ExtendedMoveCollection.AddSuggested(move);
                                }
                                else if (CurrentKillers.Contains(move.Key))
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
                                if (move.IsCastle|| move.IsPromotion)
                                {
                                    ExtendedMoveCollection.AddSuggested(move);
                                }
                                else if (CurrentKillers.Contains(move.Key))
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
                                if (move.IsCastle)
                                {
                                    ExtendedMoveCollection.AddSuggested(move);
                                }
                                else if (CurrentKillers.Contains(move.Key))
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
                                if (move.IsPromotion)
                                {
                                    ExtendedMoveCollection.AddSuggested(move);
                                }
                                else if (CurrentKillers.Contains(move.Key))
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