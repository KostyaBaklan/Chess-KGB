using System.Collections.Generic;
using CommonServiceLocator;
using Infrastructure.Helpers;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.Moves;
using Infrastructure.Models.Enums;

namespace Infrastructure.Sorters.Pv
{
    public abstract class PvFigureMoveSorterBase: PvMoveSorter
    {
        private readonly IMoveHistoryService _moveHistory;

        protected PvFigureMoveSorterBase()
        {
            _moveHistory = ServiceLocator.Current.GetInstance<IMoveHistoryService>();
        }

        #region Overrides of PvMoveSorter

        protected override IEnumerable<IMove> OrderInternal(IEnumerable<IMove> moves, IKillerMoveCollection collection, IMove pvNode, IMove cutMove)
        {
            var lightMoves = new List<IMove>(32);
            var hardMoves = new List<IMove>(32);
            var badMoves = new List<IMove>();

            var killerMoves = new Queue<IMove>(4);
            var attacks = new Queue<IMove>(12);
            var killer = collection.GetMoves(cutMove);

            foreach (var move in moves)
            {
                if (pvNode.Equals(move))
                {
                    yield return move;
                }
                if (killer.Contains(move))
                {
                    killerMoves.Enqueue(move);
                }
                else if (move.IsCastle())
                {
                    lightMoves.Add(move);
                }
                else if (move.Operation == MoveOperation.Over)
                {
                    hardMoves.Add(move);
                }
                else if (move.Type == MoveType.Move || move.Type == MoveType.PawnOverMove)
                {
                    SetMoveClassification(move, badMoves, hardMoves, lightMoves);
                }
                else
                {
                    attacks.Enqueue(move);
                }
            }

            foreach (var move in killerMoves)
            {
                yield return move;
            }

            foreach (var move in attacks)
            {
                yield return move;
            }

            foreach (var m in OrderMoves(lightMoves, hardMoves, badMoves)) yield return m;
        }

        protected override IEnumerable<IMove> OrderInternal(IEnumerable<IMove> moves, IKillerMoveCollection collection, IMove cutMove)
        {
            var killer = collection.GetMoves(cutMove);

            return Order(moves, killer);
        }

        protected override IEnumerable<IMove> OrderInternal(IEnumerable<IMove> moves, IKillerMoveCollection collection)
        {
            var killer = collection.GetMoves();

            return Order(moves, killer);
        }

        #endregion

        private IEnumerable<IMove> Order(IEnumerable<IMove> moves, ICollection<IMove> killer)
        {
            var lightMoves = new List<IMove>(32);
            var hardMoves = new List<IMove>(32);
            var badMoves = new List<IMove>();

            foreach (var move in moves)
            {
                if (killer.Contains(move))
                {
                    yield return move;
                }
                else if (move.IsCastle())
                {
                    lightMoves.Add(move);
                }
                else if (move.Operation == MoveOperation.Over)
                {
                    hardMoves.Add(move);
                }
                else if (move.Type == MoveType.Move|| move.Type == MoveType.PawnOverMove)
                {
                    SetMoveClassification(move, badMoves, hardMoves, lightMoves);
                }
                else
                {
                    yield return move;
                }
            }

            foreach (var m in OrderMoves(lightMoves, hardMoves, badMoves)) yield return m;
        }

        private IEnumerable<IMove> OrderMoves(List<IMove> lightMoves, List<IMove> hardMoves, List<IMove> badMoves)
        {
            lightMoves.Sort(Comparer);
            foreach (var m in lightMoves)
            {
                yield return m;
            }

            hardMoves.Sort(Comparer);
            foreach (var m in hardMoves)
            {
                yield return m;
            }

            foreach (var m in badMoves)
            {
                yield return m;
            }
        }

        private void SetMoveClassification(IMove move, List<IMove> badMoves, List<IMove> hardMoves, List<IMove> lightMoves)
        {
            if (_moveHistory.IsAdditionalDebutMove(move))
            {
                if (move.Figure == FigureKind.BlackPawn || move.Figure == FigureKind.BlackPawn)
                {
                    hardMoves.Add(move);
                }
                else
                {
                    badMoves.Add(move);
                }
                return;
            }
            var lastMove = _moveHistory.GetLastMove();
            var ply = _moveHistory.Ply;
            switch (move.Figure)
            {
                case FigureKind.WhiteRook:
                    if (_moveHistory.CanDoWhiteSmallCastle && move.From.Key == 56||_moveHistory.CanDoWhiteBigCastle && move.From.Key == 0)
                    {
                        badMoves.Add(move);
                    }
                    else
                    {
                        SetHardMove(move, hardMoves, lightMoves, ply);
                    }

                    break;
                case FigureKind.BlackRook:
                    if (_moveHistory.CanDoBlackSmallCastle && move.From.Key == 63 || _moveHistory.CanDoBlackBigCastle && move.From.Key == 7)
                    {
                        badMoves.Add(move);
                    }
                    else
                    {
                        SetHardMove(move, hardMoves, lightMoves, ply);
                    }

                    break;
                case FigureKind.WhiteQueen:
                case FigureKind.BlackQueen:
                    SetHardMove(move, hardMoves, lightMoves, ply);
                    break;
                case FigureKind.WhiteKing:
                    if (_moveHistory.CanDoWhiteSmallCastle || _moveHistory.CanDoWhiteBigCastle)
                    {
                        badMoves.Add(move);
                    }
                    else
                    {
                        SetHardMove(move, hardMoves, lightMoves, lastMove);
                    }

                    break;
                case FigureKind.BlackKing:
                    if (_moveHistory.CanDoBlackBigCastle || _moveHistory.CanDoBlackSmallCastle)
                    {
                        badMoves.Add(move);
                    }
                    else
                    {
                        SetHardMove(move, hardMoves, lightMoves, lastMove);
                    }

                    break;
                default:
                    lightMoves.Add(move);
                    break;
            }
        }

        private static void SetHardMove(IMove move, List<IMove> hardMoves, List<IMove> lightMoves, IMove lastMove)
        {
            if (lastMove == null || lastMove.Result != MoveResult.Check)
            {
                hardMoves.Add(move);
            }
            else
            {
                lightMoves.Add(move);
            }
        }

        private static void SetHardMove(IMove move, List<IMove> hardMoves, List<IMove> lightMoves, int ply)
        {
            if (ply < 12)
            {
                hardMoves.Add(move);
            }
            else
            {
                lightMoves.Add(move);
            }
        }
    }
}
