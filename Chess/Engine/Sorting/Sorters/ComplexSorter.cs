﻿using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CommonServiceLocator;
using Engine.DataStructures;
using Engine.DataStructures.Moves;
using Engine.Interfaces;
using Engine.Models.Enums;
using Engine.Models.Helpers;

namespace Engine.Sorting.Sorters
{
    public class ComplexSorter : MoveSorter
    {
        private readonly IMoveProvider _moveProvider;

        public ComplexSorter(IPosition position) : base(position)
        {
            _moveProvider = ServiceLocator.Current.GetInstance<IMoveProvider>();
        }

        #region Overrides of MoveSorter

        protected override IMoveCollection OrderInternal(IEnumerable<IAttack> attacks, IEnumerable<IMove> moves,
            KillerMoveCollection killerMoveCollection)
        {
            throw new System.NotImplementedException();
        }

        protected override IMoveCollection OrderInternal(IEnumerable<IAttack> attacks, IEnumerable<IMove> moves,
            KillerMoveCollection killerMoveCollection,
            IMove pvNode)
        {
            throw new System.NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected IEnumerable<IMove> OrderInternal(IEnumerable<IMove> moves, KillerMoveCollection collection)
        {
            var heap = new Heap(2, 64);

            ProcessMoves(moves, collection, heap);

            return OrderInternal(heap);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected IEnumerable<IMove> OrderInternal(IEnumerable<IMove> moves, KillerMoveCollection collection, IMove cutMove)
        {
            var heap = new Heap(2, 64);

            ProcessMoves(moves, collection, heap);

            return OrderInternal(heap);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected IEnumerable<IMove> OrderInternal(IEnumerable<IMove> moves, KillerMoveCollection collection, IMove pvNode, IMove cutMove)
        {
            var heap = new Heap(2, 64);

            ProcessMoves(moves, collection, heap, pvNode);

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

        private void ProcessMoves(IEnumerable<IMove> moves, KillerMoveCollection killer, Heap heap, IMove pvNode = null)
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
                    if (MoveHistoryService.IsAdditionalDebutMove(move))
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
            var ply = MoveHistoryService.GetPly();
            switch (move.Piece)
            {
                case Piece.WhiteRook:
                    if (MoveHistoryService.CanDoWhiteSmallCastle() && move.From == Squares.H1 ||
                        MoveHistoryService.CanDoWhiteBigCastle() && move.From == Squares.A1)
                    {
                        move.Value =  move.Difference - 400;
                    }
                    else
                    {
                        SetHardPieceMove(move, ply);
                    }

                    break;
                case Piece.BlackRook:
                    if (MoveHistoryService.CanDoBlackSmallCastle() && move.From == Squares.H8 ||
                        MoveHistoryService.CanDoBlackBigCastle() && move.From == Squares.A8)
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
                    if (MoveHistoryService.CanDoWhiteSmallCastle() || MoveHistoryService.CanDoWhiteBigCastle())
                    {
                        move.Value =  move.Difference - 500;
                    }
                    else
                    {
                        SetKingMove(move, ply);
                    }

                    break;
                case Piece.BlackKing:
                    if (MoveHistoryService.CanDoBlackSmallCastle() || MoveHistoryService.CanDoBlackBigCastle())
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
                Position.Make(move);
                if (move.Piece.IsWhite())
                {
                }
                else
                {
                }

                if (balance < 0)
                {
                    return defaultValue - 600 - move.Piece.AsValue();
                }

                return defaultValue;
            }
            finally
            {
                Position.UnMake();
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
            move.Value = ply < 12 ? CalculateValue(move,  move.Difference - 150) : CalculateValue(move,move.Difference);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessAttack(IMove move, Dictionary<byte, int> attacksCache)
        {
            int value = move.Piece.AsValue();
            int victimValue = Position.GetPieceValue(move.To);

            if (value < victimValue)
            {
                move.Value =  move.Difference + 400 - value;
            }
            else
            {
                if (!attacksCache.TryGetValue(move.To.AsByte(), out var balance))
                {
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