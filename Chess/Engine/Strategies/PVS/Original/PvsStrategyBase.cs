using CommonServiceLocator;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Enums;
using Engine.Strategies.AlphaBeta.Extended;
using Engine.Strategies.Base;

namespace Engine.Strategies.PVS.Original
{
    public abstract class PvsStrategyBase : ComplexStrategyBase
    {
        protected int NonPvIterations;
        protected int DepthOffset;
        protected int NullWindow;

        protected PvsStrategyBase(short depth, IPosition position) : base(depth, position)
        {
            var configurationProvider = ServiceLocator.Current.GetInstance<IConfigurationProvider>();
            NonPvIterations = configurationProvider
                .AlgorithmConfiguration.NonPvIterations;
            DepthOffset = configurationProvider
                .AlgorithmConfiguration.DepthOffset;
            NullWindow = configurationProvider
                .AlgorithmConfiguration.NullWindow;
            InternalStrategy = new AlphaBetaExtendedHistoryStrategy(depth, position);
        }

        public override IResult GetResult()
        {
            var depth = Depth;
            if (Position.GetPhase() == Phase.End)
            {
                depth++;
            }

            var depthOffset = depth - DepthOffset;
            var result = InternalStrategy.GetResult(-SearchValue, SearchValue, depthOffset);
            if (result.GameResult == GameResult.Continue)
            {
                for (int d = depthOffset + 1; d <= depth; d++)
                {
                    result = GetResult(-SearchValue, SearchValue, d, result.Move);
                    if (result.GameResult != GameResult.Continue) break;
                }
            }

            return result;
        }

        #region Overrides of ComplexStrategyBase

        public override IResult GetResult(int alpha, int beta, int depth, IMove pvMove = null)
        {
            Result result = new Result();

            var moves = Position.GetAllMoves(Sorter, pvMove);

            if (CheckMoves(moves, out var res)) return res;

            if (moves.Length > 1)
            {
                for (var i = 0; i < moves.Length; i++)
                {
                    var move = moves[i];

                    Position.Make(move);

                    int score;
                    if (i < NonPvIterations)
                    {
                        score = -Search(-beta, -alpha, depth - 1);
                    }
                    else
                    {
                        score = -Search(-alpha - NullWindow, -alpha, depth - 1);
                        if (alpha < score && score < beta && depth > 1)
                        {
                            score = -Search(-beta, -score, depth - 1);
                        }
                    }

                    if (score > result.Value)
                    {
                        result.Value = score;
                        result.Move = move;
                    }

                    Position.UnMake();

                    if (score > alpha)
                    {
                        alpha = score;
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
            if (depth == 0)
            {
                return Evaluate(alpha, beta);
            }

            int value = int.MinValue;
            IMove bestMove = null;

            var moves = GenerateMoves(alpha, beta, depth);
            if (moves == null) return alpha;

            if (CheckMoves(alpha, beta, moves, out var defaultValue)) return defaultValue;

            for (var i = 0; i < moves.Length; i++)
            {
                var move = moves[i];

                Position.Make(move);

                int score;
                if (i < NonPvIterations)
                {
                    score = -Search(-beta, -alpha, depth - 1);
                }
                else
                {
                    score = -Search(-alpha - NullWindow, -alpha, depth - 1);
                    if (alpha < score && score < beta && depth > 1)
                    {
                        score = -Search(-beta, -score, depth - 1);
                    }
                }

                if (score > value)
                {
                    value = score;
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

            return value;
        }

        #endregion
    }
}
