using System.Runtime.CompilerServices;
using Engine.DataStructures.Moves;
using Engine.Interfaces;
using Engine.Models.Enums;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;

namespace Engine.Sorting.Sorters
{
    public class BasicSorter : MoveSorter
    {
        public BasicSorter(IPosition position, IMoveComparer comparer) : base(position,comparer)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override MoveBase[] OrderInternal(AttackList attacks, MoveList moves)
        {
            var sortedAttacks = OrderAttacks(attacks);

            OrderAttacks(MoveCollection, sortedAttacks);

            ProcessMoves(moves);

            return MoveCollection.Build();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override MoveBase[] OrderInternal(AttackList attacks, MoveList moves,
            MoveBase pvNode)
        {
            var sortedAttacks = OrderAttacks(attacks);

            if (pvNode is AttackBase attack)
            {
                OrderAttacks(MoveCollection, sortedAttacks, attack);

                ProcessMoves(moves);
            }
            else
            {
                OrderAttacks(MoveCollection, sortedAttacks);

                ProcessMoves(moves, pvNode);
            }

            return MoveCollection.Build();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessMoves(MoveList moves, MoveBase pvNode)
        {
            if (Position.GetTurn() == Turn.White)
            {
                if (Position.CanWhitePromote())
                {
                    for (var index = 0; index < moves.Count; index++)
                    {
                        var move = moves[index];
                        if (move.Key == pvNode.Key)
                        {
                            MoveCollection.AddHashMove(move);
                        }
                        else if (CurrentKillers.Contains(move.Key) || move.IsPromotion)
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
                    for (var index = 0; index < moves.Count; index++)
                    {
                        var move = moves[index];
                        if (move.Key == pvNode.Key)
                        {
                            MoveCollection.AddHashMove(move);
                        }
                        else if (CurrentKillers.Contains(move.Key))
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
                    for (var index = 0; index < moves.Count; index++)
                    {
                        var move = moves[index];
                        if (move.Key == pvNode.Key)
                        {
                            MoveCollection.AddHashMove(move);
                        }
                        else if (CurrentKillers.Contains(move.Key) || move.IsPromotion)
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
                    for (var index = 0; index < moves.Count; index++)
                    {
                        var move = moves[index];
                        if (move.Key == pvNode.Key)
                        {
                            MoveCollection.AddHashMove(move);
                        }
                        else if (CurrentKillers.Contains(move.Key))
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
        private void ProcessMoves(MoveList moves)
        {
            if (Position.GetTurn() == Turn.White)
            {
                if (Position.CanWhitePromote())
                {
                    for (var index = 0; index < moves.Count; index++)
                    {
                        var move = moves[index];
                        if (CurrentKillers.Contains(move.Key) || move.IsPromotion)
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
                    for (var index = 0; index < moves.Count; index++)
                    {
                        var move = moves[index];
                        if (CurrentKillers.Contains(move.Key))
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
                    for (var index = 0; index < moves.Count; index++)
                    {
                        var move = moves[index];
                        if (CurrentKillers.Contains(move.Key) || move.IsPromotion)
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
                    for (var index = 0; index < moves.Count; index++)
                    {
                        var move = moves[index];
                        if (CurrentKillers.Contains(move.Key))
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