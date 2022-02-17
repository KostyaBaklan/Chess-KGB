using System.Runtime.CompilerServices;
using CommonServiceLocator;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Enums;
using Engine.Models.Transposition;

namespace Engine.Strategies.LateMove.Deep.Null
{
    public abstract class LmrDeepNullStrategyBase : LmrDeepStrategyBase
    {
        protected bool CanUseNull;
        protected bool IsNull;
        protected int NullWindow;
        protected int NullDepthReduction;
        protected int NullDepthOffset;

        protected LmrDeepNullStrategyBase(short depth, IPosition position) : base(depth, position)
        {
            CanUseNull = false;
            var configurationProvider = ServiceLocator.Current.GetInstance<IConfigurationProvider>();
            NullWindow = configurationProvider
                .AlgorithmConfiguration.NullConfiguration.NullWindow;
            NullDepthReduction = configurationProvider
                .AlgorithmConfiguration.DepthReduction;
            NullDepthOffset = configurationProvider
                .AlgorithmConfiguration.NullConfiguration.NullDepthOffset;
        }

        public override IResult GetResult(int alpha, int beta, int depth, IMove pvMove = null)
        {
            Result result = new Result();

            IMove pv = pvMove;
            var key = Position.GetKey();
            var isNotEndGame = Position.GetPhase() != Phase.End;
            if (pv == null)
            {
                if (isNotEndGame && Table.TryGet(key, out var entry))
                {
                    if ((entry.Depth - depth) % 2 == 0)
                    {
                        pv = MoveProvider.Get(entry.PvMove);
                    }
                }
            }

            Sorter = MoveHistory.GetPly() > 4 ? MainSorter : InitialSorter;

            var moves = Position.GetAllMoves(Sorter, pv);

            if (CheckMoves(moves, out var res)) return res;

            if (moves.Length > 1)
            {
                var isCheck = MoveHistory.GetLastMove().IsCheck();
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
                    for (var i = 0; i < moves.Length; i++)
                    {
                        IsNull = false;
                        var move = moves[i];
                        SwitchNull();
                        Position.Make(move);

                        int value;
                        if (alpha > -SearchValue && IsLmr(i) && CanReduce(move))
                        {
                            var reduction = isNotEndGame && i > LmrLateDepthThreshold ? DepthReduction + 1 : DepthReduction;
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

            IMove pv = null;
            var key = Position.GetKey();
            bool shouldUpdate = false;
            bool isInTable = false;

            //var isNotEndGame = Position.GetPhase() != Phase.End;
            if (Table.TryGet(key, out var entry))
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
            IMove bestMove = null;

            var moves = GenerateMoves(alpha, beta, depth, pv);
            if (moves == null) return alpha;

            if (CheckMoves(alpha, beta, moves, out var defaultValue)) return defaultValue;

            var isWasCheck = MoveHistory.GetLastMove().IsCheck();
            bool isNotEndGame = Position.GetPhase() != Phase.End;
            if (CanUseNull && !isWasCheck && isNotEndGame && depth > NullDepthReduction + NullDepthOffset &&
                IsValidWindow(alpha, beta))
            {
                MakeNullMove();
                var v = -Search(-beta, NullWindow - beta, depth - NullDepthReduction - 1);
                UndoNullMove();
                if (v >= beta)
                {
                    return v;
                }
            }

            if (!isWasCheck && depth > DepthReduction + 1)
            {
                for (var i = 0; i < moves.Length; i++)
                {
                    var move = moves[i];
                    Position.Make(move);

                    int r;
                    if (IsLmr(i) && CanReduce(move))
                    {
                        var reduction = isNotEndGame && i > LmrLateDepthThreshold ? DepthReduction + 1 : DepthReduction;
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

                    Sorter.Add(move);
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

                    Sorter.Add(move);
                    break;
                }
            }

            if (IsNull) return value;

            bestMove.History += 1 << depth;

            if (isInTable && !shouldUpdate) return value;

            return StoreValue(alpha, beta, depth, value, bestMove);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool IsValidWindow(int alpha, int beta)
        {
            return beta < SearchValue && beta - alpha > NullWindow;
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
