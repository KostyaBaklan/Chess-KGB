﻿using System.Runtime.CompilerServices;
using CommonServiceLocator;
using Engine.DataStructures;
using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Enums;
using Engine.Models.Moves;
using Engine.Strategies.AlphaBeta;

namespace Engine.Strategies.LateMove.Base
{
    public abstract class LmrStrategyBase : AlphaBetaStrategy
    {
        protected int DepthReduction;
        protected int LmrDepthThreshold;

        protected LmrStrategyBase(short depth, IPosition position, TranspositionTable table = null) : base(depth, position,table)
        {
            var configurationProvider = ServiceLocator.Current.GetInstance<IConfigurationProvider>();
            LmrDepthThreshold = configurationProvider
                .AlgorithmConfiguration.LateMoveConfiguration.LmrDepthThreshold;
            DepthReduction = configurationProvider
                    .AlgorithmConfiguration.LateMoveConfiguration.LmrDepthReduction;
        }

        public override IResult GetResult(int alpha, int beta, int depth, MoveBase pvMove = null)
        {
            ResetSorterFlags();
            Result result = new Result();

            MoveBase pv = pvMove;
            var key = Position.GetKey();
            var isNotEndGame = Position.GetPhase() != Phase.End;
            if (pv == null)
            {
                if (isNotEndGame && Table.TryGet(key, out var entry))
                {
                    pv = GetPv(entry.PvMove);
                }
            }

            var moves = Position.GetAllMoves(Sorters[Depth], pv);

            if (CheckMoves(moves, out var res)) return res;

            if (moves.Length > 1)
            {
                if (MoveHistory.GetLastMove().IsCheck)
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
                            value = -Search(-beta, -alpha, depth - DepthReduction);
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

            int value = int.MinValue;
            MoveBase bestMove = null;

            var moves = GetMoves(alpha, beta, depth, pv);

            if (moves == null) return alpha;

            if (CheckMoves(alpha, beta, moves, out var defaultValue)) return defaultValue;

            if (depth > DepthReduction + 1 && !MoveHistory.GetLastMove().IsCheck)
            {
                for (var i = 0; i < moves.Length; i++)
                {
                    var move = moves[i];
                    Position.Make(move);

                    int r;
                    if (IsLmr(i) && CanReduce(move))
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

            if (isInTable && !shouldUpdate) return value;

            return StoreValue((byte) depth, (short) value, bestMove);
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
    }
}
