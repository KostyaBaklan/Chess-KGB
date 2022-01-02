using Algorithms.DataStructures;
using Algorithms.Interfaces;
using Algorithms.Models;
using Infrastructure.Interfaces.Moves;
using Infrastructure.Interfaces.Position;

namespace Algorithms.Strategies.Base
{
    public abstract class AlphaBetaStrategyBase : StrategyBase
    {
        protected readonly TranspositionTable Table;

        protected AlphaBetaStrategyBase(short depth, IPosition position) : base(depth, position)
        {
            int capacity;
            if (depth < 6)
            {
                capacity = 1131467;
            }
            else if (depth == 6)
            {
                capacity = 2263139;
            }
            else if (depth == 7)
            {
                capacity = 4356733;
            }
            else
            {
                capacity = 7910731;
            }
            Table = new TranspositionTable(capacity);
        }

        protected AlphaBetaStrategyBase(short depth, IMoveSorter sorter, IPosition position) : base(depth, sorter, position)
        {
            int capacity;
            if (depth < 6)
            {
                capacity = 1131467;
            }
            else if (depth == 6)
            {
                capacity = 2263139;
            }
            else if (depth == 7)
            {
                capacity = 4356733;
            }
            else
            {
                capacity = 7910731;
            }
            Table = new TranspositionTable(capacity);
        }

        public override int Size => Table.Count;

        public override IResult GetResult()
        {
            return GetResult(-1000000, 1000000, Depth);
        }

        #region Overrides of StrategyBase

        public override IResult GetResult(int alpha, int beta, int depth, IMove pvMove = null, IMove cutMove = null)
        {
            Result result = new Result();

            IMove pv = pvMove, cut = cutMove;
            if (Table.TryGet(Position.Key, out var entry))
            {
                pv = entry.PvMove;
                cut = cutMove;
            }

            var moves = Position.GetAllMoves(Sorter, pv, cut);
            foreach (var move in moves)
            {
                try
                {
                    Position.Make(move);

                    var isCheck = Position.IsCheck();

                    if (isCheck) continue;

                    var value = -Search(-beta, -alpha, depth - 1);
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

                    result.Cut = move;
                    break;
                }
                finally
                {
                    Position.UnMake();
                }
            }

            return result;
        }

        #endregion

        public override int Search(int alpha, int beta, int depth)
        {
            if (depth == 0)
            {
                return Evaluate(alpha: alpha, beta: beta);
            }

            IMove pv = null;
            IMove cut = null;
            if (Table.TryGet(Position.Key, out var entry))
            {
                if (entry.Depth >= depth)
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

                pv = entry.PvMove;
                cut = entry.CutMove;
            }

            int value = int.MinValue;
            IMove bestMove = null;
            IMove cutMove = null;
            var moves = Position.GetAllMoves(Sorter, pv,cut);

            foreach (var move in moves)
            {
                Position.Make(move);

                var isCheck = Position.IsCheck();

                if (!isCheck)
                {
                    var r = -Search(-beta, -alpha, depth - 1);
                    if (r > value)
                    {
                        value = r;
                        bestMove=move;
                    }
                }

                Position.UnMake();

                if (isCheck) continue;

                if (value > alpha)
                {
                    alpha = value;
                }

                if (alpha < beta) continue;

                cutMove = move;
                OnCutOff(move, depth);
                break;
            }

            var best = value == int.MinValue ? short.MinValue : value;

            TranspositionEntry te = new TranspositionEntry { Depth = depth, Value = best, PvMove = bestMove, CutMove = cutMove};
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

            Table.Add(Position.Key, te);
            return best;
        }

        public void Clear()
        {
           Table.Clear();
        }
    }
}
