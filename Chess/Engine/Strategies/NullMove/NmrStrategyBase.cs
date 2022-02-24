using CommonServiceLocator;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Enums;
using Engine.Models.Moves;
using Engine.Models.Transposition;
using Engine.Strategies.AlphaBeta.Null;

namespace Engine.Strategies.NullMove
{
    public abstract class NmrStrategyBase: NullMoveStrategy
    {
        protected int NullDepthReduction;

        protected NmrStrategyBase(short depth, IPosition position) : base(depth, position)
        {
            MinReduction = 3;
            MaxReduction = 4;
            NullDepthReduction = ServiceLocator.Current.GetInstance<IConfigurationProvider>()
                .AlgorithmConfiguration.NullConfiguration.NullDepthReduction;
        }

        public override int Search(int alpha, int beta, int depth)
        {
            if (depth <= 0)
            {
                return Evaluate(alpha, beta);
            }

            MoveBase pv = null;
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
                    pv = MoveProvider.Get(entry.PvMove);
                }
            }

            int value = short.MinValue;
            MoveBase bestMove = null;

            var moves = GenerateMoves(alpha, beta, depth, pv);
            if (moves == null) return alpha;

            if (CheckMoves(alpha, beta, moves, out var defaultValue)) return defaultValue;

            int d = depth;
            var lastMove = MoveHistory.GetLastMove();
            if (CanUseNull && !lastMove.IsCheck && isNotEndGame && IsValidWindow(alpha, beta))
            {
                int r = depth > 6 ? MaxReduction : MinReduction;
                if (depth > r)
                {
                    MakeNullMove();
                    var v = -Search(-beta, NullWindow - beta, depth - r - 1);
                    UndoNullMove();
                    if (v >= beta)
                    {
                        d = depth - NullDepthReduction;
                        if (d <= 0)
                        {
                            return Evaluate(alpha, beta);
                        }
                    }
                }
            }

            for (var i = 0; i < moves.Length; i++)
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

            bestMove.History += 1 << d;

            if (isInTable && !shouldUpdate) return value;

            return StoreValue(alpha, beta, depth, value, bestMove);
        }
    }
}
