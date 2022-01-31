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
        protected readonly IMoveProvider _moveProvider = ServiceLocator.Current.GetInstance<IMoveProvider>();

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

            OrderAttacks(collection, sortedAttacks);

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

            if (pvNode is IAttack attack)
            {
                OrderAttacks(collection, sortedAttacks, attack);
            }
            else
            {
                OrderAttacks(collection, sortedAttacks);
            }

            foreach (var move in moves)
            {
                if (move.Equals(pvNode))
                {
                    collection.AddHashMove(move);
                }
                else
                {
                    if (killerMoveCollection.Contains(move) || move.IsPromotion())
                    {
                        collection.AddKillerMove(move);
                    }
                    else if (move.IsCastle())
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
        protected virtual void ProcessMove(AdvancedMoveCollection collection, IMove move)
        { 
            var board = Position.GetBoard();

            var phase = Position.GetPhase();
            if (move.Piece.IsWhite())
            {
                if (move.Piece == Piece.WhiteKing && !MoveHistoryService.GetLastMove().IsCheck())
                {
                    if (phase!=Phase.End)
                    {
                        collection.AddNonSuggested(move);
                        return; 
                    }
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
                    if (phase!=Phase.End)
                    {
                        collection.AddNonSuggested(move);
                        return; 
                    }
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
        protected bool IsCheckToWhite(IBoard board, IMove move)
        {
            var whiteKingPosition = board.GetWhiteKingPosition();
            var attacks = _moveProvider.GetAttacks(move.Piece, move.To, whiteKingPosition.AsByte());
            return attacks?.Any(attack => attack.IsLegalAttack(board)) == true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool IsBadBlackSee(AdvancedMoveCollection collection, IBoard board, IMove move)
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
        protected bool IsCheckToBlack(IBoard board, IMove move)
        {
            var blackKingPosition = board.GetBlackKingPosition();
            var attacks = _moveProvider.GetAttacks(move.Piece, move.To, blackKingPosition.AsByte());
            return attacks?.Any(attack => attack.IsLegalAttack(board)) == true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool IsBadWhiteSee(AdvancedMoveCollection collection, IBoard board, IMove move)
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
        protected bool IsBadSee(AdvancedMoveCollection collection, IBoard board, IMove move, BitBoard attackTo,
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