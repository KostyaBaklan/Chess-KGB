using System;
using CommonServiceLocator;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Enums;
using Engine.Models.Moves;
using Engine.Models.Transposition;
using Engine.Strategies.AlphaBeta;
using Engine.Strategies.Base;
using Engine.Strategies.LateMove.Base;
using Engine.Strategies.LateMove.Deep;

namespace Engine.Strategies.PVS
{
    public abstract class PvsStrategyBase : AlphaBetaStrategy
    {
        protected int NonPvIterations;
        protected int DepthOffset;
        protected int NullWindow;
        protected int MaxEndGameDepth;
        protected int PvsMinDepth;
        protected int PvsDepthStep;
        protected int PvsDepthIterations;

        protected StrategyBase InitialStrategy;
        protected StrategyBase EndGameStrategy;

        protected PvsStrategyBase(short depth, IPosition position) : base(depth, position)
        {
            var configurationProvider = ServiceLocator.Current.GetInstance<IConfigurationProvider>();
            NonPvIterations = configurationProvider
                .AlgorithmConfiguration.PvsConfiguration.NonPvIterations;
            PvsMinDepth = configurationProvider
                .AlgorithmConfiguration.PvsConfiguration.PvsMinDepth;
            PvsDepthStep = configurationProvider
                .AlgorithmConfiguration.PvsConfiguration.PvsDepthStep;
            PvsDepthIterations = configurationProvider
                .AlgorithmConfiguration.PvsConfiguration.PvsDepthIterations;
            MaxEndGameDepth = configurationProvider
                .AlgorithmConfiguration.MaxEndGameDepth;
            DepthOffset = configurationProvider
                .AlgorithmConfiguration.DepthOffset;
            NullWindow = configurationProvider
                .AlgorithmConfiguration.NullConfiguration.NullWindow;
            EndGameStrategy = new LmrDeepNullNoCacheStrategy(depth, position);
            InitialStrategy = new LmrDeepExtendedStrategy(depth,position,Table);
        }


        public override IResult GetResult()
        {
            if (Position.GetPhase() == Phase.End)
            {
                return EndGameStrategy.GetResult(-SearchValue, SearchValue,Math.Min(Depth + 1, MaxEndGameDepth));
            }

            //int x = (depth - PvsMinDepth) / PvsDepthStep;
            //var initialDepth = depth - PvsDepthStep * x;
            var initialDepth = Math.Max(Depth - PvsDepthIterations, PvsMinDepth);
            var result = InitialStrategy.GetResult(-SearchValue, SearchValue, initialDepth);
            if (result.GameResult == GameResult.Continue)
            {
                for (int d = initialDepth+1; d <= Depth; d++)
                {
                    result = GetResult(-SearchValue, SearchValue, d, result.Move);
                    if (result.GameResult != GameResult.Continue) break;
                }
            }

            return result;
        }

        #region Overrides of ComplexStrategyBase

        public override IResult GetResult(int alpha, int beta, int depth, MoveBase pvMove = null)
        {
            Result result = new Result();

            MoveBase pv = pvMove;
            var key = Position.GetKey();
            if (pv == null)
            {
                var isNotEndGame = Position.GetPhase() != Phase.End;
                if (isNotEndGame && Table.TryGet(key, out var entry))
                {
                    pv = GetPv(entry.PvMove);
                }
            }

            var moves = Position.GetAllMoves(Sorters[Depth], pv);

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
                        if (alpha < score && score < beta)
                        {
                            score = -Search(-beta, -alpha, depth - 1);
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
            MoveBase pv = null;
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

                pv = GetPv(entry.PvMove);
            }

            int value = short.MinValue;
            MoveBase bestMove = null;

            var moves = GenerateMoves(alpha, beta, depth,pv);
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
                    if (alpha < score && score < beta)
                    {
                        score = -Search(-beta, -alpha, depth - 1);
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

                Sorters[depth].Add(move);
                break;
            }

            bestMove.History += 1 << depth;

            if (!isNotEndGame) return value;

            if (isInTable && !shouldUpdate) return value;

            return StoreValue(alpha, beta, depth, value, bestMove);
        }

        #endregion
    }
}
