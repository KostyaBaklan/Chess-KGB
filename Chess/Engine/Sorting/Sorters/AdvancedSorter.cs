using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CommonServiceLocator;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Enums;
using Engine.Models.Helpers;

namespace Engine.Sorting.Sorters
{
    public class AdvancedSorter : MoveSorter
    {
        private readonly IPosition _position;
        private readonly IHistoryHeuristic _historyHeuristic = ServiceLocator.Current.GetInstance<IHistoryHeuristic>();

        public AdvancedSorter(IPosition position)
        {
            _position = position;
        }

        #region Overrides of MoveSorter

        protected override IEnumerable<IMove> OrderInternal(IEnumerable<IMove> attacks, IEnumerable<IMove> moves, KillerMoveCollection killerMoveCollection)
        {
            throw new System.NotImplementedException();
        }

        protected override IEnumerable<IMove> OrderInternal(IEnumerable<IMove> attacks, IEnumerable<IMove> moves, KillerMoveCollection killerMoveCollection,
            IMove pvNode)
        {
            throw new System.NotImplementedException();
        }

        protected override IEnumerable<IMove> OrderInternal(IEnumerable<IMove> attacks, IEnumerable<IMove> moves, KillerMoveCollection killerMoveCollection,
            IMove pvNode, IMove cutMove)
        {
            throw new System.NotImplementedException();
        }

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
            var killer = pvNode.Equals(cutMove) ? collection.GetMoves() : collection.GetMoves(cutMove);

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
            foreach (var move in moves)
            {
                if (move.Equals(pvNode))
                {
                    move.Value = int.MaxValue - 100;
                }
                else if (move.IsPromotion())
                {
                    move.Value = int.MaxValue - 300;
                }
                else if (move.IsAttack())
                {
                    ProcessAttack(move);
                }
                else if (move.IsCastle())
                {
                    move.Value = _historyHeuristic.Get(move) + 200;
                }
                else
                {
                    if (MoveHistoryService.IsAdditionalDebutMove(move))
                    {
                        if (move.Piece == Piece.BlackPawn || move.Piece == Piece.BlackPawn)
                        {
                            move.Value = _historyHeuristic.Get(move) - 100;
                        }
                        else
                        {
                            move.Value = _historyHeuristic.Get(move) - 200;
                        }
                    }
                    if (killer.Contains(move))
                    {
                        move.Value = _historyHeuristic.Get(move) + 300;
                    }
                    else if (move.Type == MoveType.Over)
                    {
                        move.Value = _historyHeuristic.Get(move) - 200;
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
                        move.Value = _historyHeuristic.Get(move) - 400;
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
                        move.Value = _historyHeuristic.Get(move) - 400;
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
                        move.Value = _historyHeuristic.Get(move) - 500;
                    }
                    else
                    {
                        SetKingMove(move, ply);
                    }

                    break;
                case Piece.BlackKing:
                    if (MoveHistoryService.CanDoBlackSmallCastle() || MoveHistoryService.CanDoBlackBigCastle())
                    {
                        move.Value = _historyHeuristic.Get(move) - 500;
                    }
                    else
                    {
                        SetKingMove(move, ply);
                    }

                    break;
                default:
                    move.Value = _historyHeuristic.Get(move);
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetKingMove(IMove move, int ply)
        {
            if (ply < 12)
            {
                move.Value = _historyHeuristic.Get(move) - 400;
            }
            else if (ply < 20)
            {
                move.Value = _historyHeuristic.Get(move) - 300;
            }
            else
            {
                move.Value = _historyHeuristic.Get(move);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetHardPieceMove(IMove move, int ply)
        {
            move.Value = ply < 12 ? _historyHeuristic.Get(move) - 150 : _historyHeuristic.Get(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessAttack(IMove move)
        {
            int value = move.Piece.AsValue();
            int victimValue = _position.GetPieceValue(move.To);

            if (value < victimValue)
            {
                move.Value = int.MaxValue - 500 - value;
            }
            else
            {
                move.Value = int.MaxValue - 700 - value;
            }
        }

        #endregion
    }
}