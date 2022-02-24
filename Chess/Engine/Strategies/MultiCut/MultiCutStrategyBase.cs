using System;
using CommonServiceLocator;
using Engine.DataStructures;
using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Enums;
using Engine.Models.Moves;
using Engine.Models.Transposition;
using Engine.Strategies.AlphaBeta;

namespace Engine.Strategies.MultiCut
{
    public abstract class MultiCutStrategyBase:AlphaBetaStrategy
    {
        protected bool IsCut;
        protected int MultiCutReduction;
        protected int MultiCutDepth;
        protected int MultiCutRequirement;
        protected int NullWindow;
        protected int MultiCutMoves;

        protected MultiCutStrategyBase(short depth, IPosition position, TranspositionTable table = null)
            : base(depth, position, table)
        {
            var configurationProvider = ServiceLocator.Current.GetInstance<IConfigurationProvider>();
            MultiCutReduction = configurationProvider
                .AlgorithmConfiguration.MultiCutConfiguration.MultiCutReduction;
            MultiCutDepth = configurationProvider
                .AlgorithmConfiguration.MultiCutConfiguration.MultiCutDepth;
            MultiCutRequirement = configurationProvider
                .AlgorithmConfiguration.MultiCutConfiguration.MultiCutRequirement;
            NullWindow = configurationProvider
                .AlgorithmConfiguration.NullConfiguration.NullWindow;
            MultiCutMoves = configurationProvider
                .AlgorithmConfiguration.MultiCutConfiguration.MultiCutMoves;
        }
        #region Overrides of StrategyBase

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
                    if ((entry.Depth - depth) % 2 == 0)
                    {
                        pv = MoveProvider.Get(entry.PvMove);
                    }
                }
            }

            var moves = Position.GetAllMoves(Sorter, pv);

            if (CheckMoves(moves, out var res)) return res;

            if (moves.Length > 1)
            {
                for (var i = 0; i < moves.Length; i++)
                {
                    var move = moves[i];
                    Position.Make(move);

                    IsCut = true;
                    var value = -Search(-beta, -alpha, depth - 1);
                    IsCut = false;

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
                result.Move = moves[0];
            }

            result.Move.History++;

            return result;
        }

        #endregion

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

                if ((entryDepth - depth) % 2 == 0)
                {
                    pv = MoveProvider.Get(entry.PvMove);
                }
            }

            int value = int.MinValue;
            MoveBase bestMove = null;

            var moves = GenerateMoves(alpha, beta, depth, pv);
            if (moves == null) return alpha;

            if (CheckMoves(alpha, beta, moves, out var defaultValue)) return defaultValue;

            if (IsCut && depth > MultiCutDepth)
            {
                int cutCount = 0;
                var multiCutMoves = Math.Min(MultiCutMoves, moves.Length);
                for (var i = 0; i < multiCutMoves; i++)
                {
                    var move = moves[i];
                    Position.Make(move);
                    IsCut = false;

                    var v = -Search(-beta, -beta + NullWindow, depth - MultiCutReduction - 1);

                    Position.UnMake();
                    IsCut = true;
                    if (v < beta) continue;

                    cutCount++;
                    if (cutCount >= MultiCutRequirement)
                    {
                        return v;
                    }
                }
            }


            for (var i = 0; i < moves.Length; i++)
            {
                var move = moves[i];

                Position.Make(move);
                IsCut = !IsCut;

                var r = -Search(-beta, -alpha, depth - 1);

                IsCut = !IsCut;
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

            bestMove.History += 1 << depth;

            if (!isNotEndGame) return value;

            if (isInTable && !shouldUpdate) return value;

            return StoreValue(alpha, beta, depth, value, bestMove);
        }
    }
}
