using System.Linq;
using System.Runtime.CompilerServices;
using Engine.DataStructures.Moves;
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
        protected override void ProcessMove(AdvancedMoveCollection collection, IMove move)
        {
            var board = Position.GetBoard();

            var phase = Position.GetPhase();
            if (move.Piece.IsWhite())
            {
                if (move.Piece == Piece.WhiteKing && !MoveHistoryService.GetLastMove().IsCheck())
                {
                    if (phase != Phase.End)
                    {
                        collection.AddNonSuggested(move);
                        return;
                    }
                }

                if (WhiteQueenUnderAttack(collection, board, move))
                {
                    return;
                }
                if (WhiteRookUnderAttack(collection, board, move))
                {
                    return;
                }

                if (IsBadWhiteSee(collection, board, move))
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
                                collection.AddNonSuggested(move);
                                return;
                            }

                            break;

                        }
                        case Piece.WhiteRook:
                        {
                            if (move.From == Squares.A1 && MoveHistoryService.CanDoWhiteBigCastle())
                            {
                                collection.AddNonSuggested(move);
                                return;
                            }

                            if (move.From == Squares.H1 && MoveHistoryService.CanDoWhiteSmallCastle())
                            {
                                collection.AddNonSuggested(move);
                                return;
                            }

                            break;
                        }
                        case Piece.WhiteQueen:
                        {
                            collection.AddNonSuggested(move);
                            return;
                        }
                    }

                    collection.AddNonCapture(move);
                }
                else
                {
                    Position.Do(move);
                    if (IsCheckToBlack(board, move))
                    {
                        collection.AddCheck(move);
                    }
                    else
                    {
                        collection.AddNonCapture(move);
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
                        collection.AddNonSuggested(move);
                        return;
                    }
                }

                if (BlackQueenUnderAttack(collection, board, move))
                {
                    return;
                }
                if (BlackRookUnderAttack(collection, board, move))
                {
                    return;
                }

                if (IsBadBlackSee(collection, board, move))
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
                                collection.AddNonSuggested(move);
                                return;
                            }

                            break;
                        }
                        case Piece.BlackRook:
                            if (move.From == Squares.A8 && MoveHistoryService.CanDoBlackBigCastle())
                            {
                                collection.AddNonSuggested(move);
                                return;
                            }

                            if (move.From == Squares.H8 && MoveHistoryService.CanDoBlackSmallCastle())
                            {
                                collection.AddNonSuggested(move);
                                return;
                            }

                            break;
                        case Piece.BlackQueen:
                            collection.AddNonSuggested(move);
                            return;
                    }

                    collection.AddNonCapture(move);
                }
                else
                {
                    Position.Do(move);
                    if (IsCheckToWhite(board, move))
                    {
                        collection.AddCheck(move);
                    }
                    else
                    {
                        collection.AddNonCapture(move);
                    }

                    Position.UnDo(move);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool BlackRookUnderAttack(AdvancedMoveCollection collection, IBoard board, IMove move)
        {
            var pieceBits = board.GetPieceBits(Piece.BlackRook);
            if (!pieceBits.Any()) return false;

            foreach (var rookPosition in pieceBits.BitScan())
            {
                if (_moveProvider.GetWhitePawnAttackTo(board, rookPosition).Any() ||
                    _moveProvider.GetWhiteKnightAttackTo(board, rookPosition).Any() ||
                    _moveProvider.GetWhiteBishopAttackTo(board, rookPosition).Any())
                {
                    collection.AddBadMove(move);
                    return true;
                }

                var whiteRookAttackTo = _moveProvider.GetWhiteRookAttackTo(board, rookPosition);
                foreach (var p in whiteRookAttackTo.BitScan())
                {
                    foreach (var a in _moveProvider.GetAttacks(Piece.WhiteRook, p, rookPosition))
                    {
                        a.Captured = Piece.BlackRook;
                        if (board.StaticExchange(a) <= 0) continue;

                        collection.AddBadMove(move);
                        return true;
                    }
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool BlackQueenUnderAttack(AdvancedMoveCollection collection, IBoard board, IMove move)
        {
            var pieceBits = board.GetPieceBits(Piece.BlackQueen);
            if (!pieceBits.Any()) return false;

            var queenPosition = pieceBits.BitScanForward();
            if (_moveProvider.GetWhitePawnAttackTo(board, queenPosition).Any() ||
                _moveProvider.GetWhiteKnightAttackTo(board, queenPosition).Any() ||
                _moveProvider.GetWhiteBishopAttackTo(board, queenPosition).Any() ||
                _moveProvider.GetWhiteRookAttackTo(board, queenPosition).Any())
            {
                collection.AddBadMove(move);
            }
            else
            {
                var whiteQueenAttackTo = _moveProvider.GetWhiteQueenAttackTo(board, queenPosition);
                if (!whiteQueenAttackTo.Any()) return false;

                var attack = _moveProvider
                    .GetAttacks(Piece.WhiteQueen, whiteQueenAttackTo.BitScanForward(), queenPosition)
                    .First();
                attack.Captured = Piece.BlackQueen;
                if (board.StaticExchange(attack) <= 0) return false;

                collection.AddBadMove(move);
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool WhiteQueenUnderAttack(AdvancedMoveCollection collection, IBoard board, IMove move)
        {
            var pieceBits = board.GetPieceBits(Piece.WhiteQueen);
            if (!pieceBits.Any()) return false;

            var queenPosition = pieceBits.BitScanForward();
            if (_moveProvider.GetBlackPawnAttackTo(board, queenPosition).Any() ||
                _moveProvider.GetBlackKnightAttackTo(board, queenPosition).Any() ||
                _moveProvider.GetBlackBishopAttackTo(board, queenPosition).Any() ||
                _moveProvider.GetBlackRookAttackTo(board, queenPosition).Any())
            {
                collection.AddBadMove(move);
            }
            else
            {
                var blackQueenAttackTo = _moveProvider.GetBlackQueenAttackTo(board, queenPosition);
                if (!blackQueenAttackTo.Any()) return false;

                var attack = _moveProvider
                    .GetAttacks(Piece.BlackQueen, blackQueenAttackTo.BitScanForward(), queenPosition)
                    .First();
                attack.Captured = Piece.WhiteQueen;
                if (board.StaticExchange(attack) <= 0) return false;

                collection.AddBadMove(move);
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool WhiteRookUnderAttack(AdvancedMoveCollection collection, IBoard board, IMove move)
        {
            var pieceBits = board.GetPieceBits(Piece.WhiteRook);
            if (!pieceBits.Any()) return false;

            foreach (var rookPosition in pieceBits.BitScan())
            {
                if (_moveProvider.GetBlackPawnAttackTo(board, rookPosition).Any() ||
                    _moveProvider.GetBlackKnightAttackTo(board, rookPosition).Any() ||
                    _moveProvider.GetBlackBishopAttackTo(board, rookPosition).Any())
                {
                    collection.AddBadMove(move);
                    return true;
                }

                var blackRookAttackTo = _moveProvider.GetBlackRookAttackTo(board, rookPosition);
                foreach (var p in blackRookAttackTo.BitScan())
                {
                    foreach (var a in _moveProvider.GetAttacks(Piece.BlackRook, p, rookPosition))
                    {
                        a.Captured = Piece.WhiteRook;
                        if (board.StaticExchange(a) <= 0) continue;

                        collection.AddBadMove(move);
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion
    }
}