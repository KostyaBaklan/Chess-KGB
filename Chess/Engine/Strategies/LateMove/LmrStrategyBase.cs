using System.Runtime.CompilerServices;
using CommonServiceLocator;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Enums;
using Engine.Models.Transposition;
using Engine.Strategies.AlphaBeta.Simple;

namespace Engine.Strategies.LateMove
{
    public abstract class LmrStrategyBase: AlphaBetaStrategy
    {
        protected int DepthReduction;

        protected LmrStrategyBase(short depth, IPosition position) : base(depth, position)
        {
            DepthReduction = ServiceLocator.Current.GetInstance<IConfigurationProvider>()
                    .AlgorithmConfiguration.DepthReduction;
        }

        public override int Search(int alpha, int beta, int depth)
        {
            if (depth == 0)
            {
                return Evaluate(alpha, beta);
            }

            IMove pv = null;
            var key = Position.GetKey();
            bool shouldUpdate = false;
            bool isInTable = false;

            var isNotEndGame = Position.GetPhase() != Phase.End;
            if (isNotEndGame && Table.TryGet(key, out var entry))
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

            int value = int.MinValue;
            IMove bestMove = null;

            IMoveCollection moves = GenerateMoves(alpha, beta, depth, pv);
            if (moves == null) return alpha;

            if (CheckMoves(alpha, beta, moves, out var defaultValue)) return defaultValue;

            for (var i = 0; i < moves.Count; i++)
            {
                var move = moves[i];

                Position.Make(move);

                int r;
                if (depth > DepthReduction && moves.IsLmr(i) && CanReduce(move))
                {
                    r = -Search(-beta, -alpha, depth - DepthReduction);
                    if (r > alpha)
                    {
                        r = -Search(-beta, -alpha, depth - 1);
                    }
                }
                else
                {
                    r = -Search(-beta, -alpha, depth - 1);
                }

                if (r > value)
                {
                    value = r;
                    bestMove = move;
                }

                Position.UnMake();

                if (value > alpha)
                {
                    alpha = value;
                }

                if (alpha < beta) continue;

                Sorter.Add(move);
                break;
            }

            bestMove.History += 1 << depth;

            if (!isNotEndGame) return value;

            if (isInTable && !shouldUpdate) return value;

            return StoreValue(alpha, beta, depth, value, bestMove);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool CanReduce(IMove move)
        {
            if (move.IsAttack() || move.IsPromotion() || move.IsCheck())
            {
                return false;
            }

            return !MoveHistory.GetLastMove().IsCheck();
        }
    }
}
