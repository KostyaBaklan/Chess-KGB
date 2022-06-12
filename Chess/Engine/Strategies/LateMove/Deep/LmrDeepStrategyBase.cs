using System;
using CommonServiceLocator;
using Engine.DataStructures;
using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Enums;
using Engine.Models.Moves;
using Engine.Strategies.LateMove.Base;

namespace Engine.Strategies.LateMove.Deep
{
    public abstract class LmrDeepStrategyBase : LmrStrategyBase
    {
        protected int LmrLateDepthThreshold;

        protected LmrDeepStrategyBase(short depth, IPosition position, TranspositionTable table = null) : base(depth, position, table)
        {
            LmrLateDepthThreshold = ServiceLocator.Current.GetInstance<IConfigurationProvider>()
                .AlgorithmConfiguration.LateMoveConfiguration.LmrLateDepthThreshold;
        }

        public override IResult GetResult(int alpha, int beta, int depth, MoveBase pvMove = null)
        {
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
                        if (alpha > -SearchValue && i > LmrDepthThreshold && move.CanReduce&& !move.IsCheck)
                        {
                            var reduction = isNotEndGame && i > LmrLateDepthThreshold ? DepthLateReduction : DepthReduction;
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

            if (Position.GetPhase() == Phase.End)
            {
                return EndGameStrategy.Search(alpha, beta, Math.Min(depth + 1, MaxEndGameDepth-1));
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

            int value = int.MinValue;
            MoveBase bestMove = null;

            var moves = GenerateMoves(alpha, beta, depth, pv);
            if (moves == null) return alpha;

            if (CheckMoves(alpha, beta, moves, out var defaultValue)) return defaultValue;

            if (depth < DepthLateReduction || MoveHistory.GetLastMove().IsCheck)
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

                    Sorters[depth].Add(move.Key);
                    break;
                }
            }
            else if (depth == DepthLateReduction)
            {
                for (var i = 0; i < moves.Length; i++)
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
                for (var i = 0; i < moves.Length; i++)
                {
                    var move = moves[i];
                    Position.Make(move);

                    int r;
                    if (!move.CanReduce || move.IsCheck)
                    {
                        r = -Search(-beta, -alpha, depth - 1);
                    }
                    else
                    {
                        if (i > LmrLateDepthThreshold)
                        {
                            r = -Search(-beta, -alpha, depth - DepthLateReduction);
                            if (r > alpha)
                            {
                                r = -Search(-beta, -alpha, depth - 1);
                            }
                        }
                        else if (i > LmrDepthThreshold)
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

            bestMove.History += 1 << depth;

            if (isInTable && !shouldUpdate) return value;

            return StoreValue((byte)depth, (short)value, bestMove.Key);
        }
    }
}