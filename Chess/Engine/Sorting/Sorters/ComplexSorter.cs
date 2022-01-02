using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CommonServiceLocator;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;

namespace Engine.Sorting.Sorters
{
    public class ComplexSorter : MoveSorter
    {
        private readonly IPosition _position;
        private readonly IMoveProvider _moveProvider;

        public ComplexSorter(IPosition position)
        {
            _position = position;
            _moveProvider = ServiceLocator.Current.GetInstance<IMoveProvider>();
        }

        #region Overrides of MoveSorter

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override IEnumerable<IMove> OrderInternal(IEnumerable<IMove> moves, KillerMoveCollection collection)
        {
            var heap = new Heap(2, 64);
            var killer = collection.GetMoves();

            ProcessMoves(moves, killer, heap);

            return OrderInternal(heap);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override IEnumerable<IMove> OrderInternal(IEnumerable<IMove> moves, KillerMoveCollection collection, IMove cutMove)
        {
            var heap = new Heap(2, 64);
            var killer = collection.GetMoves(cutMove);

            ProcessMoves(moves, killer, heap);

            return OrderInternal(heap);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override IEnumerable<IMove> OrderInternal(IEnumerable<IMove> moves, KillerMoveCollection collection, IMove pvNode, IMove cutMove)
        {
            var heap = new Heap(2, 64);
            var killer = pvNode.Equals(cutMove)? collection.GetMoves() : collection.GetMoves(cutMove);

            ProcessMoves(moves, killer, heap, pvNode);

            return OrderInternal(heap);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IEnumerable<IMove> OrderInternal(Heap heap)
        {
            var count = heap.GetCount();
            for (int i = 0; i < count; i++)
            {
                yield return heap.Maximum();
            }
        }

        private void ProcessMoves(IEnumerable<IMove> moves, ICollection<IMove> killer, Heap heap, IMove pvNode = null)
        {
            Dictionary<byte, int> attacksCache = new Dictionary<byte, int>(8);
            foreach (var move in moves)
            {
                if (move.Equals(pvNode))
                {
                    move.Value = move.Difference + 600;
                }
                else if (move.IsPromotion())
                {
                    move.Value = move.Difference + 500;
                }
                else if (move.IsAttack())
                {
                    ProcessAttack(move, attacksCache);
                }
                else if (move.IsCastle())
                {
                    move.Value = move.Difference + 200;
                }
                else
                {
                    if (_moveHistoryService.IsAdditionalDebutMove(move))
                    {
                        if (move.Piece == Piece.BlackPawn || move.Piece == Piece.BlackPawn)
                        {
                            move.Value = move.Difference - 100;
                        }
                        else
                        {
                            move.Value = move.Difference - 200;
                        }
                    }
                    else if (killer.Contains(move))
                    {
                        move.Value = move.Difference + 300;
                    }
                    else if (move.Type == MoveType.Over)
                    {
                        move.Value = move.Difference - 200;
                    }
                    else
                    {
                        ProcessMove(move);
                    }
                }

                heap.Insert(move);
            }
        }

        private void ProcessMove(IMove move)
        {
            var ply = _moveHistoryService.GetPly();
            switch (move.Piece)
            {
                case Piece.WhiteRook:
                    if (_moveHistoryService.CanDoWhiteSmallCastle() && move.From == Squares.H1 ||
                        _moveHistoryService.CanDoWhiteBigCastle() && move.From == Squares.A1)
                    {
                        move.Value =  move.Difference - 400;
                    }
                    else
                    {
                        SetHardPieceMove(move, ply);
                    }

                    break;
                case Piece.BlackRook:
                    if (_moveHistoryService.CanDoBlackSmallCastle() && move.From == Squares.H8 ||
                        _moveHistoryService.CanDoBlackBigCastle() && move.From == Squares.A8)
                    {
                        move.Value =  move.Difference - 400;
                    }
                    else
                    {
                        SetHardPieceMove(move, ply);
                    }

                    break;
                case Piece.BlackQueen:
                case Piece.WhiteQueen:
                    SetHardPieceMove(move, ply);

                    break;
                case Piece.WhiteKing:
                    if (_moveHistoryService.CanDoWhiteSmallCastle() || _moveHistoryService.CanDoWhiteBigCastle())
                    {
                        move.Value =  move.Difference - 500;
                    }
                    else
                    {
                        SetKingMove(move, ply);
                    }

                    break;
                case Piece.BlackKing:
                    if (_moveHistoryService.CanDoBlackSmallCastle() || _moveHistoryService.CanDoBlackBigCastle())
                    {
                        move.Value =  move.Difference - 500;
                    }
                    else
                    {
                        SetKingMove(move, ply);
                    }

                    break;
                default:
                    move.Value = CalculateValue(move, move.Difference);
                    break;
            }
        }

        private int CalculateValue(IMove move, int defaultValue)
        {
            try
            {
                int balance = 0;
                int victimValue = move.Piece.AsValue();
                _position.Make(move);
                if (move.Piece.IsWhite())
                {
                    var materialBalance = _moveProvider.GetBlackMaterialBalance(_position.GetBoard(), move.To.AsByte());
                    while (materialBalance.Black.Count > 0)
                    {
                        if (balance < 0) break;

                        balance -= victimValue;
                        victimValue = materialBalance.Black.Dequeue().AsValue();
                        if (materialBalance.White.Count > 0)
                        {
                            balance += victimValue;
                            victimValue = materialBalance.White.Dequeue().AsValue();
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                else
                {
                    var materialBalance = _moveProvider.GetWhiteMaterialBalance(_position.GetBoard(), move.To.AsByte());
                    while (materialBalance.White.Count > 0)
                    {
                        if (balance < 0) break;

                        balance -= victimValue;
                        victimValue = materialBalance.White.Dequeue().AsValue();
                        if (materialBalance.Black.Count > 0)
                        {
                            balance += victimValue;
                            victimValue = materialBalance.Black.Dequeue().AsValue();
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                if (balance < 0)
                {
                    return defaultValue - 600 - move.Piece.AsValue();
                }

                return defaultValue;
            }
            finally
            {
                _position.UnMake();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetKingMove(IMove move, int ply)
        {
            if (ply < 12)
            {
                move.Value =  move.Difference - 400;
            }
            else if (ply < 20)
            {
                move.Value =  move.Difference - 300;
            }
            else
            {
                move.Value = move.Difference;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetHardPieceMove(IMove move, int ply)
        {
            move.Value = ply < 12 ? CalculateValue(move,  (move.Difference - 150)) : CalculateValue(move,move.Difference);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessAttack(IMove move, Dictionary<byte, int> attacksCache)
        {
            int value = move.Piece.AsValue();
            int victimValue = _position.GetPieceValue(move.To);

            if (value < victimValue)
            {
                move.Value =  move.Difference + 400 - value;
            }
            else
            {
                if (!attacksCache.TryGetValue(move.To.AsByte(),out var balance))
                {
                    if (move.Piece.IsBlack())
                    {
                        MaterialBalance materialBalance = _moveProvider.GetBlackMaterialBalance(_position.GetBoard(), move.To.AsByte());
                        while (materialBalance.Black.Count > 0)
                        {
                            balance += victimValue;
                            victimValue = materialBalance.Black.Dequeue().AsValue();
                            if (materialBalance.White.Count > 0)
                            {
                                balance -= victimValue;
                                victimValue = materialBalance.White.Dequeue().AsValue();
                            }
                            else
                            {
                                break;
                            }
                        }

                        attacksCache[move.To.AsByte()] = balance;
                    }
                    else
                    {
                        MaterialBalance materialBalance = _moveProvider.GetWhiteMaterialBalance(_position.GetBoard(), move.To.AsByte());
                        while (materialBalance.White.Count > 0)
                        {
                            balance += victimValue;
                            victimValue = materialBalance.White.Dequeue().AsValue();
                            if (materialBalance.Black.Count > 0)
                            {
                                balance -= victimValue;
                                victimValue = materialBalance.Black.Dequeue().AsValue();
                            }
                            else
                            {
                                break;
                            }
                        }

                        attacksCache[move.To.AsByte()] = balance;
                    }
                }

                if (balance > 0)
                {
                    move.Value =  move.Difference + 400 - value;
                }
                else if (balance < 0)
                {
                    move.Value = move.Difference - 600 - value;
                }
                else
                {
                    move.Value =  move.Difference + 100 - value;
                }
            }
        }

        #endregion
    }
}