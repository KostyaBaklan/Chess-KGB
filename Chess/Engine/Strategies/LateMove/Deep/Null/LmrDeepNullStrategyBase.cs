using System;
using System.Runtime.CompilerServices;
using CommonServiceLocator;
using Engine.DataStructures;
using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Enums;
using Engine.Models.Moves;

namespace Engine.Strategies.LateMove.Deep.Null
{
    public abstract class LmrDeepNullStrategyBase : LmrDeepStrategyBase
    {
        protected bool CanUseNull;
        protected bool IsNull;
        protected int NullWindow;
        protected int NullDepthReduction;
        protected int Reduction;
        protected int NullDepthOffset;

        protected LmrDeepNullStrategyBase(short depth, IPosition position, TranspositionTable table = null) : base(depth, position,table)
        {
            CanUseNull = false;
            var configurationProvider = ServiceLocator.Current.GetInstance<IConfigurationProvider>();
            NullWindow = configurationProvider
                .AlgorithmConfiguration.NullConfiguration.NullWindow;
            NullWindow = 1;
            NullDepthReduction = configurationProvider
                .AlgorithmConfiguration.DepthReduction;
            NullDepthOffset = configurationProvider
                .AlgorithmConfiguration.NullConfiguration.NullDepthOffset+ NullDepthReduction;
            Reduction = NullDepthReduction + 1;
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

            var moves = Position.GetAllMoves(Sorters[depth], pv);

            if (CheckMoves(moves.Length, out var res)) return res;

            if (moves.Length > 1)
            {
                var isCheck = MoveHistory.IsLastMoveWasCheck();
                if (isCheck)
                {
                    for (var i = 0; i < moves.Length; i++)
                    {
                        IsNull = false;
                        var move = moves[i];
                        SwitchNull();
                        Position.Make(move);

                        var value = -Search(-beta, -alpha, depth - 1);

                        Position.UnMake();
                        SwitchNull();

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
                    for (var i = 0; i < moves.Length; i++)
                    {
                        IsNull = false;
                        var move = moves[i];
                        SwitchNull();
                        Position.Make(move);

                        int value;
                        if (alpha > -SearchValue && i > LmrDepthThreshold && move.CanReduce && !move.IsCheck)
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
                        SwitchNull();

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

            if (CanUseNull && !MoveHistory.IsLastMoveWasCheck() && depth > NullDepthOffset &&  IsValidWindow(alpha, beta))
            {
                MakeNullMove();
                var v = -Search(-beta, NullWindow - beta, depth - Reduction);
                UndoNullMove();
                if (v >= beta)
                {
                    return v;
                }
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

            var moves = IsFutility(alpha, depth)
                ? Position.GetAllAttacks(Sorters[depth])
                : Position.GetAllMoves(Sorters[depth], pv);

            var count = moves.Length;
            if (CheckPosition(count, out var defaultValue)) return defaultValue;

            int value = int.MinValue;
            MoveBase bestMove = null;

            if (depth < DepthLateReduction || MoveHistory.IsLastMoveNotReducable())
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
            else if (depth == DepthLateReduction)
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

                    Sorters[depth].Add(move.Key);
                    break;
                }
            }

            if (IsNull) return value;

            bestMove.History += 1 << depth;

            if (isInTable && !shouldUpdate) return value;

            return StoreValue((byte)depth, (short)value, bestMove.Key);
        }

        protected int NullSearch(int alpha, int beta, int depth)
        {
            if (depth <= 0)
            {
                return Evaluate(alpha, beta);
            }

            MoveBase pv = null;
            var key = Position.GetKey();

            var isNotEndGame = Position.GetPhase() != Phase.End;
            if (isNotEndGame && Table.TryGet(key, out var entry))
            {
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

                pv = GetPv(entry.PvMove);
            }

            int value = int.MinValue;

            var moves = Position.GetAllMoves(Sorters[depth], pv);

            var count = moves.Length;
            if (CheckPosition(count, out var defaultValue)) return defaultValue;

            for (var i = 0; i < count; i++)
            {
                var move = moves[i];

                Position.Make(move);

                var r = -NullSearch(-beta, -alpha, depth - 1);
                if (r > value)
                {
                    value = r;
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
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool IsValidWindow(int alpha, int beta)
        {
            return beta < SearchValue && alpha > -SearchValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void UndoNullMove()
        {
            SwitchNull();
            Position.SwapTurn();
            IsNull = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MakeNullMove()
        {
            SwitchNull();
            Position.SwapTurn();
            IsNull = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void SwitchNull()
        {
            CanUseNull = !CanUseNull;
        }
    }
}
