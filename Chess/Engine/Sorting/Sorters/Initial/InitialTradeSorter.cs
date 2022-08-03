using System;
using System.Runtime.CompilerServices;
using Engine.DataStructures.Moves;
using Engine.DataStructures.Moves.Collections.Initial;
using Engine.Interfaces;
using Engine.Models.Enums;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;

namespace Engine.Sorting.Sorters.Initial
{
    public class InitialTradeSorter : InitialSorter
    {
        public InitialTradeSorter(IPosition position, IMoveComparer comparer) : base(position, comparer)
        {
            InitialMoveCollection = new InitialTradeMoveCollection(comparer);
        }

        #region Overrides of MoveSorter

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override MoveBase[] OrderInternal(AttackList attacks, MoveList moves)
        {
            OrderAttacks(InitialMoveCollection, attacks);

            if (Position.GetTurn() == Turn.White)
            {
                ProcessWhiteMoves(moves);
            }
            else
            {
                ProcessBlackMoves(moves);
            }

            return InitialMoveCollection.Build();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override MoveBase[] OrderInternal(AttackList attacks, MoveList moves,
            MoveBase pvNode)
        {
            if (pvNode is AttackBase attack)
            {
                OrderAttacks(InitialMoveCollection, attacks, attack);

                if (Position.GetTurn() == Turn.White)
                {
                    ProcessWhiteMoves(moves);
                }
                else
                {
                    ProcessBlackMoves(moves);
                }
            }
            else
            {
                OrderAttacks(InitialMoveCollection, attacks);

                if (Position.GetTurn() == Turn.White)
                {
                    ProcessWhiteMoves(moves, pvNode.Key);
                }
                else
                {
                    ProcessBlackMoves(moves, pvNode.Key);
                }
            }

            return InitialMoveCollection.Build();
        }

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessWhiteMoves(MoveList moves)
        {
            if (Position.GetPhase() == Phase.Opening)
            {
                for (var index = 0; index < moves.Count; index++)
                {
                    var move = moves[index];
                    if (move.IsPromotion)
                    {
                        ProcessPromotion(move);
                    }
                    else if (CurrentKillers.Contains(move.Key))
                    {
                        InitialMoveCollection.AddKillerMove(move);
                    }
                    else if (move.IsCastle)
                    {
                        InitialMoveCollection.AddSuggested(move);
                    }
                    else
                    {
                        ProcessWhiteOpeningMove(move);
                    }
                }
            }
            else if (Position.GetPhase() == Phase.Middle)
            {
                for (var index = 0; index < moves.Count; index++)
                {
                    var move = moves[index];
                    if (move.IsPromotion)
                    {
                        ProcessPromotion(move);
                    }
                    else if (CurrentKillers.Contains(move.Key))
                    {
                        InitialMoveCollection.AddKillerMove(move);
                    }
                    else if (move.IsCastle)
                    {
                        InitialMoveCollection.AddSuggested(move);
                    }
                    else
                    {
                        ProcessWhiteMiddleMove(move);
                    }
                }
            }
            else
            {
                for (var index = 0; index < moves.Count; index++)
                {
                    var move = moves[index];
                    if (move.IsPromotion)
                    {
                        ProcessPromotion(move);
                    }
                    else if (CurrentKillers.Contains(move.Key))
                    {
                        InitialMoveCollection.AddKillerMove(move);
                    }
                    else if (move.IsCastle)
                    {
                        InitialMoveCollection.AddSuggested(move);
                    }
                    else
                    {
                        ProcessWhiteEndMove(move);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessBlackMoves(MoveList moves)
        {
            if (Position.GetPhase() == Phase.Opening)
            {
                for (var index = 0; index < moves.Count; index++)
                {
                    var move = moves[index];
                    if (move.IsPromotion)
                    {
                        ProcessPromotion(move);
                    }
                    else if (CurrentKillers.Contains(move.Key))
                    {
                        InitialMoveCollection.AddKillerMove(move);
                    }
                    else if (move.IsCastle)
                    {
                        InitialMoveCollection.AddSuggested(move);
                    }
                    else
                    {
                        ProcessBlackOpeningMove(move);
                    }
                }
            }
            else if (Position.GetPhase() == Phase.Middle)
            {
                for (var index = 0; index < moves.Count; index++)
                {
                    var move = moves[index];
                    if (move.IsPromotion)
                    {
                        ProcessPromotion(move);
                    }
                    else if (CurrentKillers.Contains(move.Key))
                    {
                        InitialMoveCollection.AddKillerMove(move);
                    }
                    else if (move.IsCastle)
                    {
                        InitialMoveCollection.AddSuggested(move);
                    }
                    else
                    {
                        ProcessBlackMiddleMove(move);
                    }
                }
            }
            else
            {
                for (var index = 0; index < moves.Count; index++)
                {
                    var move = moves[index];
                    if (move.IsPromotion)
                    {
                        ProcessPromotion(move);
                    }
                    else if (CurrentKillers.Contains(move.Key))
                    {
                        InitialMoveCollection.AddKillerMove(move);
                    }
                    else if (move.IsCastle)
                    {
                        InitialMoveCollection.AddSuggested(move);
                    }
                    else
                    {
                        ProcessBlackEndMove(move);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessWhiteMoves(MoveList moves, short pvNodeKey)
        {
            if (Position.GetPhase() == Phase.Opening)
            {
                for (var index = 0; index < moves.Count; index++)
                {
                    var move = moves[index];
                    if (move.Key == pvNodeKey)
                    {
                        InitialMoveCollection.AddHashMove(move);
                    }
                    else
                    {
                        if (move.IsPromotion)
                        {
                            ProcessPromotion(move);
                        }
                        else if (CurrentKillers.Contains(move.Key))
                        {
                            InitialMoveCollection.AddKillerMove(move);
                        }
                        else if (move.IsCastle)
                        {
                            InitialMoveCollection.AddSuggested(move);
                        }
                        else
                        {
                            ProcessWhiteOpeningMove(move);
                        }
                    }
                }
            }
            else if (Position.GetPhase() == Phase.Middle)
            {
                for (var index = 0; index < moves.Count; index++)
                {
                    var move = moves[index];
                    if (move.Key == pvNodeKey)
                    {
                        InitialMoveCollection.AddHashMove(move);
                    }
                    else
                    {
                        if (move.IsPromotion)
                        {
                            ProcessPromotion(move);
                        }
                        else if (CurrentKillers.Contains(move.Key))
                        {
                            InitialMoveCollection.AddKillerMove(move);
                        }
                        else if (move.IsCastle)
                        {
                            InitialMoveCollection.AddSuggested(move);
                        }
                        else
                        {
                            ProcessWhiteMiddleMove(move);
                        }
                    }
                }
            }
            else
            {
                for (var index = 0; index < moves.Count; index++)
                {
                    var move = moves[index];
                    if (move.Key == pvNodeKey)
                    {
                        InitialMoveCollection.AddHashMove(move);
                    }
                    else
                    {
                        if (move.IsPromotion)
                        {
                            ProcessPromotion(move);
                        }
                        else if (CurrentKillers.Contains(move.Key))
                        {
                            InitialMoveCollection.AddKillerMove(move);
                        }
                        else if (move.IsCastle)
                        {
                            InitialMoveCollection.AddSuggested(move);
                        }
                        else
                        {
                            ProcessWhiteEndMove(move);
                        }
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessBlackMoves(MoveList moves, short pvNodeKey)
        {
            if (Position.GetPhase() == Phase.Opening)
            {
                for (var index = 0; index < moves.Count; index++)
                {
                    var move = moves[index];
                    if (move.Key == pvNodeKey)
                    {
                        InitialMoveCollection.AddHashMove(move);
                    }
                    else
                    {
                        if (move.IsPromotion)
                        {
                            ProcessPromotion(move);
                        }
                        else if (CurrentKillers.Contains(move.Key))
                        {
                            InitialMoveCollection.AddKillerMove(move);
                        }
                        else if (move.IsCastle)
                        {
                            InitialMoveCollection.AddSuggested(move);
                        }
                        else
                        {
                            ProcessBlackOpeningMove(move);
                        }
                    }
                }
            }
            else if (Position.GetPhase() == Phase.Middle)
            {
                for (var index = 0; index < moves.Count; index++)
                {
                    var move = moves[index];
                    if (move.Key == pvNodeKey)
                    {
                        InitialMoveCollection.AddHashMove(move);
                    }
                    else
                    {
                        if (move.IsPromotion)
                        {
                            ProcessPromotion(move);
                        }
                        else if (CurrentKillers.Contains(move.Key))
                        {
                            InitialMoveCollection.AddKillerMove(move);
                        }
                        else if (move.IsCastle)
                        {
                            InitialMoveCollection.AddSuggested(move);
                        }
                        else
                        {
                            ProcessBlackMiddleMove(move);
                        }
                    }
                }
            }
            else
            {
                for (var index = 0; index < moves.Count; index++)
                {
                    var move = moves[index];
                    if (move.Key == pvNodeKey)
                    {
                        InitialMoveCollection.AddHashMove(move);
                    }
                    else
                    {
                        if (move.IsPromotion)
                        {
                            ProcessPromotion(move);
                        }
                        else if (CurrentKillers.Contains(move.Key))
                        {
                            InitialMoveCollection.AddKillerMove(move);
                        }
                        else if (move.IsCastle)
                        {
                            InitialMoveCollection.AddSuggested(move);
                        }
                        else
                        {
                            ProcessBlackEndMove(move);
                        }
                    }
                }
            }
        }
    }
}