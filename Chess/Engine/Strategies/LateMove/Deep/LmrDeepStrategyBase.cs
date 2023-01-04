using System;
using CommonServiceLocator;
using Engine.DataStructures;
using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Enums;
using Engine.Models.Moves;
using Engine.Strategies.Base;
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
            var isNotEndGame = Position.GetPhase() != Phase.End;
            if (pv == null)
            {
                if (isNotEndGame && Table.TryGet(Position.GetKey(), out var entry))
                {
                    pv = GetPv(entry.PvMove);
                }
            }

            var moves = Position.GetAllMoves(Sorters[Depth], pv);

            var count = moves.Length;
            if (CheckMoves(count, out var res)) return res;

            if (count > 1)
            {
                if (UseSubSearch)
                {
                    moves = SubSearch(moves, alpha, beta, depth);
                }

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
                    for (var i = 0; i < count; i++)
                    {
                        var move = moves[i];
                        Position.Make(move);

                        int value;
                        if (i > LmrDepthThreshold && move.CanReduce && !move.IsCheck)
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
                return EndGameStrategy.Search(alpha, beta, Math.Min(depth + 1, MaxEndGameDepth));
            }

            MoveBase pv = null;
            bool shouldUpdate = false;
            bool isInTable = false;
            if (Table.TryGet(Position.GetKey(), out var entry))
            {
                isInTable = true;
                if (entry.Depth >= depth)
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
            int r;
            MoveBase move;
            MoveBase bestMove = null;

            if (depth < DepthLateReduction || MoveHistory.IsLastMoveNotReducible())
            {
                for (var i = 0; i < count; i++)
                {
                    move = moves[i];
                    Position.Make(move);

                    r = -Search(-beta, -alpha, depth - 1);

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
                    move = moves[i];
                    Position.Make(move);

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
                    move = moves[i];
                    Position.Make(move);

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

        protected override StrategyBase CreateSubSearchStrategy()
        {
            return new LmrDeepExtendedStrategy((short)(Depth - LmrSubSearchDepth), Position);
        }
    }
}