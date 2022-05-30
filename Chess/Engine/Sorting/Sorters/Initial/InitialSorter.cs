using System;
using System.Runtime.CompilerServices;
using CommonServiceLocator;
using Engine.DataStructures;
using Engine.DataStructures.Moves;
using Engine.DataStructures.Moves.Collections.Initial;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;

namespace Engine.Sorting.Sorters.Initial
{
    public abstract class InitialSorter : MoveSorter
    {
        private readonly BitBoard _minorStartPositions;
        protected readonly PositionsList PositionsList;
        protected readonly AttackList AttackList;
        protected InitialMoveCollection InitialMoveCollection;

        protected readonly IMoveProvider MoveProvider = ServiceLocator.Current.GetInstance<IMoveProvider>();

        protected InitialSorter(IPosition position, IMoveComparer comparer) : base(position, comparer)
        {
            PositionsList = new PositionsList();
            AttackList = new AttackList();
            Comparer = comparer;
            _minorStartPositions = Squares.B1.AsBitBoard() | Squares.C1.AsBitBoard() | Squares.F1.AsBitBoard() |
                                   Squares.G1.AsBitBoard() | Squares.B8.AsBitBoard() | Squares.C8.AsBitBoard() |
                                   Squares.F8.AsBitBoard() | Squares.H8.AsBitBoard();
        }

        #region Overrides of MoveSorter

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ProcessMove(MoveBase move)
        {
            var phase = Position.GetPhase();
            switch (move.Piece)
            {
                case Piece.WhitePawn:
                    if ((move.To.AsBitBoard() & Board.GetRank(6)).Any())
                    {
                        InitialMoveCollection.AddSuggested(move);
                        return;
                    }

                    break;
                case Piece.WhiteKnight:
                case Piece.WhiteBishop:
                case Piece.BlackKnight:
                case Piece.BlackBishop:
                    if (phase == Phase.Opening)
                    {
                        if ((move.To.AsBitBoard() & Board.GetPerimeter()).Any())
                        {
                            InitialMoveCollection.AddNonSuggested(move);
                            return;
                        }

                        if ((_minorStartPositions & move.From.AsBitBoard()).IsZero())
                        {
                            InitialMoveCollection.AddNonSuggested(move);
                            return;
                        }
                    }

                    break;
                case Piece.WhiteRook:
                    if (move.From == Squares.A1 && MoveHistoryService.CanDoWhiteBigCastle() ||
                        move.From == Squares.H1 && MoveHistoryService.CanDoWhiteSmallCastle())
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
                        InitialMoveCollection.AddNonSuggested(move);
                        return;
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

                            return;
                        }

                        if (phase == Phase.Middle && canCastle)
                        {
                            InitialMoveCollection.AddNonSuggested(move);
                            return;
                        }
                    }

                    break;
                case Piece.BlackPawn:
                    if ((move.To.AsBitBoard() & Board.GetRank(1)).Any())
                    {
                        InitialMoveCollection.AddSuggested(move);
                        return;
                    }

                    break;
                case Piece.BlackRook:
                    if (move.From == Squares.A8 && MoveHistoryService.CanDoBlackBigCastle() ||
                        move.From == Squares.H8 && MoveHistoryService.CanDoBlackSmallCastle())
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

                            return;
                        }

