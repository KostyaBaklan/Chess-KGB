using Algorithms.Models;
using Algorithms.Strategies.Base;
using Infrastructure.Interfaces.Moves;
using Infrastructure.Interfaces.Position;

namespace Algorithms.Strategies.Pvs
{
    public abstract class PvsStrategyBase : AlphaBetaStrategyBase
    {
        protected PvsStrategyBase(short depth, IMoveSorter sorter, IPosition position) : base(depth, sorter, position)
        {
        }

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

            bool isFirst = true;
            int value = int.MinValue;
            IMove bestMove = null;
            IMove cutMove = null;
            var moves = Position.GetAllMoves(Sorter, pv, cut);

            foreach (var move in moves)
            {
                Position.Make(move);

                var isCheck = Position.IsCheck();

                if (!isCheck)
                {
                    int score;
                    if (isFirst)
                    {
                        score = -Search(-beta, -alpha, depth - 1);
                        isFirst = false;
                    }
                    else
                    {
                        score = -Search(-alpha - 125, -alpha, depth - 1);
                        if (alpha < score && score < beta)
                        {
                            score = -Search(-beta, -score, depth - 1);
                        }
                    }
                    if (score > value)
                    {
                        value = score;
                        bestMove = move;
                    }
                }

                Position.UnMake();

                if (isCheck) continue;

                if (value > alpha)
                {
                    alpha = value;
                }

                if (alpha < beta)  continue;

                cutMove = move;
                OnCutOff(move, depth);
                break;
            }

            var best = value == int.MinValue ? short.MinValue : value;

            TranspositionEntry te = new TranspositionEntry { Depth = depth, Value = best, PvMove = bestMove, CutMove = cutMove };
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
    }
}
