using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using CommonServiceLocator;
using Engine.DataStructures.Moves;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Sorting.Comparers;

namespace Engine.Sorting.Sorters
{
    public class AdvancedSorter : MoveSorter
    {
        protected AdvancedMoveCollection AdvancedMoveCollection;
        protected readonly IMoveProvider MoveProvider = ServiceLocator.Current.GetInstance<IMoveProvider>();

        public AdvancedSorter(IPosition position, IMoveComparer comparer) : base(position, comparer)
        {
            AdvancedMoveCollection = new AdvancedMoveCollection(comparer);
            Comparer = comparer;
        }

        #region Overrides of MoveSorter

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override IMove[] OrderInternal(IEnumerable<IAttack> attacks, IEnumerable<IMove> moves,
            IKillerMoveCollection killerMoveCollection)
        {
            var sortedAttacks = OrderAttacks(attacks);

            OrderAttacks(AdvancedMoveCollection, sortedAttacks);

            foreach (var move in moves)
            {
                if (killerMoveCollection.Contains(move.Key))
                {
                    AdvancedMoveCollection.AddKillerMove(move);
                }
                else if (move.IsCastle() || move.IsPromotion())
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
        protected override IMove[] OrderInternal(IEnumerable<IAttack> attacks, IEnumerable<IMove> moves,
            IKillerMoveCollection killerMoveCollection,
            IMove pvNode)
        {
            var sortedAttacks = OrderAttacks(attacks);

            if (pvNode is IAttack attack)
            {
                OrderAttacks(AdvancedMoveCollection, sortedAttacks, attack);

                foreach (var move in moves)
                {
                    if (killerMoveCollection.Contains(move.Key))
                    {
                        AdvancedMoveCollection.AddKillerMove(move);
                    }
                    else if (move.IsCastle() || move.IsPromotion())
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
                OrderAttacks(AdvancedMoveCollection, sortedAttacks);

                foreach (var move in moves)
                {
                    if (move.Equals(pvNode))
                    {
                        AdvancedMoveCollection.AddHashMove(move);
                    }
                    else
                    {
                        if (killerMoveCollection.Contains(move.Key))
                        {
                            AdvancedMoveCollection.AddKillerMove(move);
                        }
                        else if (move.IsCastle() || move.IsPromotion())
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
        protected virtual void ProcessMove(IMove move)
        { 
            var board = Position.GetBoard();

            var phase = Position.GetPhase();
            if (move.Piece.IsWhite())
            {
                if (move.Piece == Piece.WhiteKing && !MoveHistoryService.GetLastMove().IsCheck())
                {
                    if (phase!=Phase.End)
                    {
                        AdvancedMoveCollection.AddNonSuggested(move);
                        return; 
                    }
                }

                if (IsBadWhiteSee(board, move))
                {
                    return;
                }

                if (phase == Phase.Opening)
                {
                    switch (move.Piece)
                    {
                        case Piece.WhiteKnight:
                        case Piece.WhiteBishop:
                        {
                            var bit = move.To.AsBitBoard();
                            if ((bit & board.GetPerimeter()).Any())
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
                    if (IsCheckToBlack(board, move))
                    {
                        AdvancedMoveCollection.AddCheck(move);
                    }
                    else
                    {
                        AdvancedMoveCollection.AddNonCapture(move);
                    }

                    Position.UnDo(move);
                }
            }
            else
            {
                if (move.Piece == Piece.BlackKing && !MoveHistoryService.GetLastMove().IsCheck())
                {
                    if (phase!=Phase.End)
                    {
                        AdvancedMoveCollection.AddNonSuggested(move);
                        return; 
                    }
                }

                if (IsBadBlackSee(board, move))
                {
                    return;
                }

                if (phase == Phase.Opening)
                {
                    switch (move.Piece)
                    {
                        case Piece.BlackKnight:
                        case Piece.BlackBishop:
                        {
                            var bit = move.To.AsBitBoard();
                            if ((bit & board.GetPerimeter()).Any())
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
                    if (IsCheckToWhite(board, move))
                    {
                        AdvancedMoveCollection.AddCheck(move);
                    }
                    else
                    {
                        AdvancedMoveCollection.AddNonCapture(move);
                    }

                    Position.UnDo(move);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool IsCheckToWhite(IBoard board, IMove move)
        {
            var whiteKingPosition = board.GetWhiteKingPosition();
            var attacks = MoveProvider.GetAttacks(move.Piece, move.To, whiteKingPosition.AsByte());
            return attacks?.Any(attack => attack.IsLegalAttack(board)) == true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool IsBadBlackSee(IBoard board, IMove move)
        {
            var attackTo = MoveProvider.GetWhitePawnAttackTo(board, move.To.AsByte());
            if (IsBadSee(board, move, attackTo, Piece.WhitePawn)) return true;

            attackTo = MoveProvider.GetWhiteKnightAttackTo(board, move.To.AsByte());
            if (IsBadSee(board, move, attackTo, Piece.WhiteKnight)) return true;

            attackTo = MoveProvider.GetWhiteBishopAttackTo(board, move.To.AsByte());
            if (IsBadSee(board, move, attackTo, Piece.WhiteBishop)) return true;

            attackTo = MoveProvider.GetWhiteRookAttackTo(board, move.To.AsByte());
            if (IsBadSee(board, move, attackTo, Piece.WhiteRook)) return true;

            attackTo = MoveProvider.GetWhiteQueenAttackTo(board, move.To.AsByte());
            if (IsBadSee(board, move, attackTo, Piece.WhiteQueen)) return true;

            attackTo = MoveProvider.GetWhiteKingAttackTo(board, move.To.AsByte());
            if (IsBadSee(board, move, attackTo, Piece.WhiteKing)) return true;

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool IsCheckToBlack(IBoard board, IMove move)
        {
            var blackKingPosition = board.GetBlackKingPosition();
            var attacks = MoveProvider.GetAttacks(move.Piece, move.To, blackKingPosition.AsByte());
            return attacks?.Any(attack => attack.IsLegalAttack(board)) == true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool IsBadWhiteSee(IBoard board, IMove move)
        {
            var attackTo = MoveProvider.GetBlackPawnAttackTo(board, move.To.AsByte());
            if (IsBadSee(board, move, attackTo, Piece.BlackPawn)) return true;

            attackTo = MoveProvider.GetBlackKnightAttackTo(board, move.To.AsByte());
            if (IsBadSee(board, move, attackTo, Piece.BlackKnight)) return true;

            attackTo = MoveProvider.GetBlackBishopAttackTo(board, move.To.AsByte());
            if (IsBadSee(board, move, attackTo, Piece.BlackBishop)) return true;

            attackTo = MoveProvider.GetBlackRookAttackTo(board, move.To.AsByte());
            if (IsBadSee(board, move, attackTo, Piece.BlackRook)) return true;

            attackTo = MoveProvider.GetBlackQueenAttackTo(board, move.To.AsByte());
            if (IsBadSee( board, move, attackTo, Piece.BlackQueen)) return true;

            attackTo = MoveProvider.GetBlackKingAttackTo(board, move.To.AsByte());
            if (IsBadSee(board, move, attackTo, Piece.BlackKing)) return true;

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool IsBadSee(IBoard board, IMove move, BitBoard attackTo,
            Piece piece)
        {
            foreach (var p in attackTo.BitScan())
            {
                foreach (var a in MoveProvider.GetAttacks(piece, p, board))
                {
                    a.Captured = move.Piece;
                    if (board.StaticExchange(a) <= 0) continue;

                    AdvancedMoveCollection.AddBadMove(move);
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}