﻿using System.Runtime.CompilerServices;
using Engine.DataStructures.Moves;
using Engine.Interfaces;
using Engine.Models.Enums;
using Engine.Sorting.Comparers;

namespace Engine.Sorting.Sorters.History
{
    public abstract class HistoryTradeSorterBase : HistorySorter
    {
        protected HistoryTradeSorterBase(IPosition position, IMoveComparer comparer) : base(position, comparer)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void ProcessMoves(MoveList moves)
        {
            if (Position.GetTurn() == Turn.White)
            {
                if (Position.CanWhitePromote())
                {
                    for (var index = 0; index < moves.Count; index++)
                    {
                        var move = moves[index];
                        if (CurrentKillers.Contains(move.Key))
                        {
                            HistoryMoveCollection.AddKillerMove(move);
                        }
                        else if (move.IsPromotion)
                        {
                            ProcessPromotion(move, HistoryMoveCollection);
                        }
                        else
                        {
                            HistoryMoveCollection.AddNonCapture(move);
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
                            HistoryMoveCollection.AddKillerMove(move);
                        }
                        else
                        {
                            HistoryMoveCollection.AddNonCapture(move);
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
                            HistoryMoveCollection.AddKillerMove(move);
                        }
                        else if (move.IsPromotion)
                        {
                            ProcessPromotion(move, HistoryMoveCollection);
                        }
                        else
                        {
                            HistoryMoveCollection.AddNonCapture(move);
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
                            HistoryMoveCollection.AddKillerMove(move);
                        }
                        else
                        {
                            HistoryMoveCollection.AddNonCapture(move);
                        }
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void ProcessMoves(MoveList moves, short pvNode)
        {
            if (Position.GetTurn() == Turn.White)
            {
                if (Position.CanWhitePromote())
                {
                    for (var index = 0; index < moves.Count; index++)
                    {
                        var move = moves[index];
                        if (move.Key == pvNode)
                        {
                            HistoryMoveCollection.AddHashMove(move);
                        }
                        else
                        {
                            if (CurrentKillers.Contains(move.Key))
                            {
                                HistoryMoveCollection.AddKillerMove(move);
                            }
                            else if (move.IsPromotion)
                            {
                                ProcessPromotion(move, HistoryMoveCollection);
                            }
                            else
                            {
                                HistoryMoveCollection.AddNonCapture(move);
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
                            HistoryMoveCollection.AddHashMove(move);
                        }
                        else
                        {
                            if (CurrentKillers.Contains(move.Key))
                            {
                                HistoryMoveCollection.AddKillerMove(move);
                            }
                            else
                            {
                                HistoryMoveCollection.AddNonCapture(move);
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
                            HistoryMoveCollection.AddHashMove(move);
                        }
                        else
                        {
                            if (CurrentKillers.Contains(move.Key))
                            {
                                HistoryMoveCollection.AddKillerMove(move);
                            }
                            else if (move.IsPromotion)
                            {
                                ProcessPromotion(move, HistoryMoveCollection);
                            }
                            else
                            {
                                HistoryMoveCollection.AddNonCapture(move);
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
                            HistoryMoveCollection.AddHashMove(move);
                        }
                        else
                        {
                            if (CurrentKillers.Contains(move.Key))
                            {
                                HistoryMoveCollection.AddKillerMove(move);
                            }
                            else
                            {
                                HistoryMoveCollection.AddNonCapture(move);
                            }
                        }
                    }
                }
            }
        }
    }
}