using System;
using CommonServiceLocator;
using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Enums;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;
using Engine.Strategies.LateMove.Base;
using Engine.Strategies.Models;

namespace Engine.Strategies.ProbCut
{
    public sealed class ProbCutLmrDeepStrategy : LmrStrategyBase
    {
        private readonly ProbCutModel[] _models;
        private readonly int LmrLateDepthThreshold;

        public ProbCutLmrDeepStrategy(short depth, IPosition position, TranspositionTable table = null)
            : base(depth, position, table)
        {
            LmrLateDepthThreshold = ServiceLocator.Current.GetInstance<IConfigurationProvider>()
                .AlgorithmConfiguration.LateMoveConfiguration.LmrLateDepthThreshold;

            var probCutModelProvider = ServiceLocator.Current.GetInstance<IProbCutModelProvider>();
            _models = probCutModelProvider.CreateModels(depth);

            InitializeSorters(depth, position, MoveSorterProvider.GetExtended(position, new HistoryComparer()));
        }

        #region Overrides of StrategyBase

        protected override void InitializeSorters(short depth, IPosition position, MoveSorter mainSorter)
        {
            Sorters = new IMoveSorter[depth + 2];

            var initialSorter = MoveSorterProvider.GetInitial(position, Sorting.Sort.HistoryComparer);
            Sorters[0] = MoveSorterProvider.GetBasic(position, Sorting.Sort.HistoryComparer);

            var d = Math.Min(depth - SortDepth[depth], depth/2);

            if (UseSortHard)
            {
                var hardExtended = MoveSorterProvider.GetHardExtended(position, Sorting.Sort.HistoryComparer);
                var hard = d - SortHardDepth[depth];
                for (int i = 1; i < hard; i++)
                {
                    Sorters[i] = mainSorter;
                }

                for (var i = hard; i < d; i++)
                {
                    Sorters[i] = hardExtended;
                }
            }
            else if (UseSortDifference)
            {
                var differenceExtended =
                    MoveSorterProvider.GetDifferenceExtended(position, Sorting.Sort.HistoryComparer);
                var x = 1 + SortDifferenceDepth[depth];
                for (int i = 1; i < x; i++)
                {
                    Sorters[i] = differenceExtended;
                }

                for (int i = x; i < d; i++)
                {
                    Sorters[i] = mainSorter;
                }
            }
            else
            {
                for (int i = 1; i < d; i++)
                {
                    Sorters[i] = mainSorter;
                }
            }


            for (var i = d; i < Sorters.Length; i++)
            {
                Sorters[i] = initialSorter;
            }
        }

        #endregion

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

                var round = Math.Round((percentile * sigma + beta - b) / a);
                int betaBound = (int)round;
                var x = Search(betaBound - 1, betaBound, d);
                if (x >= betaBound)
                {
                    return beta;
                }

                //var r = Math.Round((-percentile * sigma + alpha - b) / a);
                //int alphaBound = (int)r;
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
                    shouldUpdate = true;
                }

                pv = GetPv(entry.PvMove);
            }

            var moves = IsFutility(alpha, depth)
                ? Position.GetAllAttacks(Sorters[depth])
                : Position.GetAllMoves(Sorters[depth], pv);

            var count = moves.Length;
            if (CheckPosition(count, out var defaultValue)) return defaultValue;

            int value = int.MinValue;
            MoveBase bestMove = null;

            if (depth < DepthLateReduction || MoveHistory.IsLastMoveNotReducible() || Math.Abs(alpha - beta) <= 1)
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

                    if(!move.IsAttack)Sorters[depth].Add(move.Key);
                    break;
                }
            }
            else if (depth == DepthLateReduction)
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

                    if(!move.IsAttack)Sorters[depth].Add(move.Key);
                    break;
                }
            }
            else
            {
                for (var i = 0; i < count; i++)
                {
                    var move = moves[i];
                    Position.Make(move);

                    int r;
                    if (i > LmrDepthThreshold && move.CanReduce && !move.IsCheck)
                    {
                        if (i > LmrLateDepthThreshold)
                        {
                            r = -Search(-beta, -alpha, depth - DepthLateReduction);
                        }
                        else
                        {
                            r = -Search(-beta, -alpha, depth - DepthReduction);
                        }

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

                    if(!move.IsAttack)Sorters[depth].Add(move.Key);
                    break;
                }
            }

            bestMove.History += 1 << depth;

            if (isInTable && !shouldUpdate) return value;

            return StoreValue((byte)depth, (short)value, bestMove.Key);
        }
    }
}