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
        private int _value;
        private readonly IMoveProvider _moveProvider = ServiceLocator.Current.GetInstance<IMoveProvider>();

        public AdvancedSorter(IPosition position, IMoveComparer comparer) : base(position)
        {
            Comparer = comparer;
        }

        #region Overrides of MoveSorter


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override IMoveCollection OrderInternal(IEnumerable<IAttack> attacks, IEnumerable<IMove> moves,
            KillerMoveCollection killerMoveCollection)
        {
            var sortedAttacks = OrderAttacks(attacks);

            AdvancedMoveCollection collection = new AdvancedMoveCollection(Comparer);

            _value = Position.GetStaticValue();

            OrderAttacks(collection, sortedAttacks);

            if (collection.HasWinCaptures())
            {
                foreach (var move in moves)
                {
                    if (killerMoveCollection.Contains(move))
                    {
                        collection.AddKillerMove(move);
                    }
                    else if (move.IsCastle() || move.IsPromotion())
                    {
                        collection.AddSuggested(move);
                    }
                    else
                    {
                        collection.AddNonCapture(move);
                    }
                }
            }
            else
            {
                foreach (var move in moves)
                {
                    if (killerMoveCollection.Contains(move))
                    {
                        collection.AddKillerMove(move);
                    }
                    else if (move.IsCastle() || move.IsPromotion())
                    {
                        collection.AddSuggested(move);
                    }
                    else
                    {
                        ProcessMove(collection, move);
                    }
                }
            }

            collection.Build();
            return collection;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override IMoveCollection OrderInternal(IEnumerable<IAttack> attacks, IEnumerable<IMove> moves,
            KillerMoveCollection killerMoveCollection,
            IMove pvNode)
        {
            var sortedAttacks = OrderAttacks(attacks);

            AdvancedMoveCollection collection = new AdvancedMoveCollection(Comparer);

            _value = Position.GetStaticValue();

            if (pvNode is IAttack attack)
            {
                OrderAttacks(collection, sortedAttacks, attack);
            }
            else
            {
                OrderAttacks(collection, sortedAttacks);
            }

            if (collection.HasWinCaptures())
            {
                foreach (var move in moves)
                {
                    if (move.Equals(pvNode))
                    {
                        collection.AddHashMove(move);
                    }
                    else
                    {
                        if (killerMoveCollection.Contains(move))
                        {
                            collection.AddKillerMove(move);
                        }
                        else if (move.IsCastle() || move.IsPromotion())
                        {
                            collection.AddSuggested(move);
                        }
                        else
                        {
                            collection.AddNonCapture(move);
                        }
                    }
                }
            }
            else
            {
                foreach (var move in moves)
                {
                    if (move.Equals(pvNode))
                    {
                        collection.AddHashMove(move);
                    }
                    else
                    {
                        if (killerMoveCollection.Contains(move))
                        {
                            collection.AddKillerMove(move);
                        }
                        else if (move.IsCastle() || move.IsPromotion())
                        {
                            collection.AddSuggested(move);
                        }
                        else
                        {
                            ProcessMove(collection, move);
                        }
                    }
                }
            }

            collection.Build();
            return collection;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessMove(AdvancedMoveCollection collection, IMove move)
        {
            var board = Position.GetBoard();

            var phase = Position.GetPhase();
            if (move.Piece.IsWhite())
            {
                if (move.Piece == Piece.WhiteKing && !MoveHistoryService.GetLastMove().IsCheck())
                {
                    collection.AddNonSuggested(move);
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
                    collection.AddNonSuggested(move);
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
        private void ProcessBlackMove(AdvancedMoveCollection collection, IBoard board, IMove move)
        {
            //if (BlackQueenUnderAttack(collection, board, move))
            //{
            //    return;
            //}
            //if (BlackRookUnderAttack(collection, board, move))
            //{
            //    return;
            //}

            if (move.Piece != Piece.BlackPawn && IsBadBlackSee(collection, board, move))
            {
                return;
            }

            if (IsCheckToWhite(board, move))
            {
                collection.AddCheck(move);
            }
            else
            {
                collection.AddNonCapture(move);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsCheckToWhite(IBoard board, IMove move)
        {
            var whiteKingPosition = board.GetWhiteKingPosition();
            var attacks = _moveProvider.GetAttacks(move.Piece, move.To, whiteKingPosition.AsByte());
            return attacks?.Any(attack => attack.IsLegalAttack(board)) == true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsBadBlackSee(AdvancedMoveCollection collection, IBoard board, IMove move)
        {
            var attackTo = _moveProvider.GetWhitePawnAttackTo(board, move.To.AsByte());
            if (IsBadSee(collection, board, move, attackTo, Piece.WhitePawn)) return true;

            attackTo = _moveProvider.GetWhiteKnightAttackTo(board, move.To.AsByte());
            if (IsBadSee(collection, board, move, attackTo, Piece.WhiteKnight)) return true;

            attackTo = _moveProvider.GetWhiteBishopAttackTo(board, move.To.AsByte());
            if (IsBadSee(collection, board, move, attackTo, Piece.WhiteBishop)) return true;

            attackTo = _moveProvider.GetWhiteRookAttackTo(board, move.To.AsByte());
            if (IsBadSee(collection, board, move, attackTo, Piece.WhiteRook)) return true;

            attackTo = _moveProvider.GetWhiteQueenAttackTo(board, move.To.AsByte());
            if (IsBadSee(collection, board, move, attackTo, Piece.WhiteQueen)) return true;

            attackTo = _moveProvider.GetWhiteKingAttackTo(board, move.To.AsByte());
            if (IsBadSee(collection, board, move, attackTo, Piece.WhiteKing)) return true;

            return false;
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
        private void ProcessWhiteMove(AdvancedMoveCollection collection, IBoard board, IMove move)
        {
            //if (WhiteQueenUnderAttack(collection, board, move))
            //{
            //    return;
            //}
            //if (WhiteRookUnderAttack(collection, board, move))
            //{
            //    return;
            //}

            //if (move.Piece!=Piece.WhitePawn && IsBadWhiteSee(collection, board, move))
            //{
            //    return;
            //}

            if (IsCheckToBlack(board, move))
            {
                collection.AddCheck(move);
            }
            else
            {
                collection.AddNonCapture(move);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsCheckToBlack(IBoard board, IMove move)
        {
            var blackKingPosition = board.GetBlackKingPosition();
            var attacks = _moveProvider.GetAttacks(move.Piece, move.To, blackKingPosition.AsByte());
            return attacks?.Any(attack => attack.IsLegalAttack(board)) == true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsBadWhiteSee(AdvancedMoveCollection collection, IBoard board, IMove move)
        {
            var attackTo = _moveProvider.GetBlackPawnAttackTo(board, move.To.AsByte());
            if (IsBadSee(collection, board, move, attackTo, Piece.BlackPawn)) return true;

            attackTo = _moveProvider.GetBlackKnightAttackTo(board, move.To.AsByte());
            if (IsBadSee(collection, board, move, attackTo, Piece.BlackKnight)) return true;

            attackTo = _moveProvider.GetBlackBishopAttackTo(board, move.To.AsByte());
            if (IsBadSee(collection, board, move, attackTo, Piece.BlackBishop)) return true;

            attackTo = _moveProvider.GetBlackRookAttackTo(board, move.To.AsByte());
            if (IsBadSee(collection, board, move, attackTo, Piece.BlackRook)) return true;

            attackTo = _moveProvider.GetBlackQueenAttackTo(board, move.To.AsByte());
            if (IsBadSee(collection, board, move, attackTo, Piece.BlackQueen)) return true;

            attackTo = _moveProvider.GetBlackKingAttackTo(board, move.To.AsByte());
            if (IsBadSee(collection, board, move, attackTo, Piece.BlackKing)) return true;

            return false;
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

        protected override void DecideTrade(AttackCollection collection, IAttack attack)
        {
            if (collection is AdvancedAttackCollection advancedMoveCollection && _value < 0)
            {
                advancedMoveCollection.AddLooseTrade(attack);
            }
            else
            {
                base.DecideTrade(collection, attack);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsBadSee(AdvancedMoveCollection collection, IBoard board, IMove move, BitBoard attackTo,
            Piece piece)
        {
            foreach (var p in attackTo.BitScan())
            {
                foreach (var a in _moveProvider.GetAttacks(piece, p, board))
                {
                    a.Captured = move.Piece;
                    if (board.StaticExchange(a) <= 0) continue;

                    collection.AddBadMove(move);
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}