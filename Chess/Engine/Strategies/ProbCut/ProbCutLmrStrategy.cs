using System;
using CommonServiceLocator;
using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Models.Enums;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;
using Engine.Strategies.LateMove.Base;
using Engine.Strategies.Models;

namespace Engine.Strategies.ProbCut
{
    public sealed class ProbCutLmrStrategy : LmrStrategyBase
    {
        private readonly ProbCutModel[] _models;

        public ProbCutLmrStrategy(short depth, IPosition position, TranspositionTable table = null)
            : base(depth, position, table)
        {
            var probCutModelProvider = ServiceLocator.Current.GetInstance<IProbCutModelProvider>();
            _models = probCutModelProvider.CreateModels(depth);

            InitializeSorters(depth, position, MoveSorterProvider.GetExtended(position, new HistoryComparer()));
        }

        public override int Search(int alpha, int beta, int depth)
        {
            if (depth == 0)
            {
                return Evaluate(alpha, beta);
            }

            if (Position.GetPhase() == Phase.End)
            {
                return EndGameStrategy.Search(alpha, beta, Math.Min(depth + 1, MaxEndGameDepth));
            }

            var probCutModel = _models[depth];
            if (probCutModel.CanReduce && Math.Abs(alpha - beta) > 1)
            {
                double percentile = probCutModel.Percentile;
                double sigma = probCutModel.Sigma;
                double b = probCutModel.B;
                double a = probCutModel.A;
                var d = probCutModel.Depth;

                int betaBound = (int)(Math.Round(percentile * sigma + beta - b) / a);
                var x = Search(betaBound - 1, betaBound, d);
                if (x >= betaBound)
                {
                    return beta;
                }

                //int alphaBound = (int) (Math.Round(-percentile * sigma + alpha - b) / a);
                //var z = base.Search(alphaBound, alphaBound + 1, d);
                //if (z <= alphaBound)
                //{
                //    return alpha;
                //}
            }

            MoveBase pv = null;
            var key = Position.GetKey();
            bool shouldUpdate = false;
            bool isInTable = false;

            if (Table.TryGet(key, out var entry))
            {
                isInTable = true;
                var entryDepth = entry.Depth;
                if (entryDepth >= depth)
                {
                    if (entry.Value > alpha)
                    {
                        alpha = entry.Value;
                    }
                    if (alpha >= beta)
                        return entry.Value;
                }
                else
                {
                    shouldUpdate = Position.GetPhase() != Phase.End;
                }
                pv = GetPv(entry.PvMove);
            }

            var moves = IsFutility(alpha, depth)
                ? Position.GetAllAttacks(Sorters[depth])
                : Position.GetAllMoves(Sorters[depth], pv);

            int value = int.MinValue;
            MoveBase bestMove = null;

            var count = moves.Length;
            if (CheckPosition(count, out var defaultValue)) return defaultValue;

            if (depth > DepthReduction + 1 && !MoveHistory.IsLastMoveWasCheck() && Math.Abs(alpha - beta) > 1)
            {
                for (var i = 0; i < count; i++)
                {
                    var move = moves[i];
                    Position.Make(move);

                    int r;
                    if (i > LmrDepthThreshold && move.CanReduce && !move.IsCheck)
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
                    Sorters[depth].Add(move.Key);
                    break;
                }
            }
            else
            {
                for (var i = 0; i < count; i++)
                {
                    var move = moves[i];
                    Position.Make(move);

                    var r = -Search(-beta, -alpha, depth - 1);

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
                    Sorters[depth].Add(move.Key);
                    break;
                }
            }

            bestMove.History += 1 << depth;

            if (isInTable && !shouldUpdate) return value;

            return StoreValue((byte)depth, (short)value, bestMove.Key);
        }
    }
}