                        if (phase == Phase.Middle && canCastle)
                        {
                            InitialMoveCollection.AddNonSuggested(move);
                            return;
                        }
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Position.Make(move);
            try
            {
                if (move.Piece.IsWhite())
                {
                    if (WhiteQueenUnderAttack(move) || WhiteRookUnderAttack(move) || IsBadWhiteSee(move))
                    {
                        return;
                    }

                    if (move.Piece == Piece.WhitePawn &&
                        (MoveProvider.GetAttackPattern(Piece.WhitePawn.AsByte(), move.To.AsByte()) &
                         Board.GetBlackBits()).Any())
                    {
                        InitialMoveCollection.AddSuggested(move);
                        return;
                    }

                    if (move.Piece == Piece.WhiteKnight &&
                        (MoveProvider.GetAttackPattern(Piece.WhiteKnight.AsByte(), move.To.AsByte()) &
                         Board.GetBlackBits()).Any())
                    {
                        InitialMoveCollection.AddSuggested(move);
                        return;
                    }

                    if (move.Piece == Piece.WhiteBishop &&
                        (move.To.AsByte().BishopAttacks(Board.GetOccupied()) &
                         Board.GetBlackBits()).Any())
                    {
                        InitialMoveCollection.AddSuggested(move);
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
                    if (BlackQueenUnderAttack(move) || BlackRookUnderAttack(move) || IsBadBlackSee(move))
                    {
                        return;
                    }

                    if (move.Piece == Piece.BlackPawn &&
                        (MoveProvider.GetAttackPattern(Piece.BlackPawn.AsByte(), move.To.AsByte()) &
                         Board.GetWhiteBits()).Any())
                    {
                        InitialMoveCollection.AddSuggested(move);
                        return;
                    }

                    if (move.Piece == Piece.BlackKnight &&
                        (MoveProvider.GetAttackPattern(Piece.BlackKnight.AsByte(), move.To.AsByte()) &
                         Board.GetWhiteBits()).Any())
                    {
                        InitialMoveCollection.AddSuggested(move);
                        return;
                    }

                    if (move.Piece == Piece.BlackBishop &&
                        (move.To.AsByte().BishopAttacks(Board.GetOccupied()) &
                         Board.GetWhiteBits()).Any())
                    {
                        InitialMoveCollection.AddSuggested(move);
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
                Position.UnMake();
            }
        }

        protected bool WhiteRookUnderAttack(MoveBase move)
        {
            var pieceBits = Board.GetPieceBits(Piece.WhiteRook);
            if (!pieceBits.Any()) return false;

            pieceBits.GetPositions(PositionsList);
            for (int i = 0; i < PositionsList.Count; i++)
            {
                byte rookPosition = PositionsList[i];
                if (MoveProvider.GetBlackPawnAttackTo(Board, rookPosition).Any() ||
                    MoveProvider.GetBlackKnightAttackTo(Board, rookPosition).Any() ||
                    MoveProvider.GetBlackBishopAttackTo(Board, rookPosition).Any())
                {
                    InitialMoveCollection.AddNonSuggested(move);
                    return true;
                }
            }

            return false;
        }

        protected bool BlackRookUnderAttack(MoveBase move)
        {
            var pieceBits = Board.GetPieceBits(Piece.BlackRook);
            if (!pieceBits.Any()) return false;

            pieceBits.GetPositions(PositionsList);
            for (int i = 0; i < PositionsList.Count; i++)
            {
                byte rookPosition = PositionsList[i];
                if (MoveProvider.GetWhitePawnAttackTo(Board, rookPosition).Any() ||
                    MoveProvider.GetWhiteKnightAttackTo(Board, rookPosition).Any() ||
                    MoveProvider.GetWhiteBishopAttackTo(Board, rookPosition).Any())
                {
                    InitialMoveCollection.AddNonSuggested(move);
                    return true;
                }
            }

            return false;
        }

        protected bool BlackQueenUnderAttack(MoveBase move)
        {
            var pieceBits = Board.GetPieceBits(Piece.BlackQueen);
            if (!pieceBits.Any()) return false;

            pieceBits.GetPositions(PositionsList);
            for (int i = 0; i < PositionsList.Count; i++)
            {
                byte queenPosition = PositionsList[i];
                if (MoveProvider.GetWhitePawnAttackTo(Board, queenPosition).Any() ||
                    MoveProvider.GetWhiteKnightAttackTo(Board, queenPosition).Any() ||
                    MoveProvider.GetWhiteBishopAttackTo(Board, queenPosition).Any() ||
                    MoveProvider.GetWhiteRookAttackTo(Board, queenPosition).Any())
                {
                    InitialMoveCollection.AddBad(move);
                    return true;
                }
            }

            return false;
        }

        protected bool WhiteQueenUnderAttack(MoveBase move)
        {
            var pieceBits = Board.GetPieceBits(Piece.WhiteQueen);
            if (!pieceBits.Any()) return false;

            pieceBits.GetPositions(PositionsList);
            for (int i = 0; i < PositionsList.Count; i++)
            {
                byte queenPosition = PositionsList[i];
                if (MoveProvider.GetBlackPawnAttackTo(Board, queenPosition).Any() ||
                    MoveProvider.GetBlackKnightAttackTo(Board, queenPosition).Any() ||
                    MoveProvider.GetBlackBishopAttackTo(Board, queenPosition).Any() ||
                    MoveProvider.GetBlackRookAttackTo(Board, queenPosition).Any())
                {
                    InitialMoveCollection.AddBad(move);
                    return true;
                }
            }

            return false;
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
        protected void ProcessPromotion(MoveBase move)
        {
            Position.Make(move);
            try
            {
                if (move.Piece.IsWhite())
                {
                    MoveProvider.GetWhiteAttacksTo(move.To.AsByte(), AttackList);
                    StaticExchange(move, Piece.WhitePawn);
                }
                else
                {
                    MoveProvider.GetBlackAttacksTo(move.To.AsByte(), AttackList);
                    StaticExchange(move, Piece.BlackPawn);
                }
            }
            finally
            {
                Position.UnMake();
            }
        }

        private void StaticExchange(MoveBase move, Piece captured)
        {
            if (AttackList.Count == 0)
            {
                InitialMoveCollection.AddWinCapture(move);
            }
            else
            {
                int max = short.MinValue;
                for (int i = 0; i < AttackList.Count; i++)
                {
                    var attack = AttackList[i];
                    attack.Captured = captured;
                    var see = Board.StaticExchange(attack);
                    if (see > max)
                    {
                        max = see;
                    }
                }

                if (max < 0)
                {
                    InitialMoveCollection.AddWinCapture(move);
                }
                else if (max > 0)
                {
                    InitialMoveCollection.AddLooseCapture(move);
                }
                else
                {
                    InitialMoveCollection.AddTrade(move);
                }
            }
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