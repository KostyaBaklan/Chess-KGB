using System.Runtime.CompilerServices;
using Engine.DataStructures.Moves;
using Engine.DataStructures.Moves.Collections.Initial;
using Engine.Interfaces;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;

namespace Engine.Sorting.Sorters.Initial
{
    public class InitialSuggestedSorter : InitialSorter
    {
        public InitialSuggestedSorter(IPosition position, IMoveComparer comparer) : base(position, comparer)
        {
            InitialMoveCollection = new InitialSuggestedMoveCollection(comparer);
        }

        #region Overrides of MoveSorter

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override MoveBase[] OrderInternal(AttackList attacks, MoveList moves)
        {
            OrderAttacks(InitialMoveCollection, attacks);

            for (var index = 0; index < moves.Count; index++)
            {
                var move = moves[index];
                if (move.IsPromotion)
                {
                    ProcessPromotion(move);
                }
                else if (move.IsCastle)
                {
                    InitialMoveCollection.AddSuggested(move);
                }
                else if (CurrentKillers.Contains(move.Key))
                {
                    InitialMoveCollection.AddKillerMove(move);
                }
                else
                {
                    ProcessMove(move);
                }
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

                for (var index = 0; index < moves.Count; index++)
                {
                    var move = moves[index];
                    if (move.IsPromotion)
                    {
                        ProcessPromotion(move);
                    }
                    else if (move.IsCastle)
                    {
                        InitialMoveCollection.AddSuggested(move);
                    }
                    else if (CurrentKillers.Contains(move.Key))
                    {
                        InitialMoveCollection.AddKillerMove(move);
                    }
                    else
                    {
                        ProcessMove(move);
                    }
                }
            }
            else
            {
                OrderAttacks(InitialMoveCollection, attacks);

                for (var index = 0; index < moves.Count; index++)
                {
                    var move = moves[index];
                    if (move.Key == pvNode.Key)
                    {
                        InitialMoveCollection.AddHashMove(move);
                    }
                    else
                    {
                        if (move.IsPromotion)
                        {
                            ProcessPromotion(move);
                        }
                        else if (move.IsCastle)
                        {
                            InitialMoveCollection.AddSuggested(move);
                        }
                        else if (CurrentKillers.Contains(move.Key))
                        {
                            InitialMoveCollection.AddKillerMove(move);
                        }
                        else
                        {
                            ProcessMove(move);
                        }
                    }
                }
            }

            return InitialMoveCollection.Build();
        }

        #endregion
    }
}