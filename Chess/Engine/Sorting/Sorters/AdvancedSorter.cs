using System.Runtime.CompilerServices;
using CommonServiceLocator;
using Engine.DataStructures;
using Engine.DataStructures.Moves;
using Engine.DataStructures.Moves.Collections;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;

namespace Engine.Sorting.Sorters
{
    public class AdvancedSorter : MoveSorter
    {
        protected readonly PositionsList PositionsList;
        protected readonly AttackList AttackList;

        protected AdvancedMoveCollection AdvancedMoveCollection;
        protected readonly IMoveProvider MoveProvider = ServiceLocator.Current.GetInstance<IMoveProvider>();

        public AdvancedSorter(IPosition position, IMoveComparer comparer) : base(position, comparer)
        {
            PositionsList = new PositionsList();
            AttackList = new AttackList();

            AdvancedMoveCollection = new AdvancedMoveCollection(comparer);
            Comparer = comparer;
        }

        #region Overrides of MoveSorter

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override MoveBase[] OrderInternal(AttackList attacks, MoveList moves)
        {
            OrderAttacks(AdvancedMoveCollection, attacks);

            for (var index = 0; index < moves.Count; index++)
            {
                var move = moves[index];
                if (CurrentKillers.Contains(move.Key))
                {
                    AdvancedMoveCollection.AddKillerMove(move);
                }
                else if (move.IsCastle || move.IsPromotion)
                {
                    AdvancedMoveCollection.AddSuggested(move);
                }
                else
                {
                    ProcessMove(move);
                }
            }

            return AdvancedMoveCollection.Build();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override MoveBase[] OrderInternal(AttackList attacks, MoveList moves,
            MoveBase pvNode)
        {
            if (pvNode is AttackBase attack)
            {
                OrderAttacks(AdvancedMoveCollection, attacks, attack);

                for (var index = 0; index < moves.Count; index++)
                {
                    var move = moves[index];
                    if (CurrentKillers.Contains(move.Key))
                    {
                        AdvancedMoveCollection.AddKillerMove(move);
                    }
                    else if (move.IsCastle || move.IsPromotion)
                    {
                        AdvancedMoveCollection.AddSuggested(move);
                    }
                    else
                    {
                        ProcessMove(move);
                    }
                }
            }
            else
            {
                OrderAttacks(AdvancedMoveCollection, attacks);

                for (var index = 0; index < moves.Count; index++)
                {
                    var move = moves[index];
                    if (move.Key == pvNode.Key)
                    {
                        AdvancedMoveCollection.AddHashMove(move);
                    }
                    else
                    {
                        if (CurrentKillers.Contains(move.Key))
                        {
                            AdvancedMoveCollection.AddKillerMove(move);
                        }
                        else if (move.IsCastle || move.IsPromotion)
                        {
                            AdvancedMoveCollection.AddSuggested(move);
                        }
                        else
                        {
                            ProcessMove(move);
                        }
                    }
                }
            }

            return AdvancedMoveCollection.Build();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void ProcessMove(MoveBase move)
        {
            var phase = Position.GetPhase();
            if (move.Piece.IsWhite())
            {
                if (move.Piece == Piece.WhiteKing && !MoveHistoryService.GetLastMove().IsCheck)
                {
                    if (phase!=Phase.End)
                    {
                        AdvancedMoveCollection.AddNonSuggested(move);
                        return; 
                    }
                }

                if (phase == Phase.Opening)
                {
                    switch (move.Piece)
                    {
                        case Piece.WhiteKnight:
                        case Piece.WhiteBishop:
                        {
                            var bit = move.To.AsBitBoard();
                            if ((bit & Board.GetPerimeter()).Any())
                            {
                                AdvancedMoveCollection.AddNonSuggested(move);
                                return;
                            }

                            break;

                        }
                        case Piece.WhiteRook:
                        {
                            if (move.From == Squares.A1 && MoveHistoryService.CanDoWhiteBigCastle())
                            {
                                AdvancedMoveCollection.AddNonSuggested(move);
                                return;
                            }

                            if (move.From == Squares.H1 && MoveHistoryService.CanDoWhiteSmallCastle())
                            {
                                AdvancedMoveCollection.AddNonSuggested(move);
                                return;
                            }

                            break;
                        }
                        case Piece.WhiteQueen:
                        {
                            AdvancedMoveCollection.AddNonSuggested(move);
                            return;
                        }
                    }

                    AdvancedMoveCollection.AddNonCapture(move);
                }
                else
                {
                    Position.Do(move);
                    try
                    {
                        if (IsBadWhiteSee(move))
                        {
                            return;
                        }

                        if (IsCheckToBlack(move))
                        {
                            AdvancedMoveCollection.AddSuggested(move);
                        }
                        else
                        {
                            AdvancedMoveCollection.AddNonCapture(move);
                        }
                    }
                    finally
                    {
                        Position.UnDo(move);
                    }
                }

            }
            else
            {
                if (move.Piece == Piece.BlackKing && !MoveHistoryService.GetLastMove().IsCheck)
                {
                    if (phase!=Phase.End)
                    {
                        AdvancedMoveCollection.AddNonSuggested(move);
                        return; 
                    }
                }

                if (phase == Phase.Opening)
                {
                    switch (move.Piece)
                    {
                        case Piece.BlackKnight:
                        case Piece.BlackBishop:
                        {
                            var bit = move.To.AsBitBoard();
                            if ((bit & Board.GetPerimeter()).Any())
                            {
                                AdvancedMoveCollection.AddNonSuggested(move);
                                return;
                            }

                            break;
                        }
                        case Piece.BlackRook:
                            if (move.From == Squares.A8 && MoveHistoryService.CanDoBlackBigCastle())
                            {
                                AdvancedMoveCollection.AddNonSuggested(move);
                                return;
                            }

                            if (move.From == Squares.H8 && MoveHistoryService.CanDoBlackSmallCastle())
                            {
                                AdvancedMoveCollection.AddNonSuggested(move);
                                return;
                            }

                            break;
                        case Piece.BlackQueen:
                            AdvancedMoveCollection.AddNonSuggested(move);
                            return;
                    }

                    AdvancedMoveCollection.AddNonCapture(move);
                }
                else
                {
                    Position.Do(move);
                    try
                    {
                        if (IsBadBlackSee(move))
                        {
                            return;
                        }

                        if (IsCheckToWhite(move))
                        {
                            AdvancedMoveCollection.AddSuggested(move);
                        }
                        else
                        {
                            AdvancedMoveCollection.AddNonCapture(move);
                        }
                    }
                    finally
                    {
                        Position.UnDo(move);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool IsCheckToWhite(MoveBase move)
        {
            var whiteKingPosition = Board.GetWhiteKingPosition();
            return MoveProvider.AnyLegalAttacksTo(move.Piece, move.To, whiteKingPosition);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool IsBadBlackSee(MoveBase move)
        {
            var attackTo = MoveProvider.GetWhitePawnAttackTo(Board, move.To.AsByte());
            if (attackTo.Any() && IsBadSee(move, attackTo, Piece.WhitePawn)) return true;

            attackTo = MoveProvider.GetWhiteKnightAttackTo(Board, move.To.AsByte());
            if (attackTo.Any() && IsBadSee(move, attackTo, Piece.WhiteKnight)) return true;

            attackTo = MoveProvider.GetWhiteBishopAttackTo(Board, move.To.AsByte());
            if (attackTo.Any() && IsBadSee(move, attackTo, Piece.WhiteBishop)) return true;

            attackTo = MoveProvider.GetWhiteRookAttackTo(Board, move.To.AsByte());
            if (attackTo.Any() && IsBadSee(move, attackTo, Piece.WhiteRook)) return true;

            attackTo = MoveProvider.GetWhiteQueenAttackTo(Board, move.To.AsByte());
            if (attackTo.Any() && IsBadSee(move, attackTo, Piece.WhiteQueen)) return true;

            attackTo = MoveProvider.GetWhiteKingAttackTo(Board, move.To.AsByte());
            if (attackTo.Any() && IsBadSee(move, attackTo, Piece.WhiteKing)) return true;

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool IsCheckToBlack(MoveBase move)
        {
            var blackKingPosition = Board.GetBlackKingPosition();
            return MoveProvider.AnyLegalAttacksTo(move.Piece, move.To, blackKingPosition);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool IsBadWhiteSee(MoveBase move)
        {
            var attackTo = MoveProvider.GetBlackPawnAttackTo(Board, move.To.AsByte());
            if (attackTo.Any() && IsBadSee(move, attackTo, Piece.BlackPawn)) return true;

            attackTo = MoveProvider.GetBlackKnightAttackTo(Board, move.To.AsByte());
            if (attackTo.Any() && IsBadSee(move, attackTo, Piece.BlackKnight)) return true;

            attackTo = MoveProvider.GetBlackBishopAttackTo(Board, move.To.AsByte());
            if (attackTo.Any() && IsBadSee(move, attackTo, Piece.BlackBishop)) return true;

            attackTo = MoveProvider.GetBlackRookAttackTo(Board, move.To.AsByte());
            if (attackTo.Any() && IsBadSee(move, attackTo, Piece.BlackRook)) return true;

            attackTo = MoveProvider.GetBlackQueenAttackTo(Board, move.To.AsByte());
            if (attackTo.Any() && IsBadSee(move, attackTo, Piece.BlackQueen)) return true;

            attackTo = MoveProvider.GetBlackKingAttackTo(Board, move.To.AsByte());
            if (attackTo.Any() && IsBadSee(move, attackTo, Piece.BlackKing)) return true;

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool IsBadSee(MoveBase move, BitBoard attackTo,
            Piece piece)
        {
            attackTo.GetPositions(PositionsList);
            for (var i = 0; i < PositionsList.Count; i++)
            {
                MoveProvider.GetAttacks(piece, PositionsList[i], AttackList);
                for (var x = 0; x < AttackList.Count; x++)
                {
                    AttackList[x].Captured = move.Piece;
                    if (Board.StaticExchange(AttackList[x]) <= 0) continue;

                    AdvancedMoveCollection.AddBadMove(move);
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}