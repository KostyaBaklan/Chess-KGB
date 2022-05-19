using System.Runtime.CompilerServices;
using CommonServiceLocator;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Enums;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;
using Engine.Strategies.Base;

namespace Engine.Strategies.End
{
    public class LmrDeepNoCacheStrategy : StrategyBase
    {
        protected int LmrLateDepthThreshold;
        protected int DepthReduction;
        protected int LmrDepthThreshold;

        public LmrDeepNoCacheStrategy(short depth, IPosition position) : base(depth, position)
        {
            var configurationProvider = ServiceLocator.Current.GetInstance<IConfigurationProvider>();
            LmrDepthThreshold = configurationProvider
                .AlgorithmConfiguration.LateMoveConfiguration.LmrDepthThreshold;
            DepthReduction = configurationProvider
                .AlgorithmConfiguration.LateMoveConfiguration.LmrDepthReduction;
            LmrLateDepthThreshold = configurationProvider
                .AlgorithmConfiguration.LateMoveConfiguration.LmrLateDepthThreshold;

            InitializeSorters(depth, position, new ExtendedSorter(position, new HistoryComparer()));
        }

        #region Overrides of StrategyBase

        public override IResult GetResult()
        {
            return GetResult(-SearchValue, SearchValue, Depth);
        }

        public override IResult GetResult(int alpha, int beta, int depth, MoveBase pvMove = null)
        {
            ResetSorterFlags();
            Result result = new Result();

            var moves = Position.GetAllMoves(Sorters[Depth]);

            if (CheckMoves(moves, out var res)) return res;

            if (moves.Length > 1)
            {
                var isCheck = MoveHistory.GetLastMove().IsCheck;
                if (isCheck)
                {
                    for (var i = 0; i < moves.Length; i++)
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
                    for (var i = 0; i < moves.Length; i++)
                    {
                        var move = moves[i];
                        Position.Make(move);

                        int value;
                        if (alpha > -SearchValue && IsLmr(i) && CanReduce(move))
                        {
                            var reduction = Position.GetPhase() != Phase.End && i > LmrLateDepthThreshold ? DepthReduction + 1 : DepthReduction;
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

            int value = int.MinValue;
            MoveBase bestMove = null;

            var moves = GetMoves(alpha, beta, depth);
            if (moves == null) return alpha;

            if (CheckMoves(alpha, beta, moves, out var defaultValue)) return defaultValue;

            if (!MoveHistory.GetLastMove().IsCheck && depth > DepthReduction + 1)
            {
                for (var i = 0; i < moves.Length; i++)
                {
                    var move = moves[i];
                    Position.Make(move);

                    int r;
                    if (IsLmr(i) && CanReduce(move))
                    {
                        var reduction = Position.GetPhase() != Phase.End && i > LmrLateDepthThreshold ? DepthReduction + 1 : DepthReduction;
                        r = -Search(-beta, -alpha, depth - reduction);
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

                    Sorters[depth].Add(move);
                    break;
                }
            }
            else
            {
                for (var i = 0; i < moves.Length; i++)
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

                    Sorters[depth].Add(move);
                    break;
                }
            }

            bestMove.History += 1 << depth;
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool IsLmr(int i)
        {
            return i > LmrDepthThreshold;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool CanReduce(MoveBase move)
        {
            return !move.IsAttack && !move.IsPromotion && !move.IsCheck;
        }

        #endregion
    }
}
