using System;
using CommonServiceLocator;
using Engine.DataStructures;
using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Enums;
using Engine.Models.Transposition;
using Engine.Strategies.AlphaBeta.Simple;

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
                .AlgorithmConfiguration.MultiCutReduction;
            MultiCutDepth = configurationProvider
                .AlgorithmConfiguration.MultiCutDepth;
            MultiCutRequirement = configurationProvider
                .AlgorithmConfiguration.MultiCutRequirement;
            NullWindow = configurationProvider
                .AlgorithmConfiguration.NullWindow;
            MultiCutMoves = configurationProvider
                .AlgorithmConfiguration.MultiCutMoves;
        }
        #region Overrides of StrategyBase

        public override IResult GetResult(int alpha, int beta, int depth, IMove pvMove = null)
        {
            Result result = new Result();

            IMove pv = pvMove;
            var key = Position.GetKey();
            if (pv == null)
            {
                var isNotEndGame = Position.GetPhase() != Phase.End;
                if (isNotEndGame && Table.TryGet(key, out var entry))
                {
                    if ((entry.Depth - depth) % 2 == 0)
                    {
                        pv = entry.PvMove;
                    }
                }
            }

            var moves = Position.GetAllMoves(Sorter, pv);
            if (moves.Count == 0)
            {
                result.GameResult = MoveHistory.GetLastMove().IsCheck() ? GameResult.Mate : GameResult.Pat;
                return result;
            }

            if (MoveHistory.IsThreefoldRepetition(key))
            {
                var v = Evaluate(alpha, beta);
                if (v < -500)
                {
                    result.GameResult = GameResult.ThreefoldRepetition;
                    return result;
                }
            }

            if (moves.Count > 1)
            {
                for (var i = 0; i < moves.Count; i++)
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
                    pv = entry.PvMove;
                }
            }

            int value = int.MinValue;
            IMove bestMove = null;

            var moves = Position.GetAllMoves(Sorter, pv);
            if (moves.Count == 0)
            {
                var lastMove = MoveHistory.GetLastMove();
                return lastMove.IsCheck()
                    ? -EvaluationService.GetMateValue()
                    : -EvaluationService.Evaluate(Position);
            }

            if (MoveHistory.IsThreefoldRepetition(key))
            {
                return Evaluate(alpha, beta);
            }

            if (IsCut && depth > MultiCutDepth)
            {
                int cutCount = 0;
                var multiCutMoves = Math.Min(MultiCutMoves, moves.All);
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


            for (var i = 0; i < moves.Count; i++)
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

            int best;
            if (bestMove == null)
            {
                best = -SearchValue;
            }
            else
            {
                bestMove.History += 1 << depth;
                best = value;
            }

            if (!isNotEndGame) return best;

            if (!isInTable || shouldUpdate)
            {
                TranspositionEntry te = new TranspositionEntry
                { Depth = (byte)depth, Value = (short)best, PvMove = bestMove };
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
