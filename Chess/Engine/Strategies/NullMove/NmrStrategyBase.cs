using CommonServiceLocator;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Enums;
using Engine.Models.Transposition;
using Engine.Strategies.AlphaBeta.Null;

namespace Engine.Strategies.NullMove
{
    public abstract class NmrStrategyBase: NullMoveStrategy
    {
        protected int NullDepthReduction;

        protected NmrStrategyBase(short depth, IPosition position) : base(depth, position)
        {
            NullDepthReduction = ServiceLocator.Current.GetInstance<IConfigurationProvider>()
                .AlgorithmConfiguration.NullDepthReduction;
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

            int d = depth;
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
                        var nullDepthReduction = NullDepthReduction;
                        if (depth > 6)
                        {
                            nullDepthReduction++;
                        }
                        d = depth - nullDepthReduction;
                        if (d <= 0)
                        {
                            return Evaluate(alpha, beta);
                        }
                    }
                }
            }

            for (var i = 0; i < moves.Count; i++)
            {
                var move = moves[i];

                Position.Make(move);
                var r = -Search(-beta, -alpha, d - 1);
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
                bestMove.History += 1 << d;
                best = value;
            }

            if (!isInTable || shouldUpdate)
            {
                TranspositionEntry te = new TranspositionEntry
                { Depth = (byte)d, Value = (short)best, PvMove = bestMove };
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
    }
}
