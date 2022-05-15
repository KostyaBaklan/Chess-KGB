using System;
using System.Runtime.CompilerServices;
using CommonServiceLocator;
using Engine.DataStructures;
using Engine.DataStructures.Moves;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;

namespace Engine.Sorting.Sorters
{
    public class InitialSorter : MoveSorter
    {
        protected readonly PositionsList PositionsList;
        protected readonly AttackList AttackList;
        protected InitialMoveCollection InitialMoveCollection;

        protected readonly IMoveProvider MoveProvider = ServiceLocator.Current.GetInstance<IMoveProvider>();

        public InitialSorter(IPosition position, IMoveComparer comparer) : base(position, comparer)
        {
            PositionsList = new PositionsList();
            AttackList = new AttackList();

            //InitialMoveCollection = new InitialMoveCollection(new HistoryDescendingComparer()));
            InitialMoveCollection = new InitialMoveCollection(comparer);
            Comparer = comparer;
        }

        #region Overrides of MoveSorter

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override MoveBase[] OrderInternal(AttackList attacks, MoveList moves)
        {
            OrderAttacks(InitialMoveCollection, attacks);

            for (var index = 0; index < moves.Count; index++)
            {
                var move = moves[index];
                if (CurrentKillers.Contains(move.Key))
                {
                    InitialMoveCollection.AddKillerMove(move);
                }
                else if (move.IsCastle || move.IsPromotion)
                {
                    InitialMoveCollection.AddSuggested(move);
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
                    if (CurrentKillers.Contains(move.Key))
                    {
                        InitialMoveCollection.AddKillerMove(move);
                    }
                    else if (move.IsCastle || move.IsPromotion)
                    {
                        InitialMoveCollection.AddSuggested(move);
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
                        if (CurrentKillers.Contains(move.Key))
                        {
                            InitialMoveCollection.AddKillerMove(move);
                        }
                        else if (move.IsCastle || move.IsPromotion)
                        {
                            InitialMoveCollection.AddSuggested(move);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ProcessMove(MoveBase move)
        {
            var phase = Position.GetPhase();
            var board = Position.GetBoard();
            switch (move.Piece)
            {
                case Piece.WhitePawn:
                    if ((move.To.AsBitBoard() & board.GetRank(6)).Any())
                    {
                        InitialMoveCollection.AddSuggested(move);
                        return;
                    }
                    break;
                case Piece.WhiteKnight:
                case Piece.WhiteBishop:
                case Piece.BlackKnight:
                case Piece.BlackBishop:
                    if (phase == Phase.Opening && (move.To.AsBitBoard() & Board.GetPerimeter()).Any())
                    {
                        InitialMoveCollection.AddNonSuggested(move);
                        return;
                    }
                    break;
                case Piece.WhiteRook:
                    if (move.From == Squares.A1 && MoveHistoryService.CanDoWhiteBigCastle()|| move.From == Squares.H1 && MoveHistoryService.CanDoWhiteSmallCastle())
                    {
                        if (phase == Phase.Opening)
                        {
                            InitialMoveCollection.AddBad(move);
                        }
                        else
                        {
                            InitialMoveCollection.AddNonSuggested(move);
                        }
                        return;
                    }
                    break;
                case Piece.WhiteQueen:
                case Piece.BlackQueen:
                    if (phase == Phase.Opening)
                    {
                        InitialMoveCollection.AddNonSuggested(move);return;
                    }
                    break;
                case Piece.WhiteKing:
                    if (!MoveHistoryService.GetLastMove().IsCheck)
                    {
                        bool canCastle = MoveHistoryService.CanDoWhiteCastle();
                        if (phase == Phase.Opening)
                        {
                            if (canCastle)
                            {
                                InitialMoveCollection.AddBad(move); 
                            }
                            else
                            {
                                InitialMoveCollection.AddNonSuggested(move);
                            }
                        }
                        else if(phase == Phase.Middle && canCastle)
                        {
                            InitialMoveCollection.AddNonSuggested(move);
                        }
                    }
                    break;
                case Piece.BlackPawn:
                    if ((move.To.AsBitBoard() & board.GetRank(1)).Any())
                    {
                        InitialMoveCollection.AddSuggested(move);
                        return;
                    }
                    break;
                case Piece.BlackRook:
                    if (move.From == Squares.A8 && MoveHistoryService.CanDoBlackBigCastle()|| move.From == Squares.H8 && MoveHistoryService.CanDoBlackSmallCastle())
                    {
                        if (phase == Phase.Opening)
                        {
                            InitialMoveCollection.AddBad(move); 
                        }
                        else
                        {
                            InitialMoveCollection.AddNonSuggested(move);
                        }
                        return;
                    }
                    break;
                case Piece.BlackKing:
                    if (!MoveHistoryService.GetLastMove().IsCheck)
                    {
                        bool canCastle = MoveHistoryService.CanDoBlackCastle();
                        if (phase == Phase.Opening)
                        {
                            if (canCastle)
                            {
                                InitialMoveCollection.AddBad(move);
                            }
                            else
                            {
                                InitialMoveCollection.AddNonSuggested(move);
                            }
                        }
                        else if (phase == Phase.Middle && canCastle)
                        {
                            InitialMoveCollection.AddNonSuggested(move);
                        }
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Position.Do(move);
            try
            {
                if (move.Piece.IsWhite())
                {
                    if (IsBadWhiteSee(move))
                    {
                        return;
                    }

                    if (IsCheckToBlack(move))
                    {
                        InitialMoveCollection.AddSuggested(move);
                    }
                    else
                    {
                        InitialMoveCollection.AddNonCapture(move);
                    }
                }
                else
                {
                    if (IsBadBlackSee(move))
                    {
                        return;
                    }

                    if (IsCheckToWhite(move))
                    {
                        InitialMoveCollection.AddSuggested(move);
                    }
                    else
                    {
                        InitialMoveCollection.AddNonCapture(move);
                    }
                }
            }
            finally
            {
                Position.UnDo(move);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool IsCheckToWhite(MoveBase move)
        {
            var whiteKingPosition = Board.GetWhiteKingPosition();
            return MoveProvider.AnyLegalAttacksTo(move.Piece, move.To, whiteKingPosition.AsByte());
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
            return MoveProvider.AnyLegalAttacksTo(move.Piece, move.To, blackKingPosition.AsByte());
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

                    if (piece == Piece.BlackQueen || piece == Piece.WhiteQueen)
                    {
                        InitialMoveCollection.AddBad(move); 
                    }
                    else
                    {
                        InitialMoveCollection.AddNonSuggested(move);
                    }
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}