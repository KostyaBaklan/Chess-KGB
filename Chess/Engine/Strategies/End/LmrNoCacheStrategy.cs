using CommonServiceLocator;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;
using Engine.Strategies.Base;

namespace Engine.Strategies.End
{
    public class LmrNoCacheStrategy : StrategyBase
    {
        protected int DepthReduction;
        protected int DepthLateReduction;
        protected int LmrDepthThreshold;
        protected int[] LmrLateDepthThreshold;
        protected int LmrDepthLimitForReduce;

        public LmrNoCacheStrategy(short depth, IPosition position) : base(depth, position)
        {
            var configurationProvider = ServiceLocator.Current.GetInstance<IConfigurationProvider>();
            LmrDepthThreshold = configurationProvider
                .AlgorithmConfiguration.LateMoveConfiguration.LmrDepthThreshold;
            DepthReduction = configurationProvider
                .AlgorithmConfiguration.LateMoveConfiguration.LmrDepthReduction;
            LmrLateDepthThreshold = configurationProvider
                .AlgorithmConfiguration.LateMoveConfiguration.LmrLateDepthThreshold;

            LmrDepthLimitForReduce = DepthReduction + 2;
            DepthLateReduction = DepthReduction + 1;

            InitializeSorters(depth, position, MoveSorterProvider.GetExtended(position, new HistoryComparer()));
        }

        #region Overrides of StrategyBase

        public override IResult GetResult()
        {
            return GetResult(-SearchValue, SearchValue, Depth);
        }

        public override IResult GetResult(int alpha, int beta, int depth, MoveBase pvMove = null)
        {
            Result result = new Result();

            var moves = Position.GetAllMoves(Sorters[Depth], pvMove);

            var count = moves.Length;
            if (CheckMoves(count, out var res)) return res;

            if (count > 1)
            {
                if (MoveHistory.IsLastMoveNotReducible())
                {
                    for (var i = 0; i < count; i++)
                    {
                        var move = moves[i];
                        Position.Make(move);

                        var value = -Search(-beta, -alpha, depth - 1);

                        Position.UnMake();
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
                    var l = LmrLateDepthThreshold[depth];
                    for (var i = 0; i < count; i++)
                    {
                        var move = moves[i];
                        Position.Make(move);

                        int value;
                        if (i > LmrDepthThreshold && move.CanReduce && !move.IsCheck)
                        {
                            var reduction = i > l ? DepthLateReduction : DepthReduction;
                            value = -Search(-beta, -alpha, depth - reduction);
                            if (value > alpha)
                            {
                                value = -Search(-beta, -alpha, depth - 1);
                            }
                        }
                        else
                        {
                            value = -Search(-beta, -alpha, depth - 1);
                        }

                        Position.UnMake();
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

            var moves = IsFutility(alpha, depth)
                ? Position.GetAllAttacks(Sorters[depth])
                : Position.GetAllMoves(Sorters[depth]);

            var count = moves.Length;
            if (CheckPosition(count, out var defaultValue)) return defaultValue;

            int value = int.MinValue;
            MoveBase bestMove = null;

            if (depth < LmrDepthLimitForReduce || MoveHistory.IsLastMoveNotReducible())
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

                    if (!move.IsAttack) Sorters[depth].Add(move.Key);
                    break;
                }
            }
            else if (depth == LmrDepthLimitForReduce)
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

                    if (!move.IsAttack) Sorters[depth].Add(move.Key);
                    break;
                }
            }
            else
            {
                var l = LmrLateDepthThreshold[depth];
                for (var i = 0; i < count; i++)
                {
                    var move = moves[i];
                    Position.Make(move);

                    int r;
                    if (i > LmrDepthThreshold && move.CanReduce && !move.IsCheck)
                    {
                        if (i > l)
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

                    if (!move.IsAttack) Sorters[depth].Add(move.Key);
                    break;
                }
            }

            bestMove.History += 1 << depth;

            return value;
        }

        #endregion
    }
}
