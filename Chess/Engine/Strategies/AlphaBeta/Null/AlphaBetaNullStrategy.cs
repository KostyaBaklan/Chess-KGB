using System.Runtime.CompilerServices;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Enums;
using Engine.Models.Transposition;
using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;
using Engine.Strategies.AlphaBeta.Simple;

namespace Engine.Strategies.AlphaBeta.Null
{
    public abstract class AlphaBetaNullStrategy : AlphaBetaStrategy
    {
        protected bool CanUseNull;
        protected bool IsNull;
        protected int MinReduction;
        protected int MaxReduction;
        protected int NullWindow;

        protected AlphaBetaNullStrategy(short depth, IPosition position, IMoveComparer comparer) : base(depth, position)
        {
            CanUseNull = false;
            MinReduction = 2;
            MaxReduction = 3;
            NullWindow = EvaluationService.GetUnitValue();
            Sorter = new ExtendedSorter(position, comparer);
        }

        public override IResult GetResult(int alpha, int beta, int depth, IMove pvMove = null)
        {
            CanUseNull = false;
            Result result = new Result();

            IMove pv = pvMove;

            var isNotEndGame = Position.GetPhase() != Phase.End;
            var key = Position.GetKey();
            if (isNotEndGame && Table.TryGet(key, out var entry))
            {
                if ((entry.Depth - depth) % 2 == 0)
                {
                    pv = entry.PvMove;
                }
            }

            var moves = Position.GetAllMoves(Sorter, pv);
            if (moves.Count == 0)
            {
                result.GameResult = MoveHistory.GetLastMove().IsCheck() ? GameResult.Mate : GameResult.Pat;
                return result;
            }

            if (MoveHistory.IsThreefoldRepetition(key))
            {
                var v = Evaluate(alpha, beta);
                if (v < -500)
                {
                    result.GameResult = GameResult.ThreefoldRepetition;
                    return result;
                }
            }

            if (moves.Count > 1)
            {
                for (var i = 0; i < moves.Count; i++)
                {
                    IsNull = false;
                    var move = moves[i];
                    SwitchNull();
                    Position.Make(move);

                    var value = -Search(-beta, -alpha, depth - 1);

                    Position.UnMake();
                    SwitchNull();

                    if (value > result.Value)
                    {
                        result.Value = value;
                        result.Move = move;
                    }

                    if (value > alpha)
                    {
                        alpha = value;
                    }

                    if (alpha < beta) continue;
                    break;
                }
            }
            else
            {
                result.Move = moves[0];
            }

            result.Move.History++;

            return result;
        }

        public override int Search(int alpha, int beta, int depth)
        {
            if (depth <= 0)
            {
                return Evaluate(alpha, beta);
            }

            IMove pv = null;
            var key = Position.GetKey();

            var isNotEndGame = Position.GetPhase() != Phase.End;

            bool shouldUpdate = false;
            bool isInTable = false;

            if (!IsNull && isNotEndGame && Table.TryGet(key, out var entry))
            {
                isInTable = true;
                var entryDepth = entry.Depth;
                if (entryDepth >= depth)
                {
                    if (entry.Type == TranspositionEntryType.Exact)
                    {
                        return entry.Value;
                    }

                    if (entry.Type == TranspositionEntryType.LowerBound && entry.Value > alpha)
                    {
                        alpha = entry.Value;
                    }
                    else if (entry.Type == TranspositionEntryType.UpperBound && entry.Value < beta)
                    {
                        beta = entry.Value;
                    }

                    if (alpha >= beta)
                        return entry.Value;
                }
                else
                {
                    shouldUpdate = true;
                }

                if ((entryDepth - depth) % 2 == 0)
                {
                    pv = entry.PvMove;
                }
            }

            int value = short.MinValue;
            IMove bestMove = null;

            var lastMove = MoveHistory.GetLastMove();
            var moves = Position.GetAllMoves(Sorter, pv);
            if (moves.Count == 0)
            {
                return lastMove.IsCheck()
                    ? -EvaluationService.GetMateValue()
                    : -EvaluationService.Evaluate(Position);
            }

            if (MoveHistory.IsThreefoldRepetition(key))
            {
                var v = Evaluate(alpha, beta);
                if (v < 0)
                {
                    return -v;
                }
            }

            if (CanUseNull && !lastMove.IsCheck() && isNotEndGame && IsValidWindow(alpha, beta))
            {
                int r = depth > 6 ? MaxReduction : MinReduction;
                if (depth > r)
                {
                    MakeNullMove();
                    var v = -Search(-beta, NullWindow - beta, depth - r - 1);
                    UndoNullMove();
                    if (v >= beta)
                    {
                        return v;
                    }
                }
            }

            for (var i = 0; i < moves.Count; i++)
            {
                var move = moves[i];

                Position.Make(move);
                var r = -Search(-beta, -alpha, depth - 1);
                Position.UnMake();

                if (r > value)
                {
                    value = r;
                    bestMove = move;
                }

                if (value > alpha)
                {
                    alpha = value;
                }

                if (alpha < beta) continue;

                Sorter.Add(move);
                break;
            }

            if (IsNull || !isNotEndGame) return value;

            int best;
            if (bestMove == null)
            {
                best = short.MinValue;
            }
            else
            {
                bestMove.History += 1 << depth;
                best = value;
            }

            if (!isInTable || shouldUpdate)
            {
                TranspositionEntry te = new TranspositionEntry
                    { Depth = (byte)depth, Value = (short)best, PvMove = bestMove };
                if (best <= alpha)
                {
                    te.Type = TranspositionEntryType.LowerBound;
                }
                else if (best >= beta)
                {
                    te.Type = TranspositionEntryType.UpperBound;
                }
                else
                {
                    te.Type = TranspositionEntryType.Exact;
                }

                Table.Set(key, te);

                return best;
            }

            return best;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsValidWindow(int alpha, int beta)
        {
            return beta < SearchValue && beta - alpha > NullWindow;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void UndoNullMove()
        {
            SwitchNull();
            Position.SwapTurn();
            IsNull = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MakeNullMove()
        {
            SwitchNull();
            Position.SwapTurn();
            IsNull = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void SwitchNull()
        {
            CanUseNull = !CanUseNull;
        }
    }
}
