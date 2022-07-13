using System;
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
        protected int DepthLateReduction;

        protected LmrStrategyBase(short depth, IPosition position, TranspositionTable table = null) : base(depth, position,table)
        {
            var configurationProvider = ServiceLocator.Current.GetInstance<IConfigurationProvider>();
            LmrDepthThreshold = configurationProvider
                .AlgorithmConfiguration.LateMoveConfiguration.LmrDepthThreshold;
            DepthReduction = configurationProvider
                    .AlgorithmConfiguration.LateMoveConfiguration.LmrDepthReduction;
            DepthLateReduction = DepthReduction + 1;
        }

        public override IResult GetResult(int alpha, int beta, int depth, MoveBase pvMove = null)
        {
            Result result = new Result();

            MoveBase pv = pvMove;
            var key = Position.GetKey();
            if (pv == null)
            {
                if (Table.TryGet(key, out var entry))
                {
                    pv = GetPv(entry.PvMove);
                }
            }

            var moves = Position.GetAllMoves(Sorters[Depth], pv);

            var count = moves.Length;
            if (CheckMoves(count, out var res)) return res;

            if (count > 1)
            {
                if (MoveHistory.IsLastMoveWasCheck())
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
                    for (var i = 0; i < count; i++)
                    {
                        var move = moves[i];
                        Position.Make(move);

                        int value;
                        if (alpha > -SearchValue && i > LmrDepthThreshold && move.CanReduce && !move.IsCheck)
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

            if (Position.GetPhase() == Phase.End)
            {
                return EndGameStrategy.Search(alpha, beta, Math.Min(depth + 1, MaxEndGameDepth));
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

            if (depth > DepthReduction + 1 && !MoveHistory.IsLastMoveWasCheck())
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

            return StoreValue((byte) depth, (short) value, bestMove.Key);
        }
    }
}
