using System.Linq;
using System.Runtime.CompilerServices;
using Engine.Interfaces;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Sorting.Comparers;

namespace Engine.Sorting.Sorters
{
    public class ComplexSorter : AdvancedSorter
    {
        public ComplexSorter(IPosition position, IMoveComparer comparer) : base(position, comparer)
        {
            Comparer = comparer;
        }

        #region Overrides of MoveSorter

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void ProcessMove(IMove move)
        {
            var board = Position.GetBoard();

            var phase = Position.GetPhase();
            if (move.Piece.IsWhite())
            {
                if (move.Piece == Piece.WhiteKing && !MoveHistoryService.GetLastMove().IsCheck())
                {
                    if (phase != Phase.End)
                    {
                        AdvancedMoveCollection.AddNonSuggested(move);
                        return;
                    }
                }

                if (WhiteQueenUnderAttack(board, move))
                {
                    return;
                }
                if (WhiteRookUnderAttack( board, move))
                {
                    return;
                }

                if (IsBadWhiteSee(move))
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
                    if (IsCheckToBlack( move))
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
                    if (phase != Phase.End)
                    {
                        AdvancedMoveCollection.AddNonSuggested(move);
                        return;
                    }
                }

                if (BlackQueenUnderAttack(board, move))
                {
                    return;
                }
                if (BlackRookUnderAttack(board, move))
                {
                    return;
                }

                if (IsBadBlackSee(move))
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
                    if (IsCheckToWhite( move))
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
        private bool BlackRookUnderAttack(IBoard board, IMove move)
        {
            var pieceBits = board.GetPieceBits(Piece.BlackRook);
            if (!pieceBits.Any()) return false;

            foreach (var rookPosition in pieceBits.BitScan())
            {
                if (MoveProvider.GetWhitePawnAttackTo(board, rookPosition).Any() ||
                    MoveProvider.GetWhiteKnightAttackTo(board, rookPosition).Any() ||
                    MoveProvider.GetWhiteBishopAttackTo(board, rookPosition).Any())
                {
                    AdvancedMoveCollection.AddBadMove(move);
                    return true;
                }

                var whiteRookAttackTo = MoveProvider.GetWhiteRookAttackTo(board, rookPosition);
                foreach (var p in whiteRookAttackTo.BitScan())
                {
                    foreach (var a in MoveProvider.GetAttacks(Piece.WhiteRook, p, rookPosition))
                    {
                        a.Captured = Piece.BlackRook;
                        if (board.StaticExchange(a) <= 0) continue;

                        AdvancedMoveCollection.AddBadMove(move);
                        return true;
                    }
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool BlackQueenUnderAttack(IBoard board, IMove move)
        {
            var pieceBits = board.GetPieceBits(Piece.BlackQueen);
            if (!pieceBits.Any()) return false;

            var queenPosition = pieceBits.BitScanForward();
            if (MoveProvider.GetWhitePawnAttackTo(board, queenPosition).Any() ||
                MoveProvider.GetWhiteKnightAttackTo(board, queenPosition).Any() ||
                MoveProvider.GetWhiteBishopAttackTo(board, queenPosition).Any() ||
                MoveProvider.GetWhiteRookAttackTo(board, queenPosition).Any())
            {
                AdvancedMoveCollection.AddBadMove(move);
            }
            else
            {
                var whiteQueenAttackTo = MoveProvider.GetWhiteQueenAttackTo(board, queenPosition);
                if (!whiteQueenAttackTo.Any()) return false;

                var attack = MoveProvider
                    .GetAttacks(Piece.WhiteQueen, whiteQueenAttackTo.BitScanForward(), queenPosition)
                    .First();
                attack.Captured = Piece.BlackQueen;
                if (board.StaticExchange(attack) <= 0) return false;

                AdvancedMoveCollection.AddBadMove(move);
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool WhiteQueenUnderAttack(IBoard board, IMove move)
        {
            var pieceBits = board.GetPieceBits(Piece.WhiteQueen);
            if (!pieceBits.Any()) return false;

            var queenPosition = pieceBits.BitScanForward();
            if (MoveProvider.GetBlackPawnAttackTo(board, queenPosition).Any() ||
                MoveProvider.GetBlackKnightAttackTo(board, queenPosition).Any() ||
                MoveProvider.GetBlackBishopAttackTo(board, queenPosition).Any() ||
                MoveProvider.GetBlackRookAttackTo(board, queenPosition).Any())
            {
                AdvancedMoveCollection.AddBadMove(move);
            }
            else
            {
                var blackQueenAttackTo = MoveProvider.GetBlackQueenAttackTo(board, queenPosition);
                if (!blackQueenAttackTo.Any()) return false;

                var attack = MoveProvider
                    .GetAttacks(Piece.BlackQueen, blackQueenAttackTo.BitScanForward(), queenPosition)
                    .First();
                attack.Captured = Piece.WhiteQueen;
                if (board.StaticExchange(attack) <= 0) return false;

                AdvancedMoveCollection.AddBadMove(move);
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool WhiteRookUnderAttack(IBoard board, IMove move)
        {
            var pieceBits = board.GetPieceBits(Piece.WhiteRook);
            if (!pieceBits.Any()) return false;

            foreach (var rookPosition in pieceBits.BitScan())
            {
                if (MoveProvider.GetBlackPawnAttackTo(board, rookPosition).Any() ||
                    MoveProvider.GetBlackKnightAttackTo(board, rookPosition).Any() ||
                    MoveProvider.GetBlackBishopAttackTo(board, rookPosition).Any())
                {
                    AdvancedMoveCollection.AddBadMove(move);
                    return true;
                }

                var blackRookAttackTo = MoveProvider.GetBlackRookAttackTo(board, rookPosition);
                foreach (var p in blackRookAttackTo.BitScan())
                {
                    foreach (var a in MoveProvider.GetAttacks(Piece.BlackRook, p, rookPosition))
                    {
                        a.Captured = Piece.WhiteRook;
                        if (board.StaticExchange(a) <= 0) continue;

                        AdvancedMoveCollection.AddBadMove(move);
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion
    }
}