using System.Runtime.CompilerServices;
using CommonServiceLocator;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Enums;
using Engine.Models.Moves;

namespace Engine.Strategies.AlphaBeta.Null
{
    public abstract class NullMoveStrategy : AlphaBetaStrategy
    {
        protected bool CanUseNull;
        protected bool IsNull;
        protected int MinReduction;
        protected int MaxReduction;
        protected int NullWindow;

        protected NullMoveStrategy(short depth, IPosition position) : base(depth, position)
        {
            CanUseNull = false;
            MinReduction = 2;
            MaxReduction = 3;
            NullWindow = ServiceLocator.Current.GetInstance<IConfigurationProvider>()
                .AlgorithmConfiguration.NullConfiguration.NullWindow;
        }

        public override IResult GetResult(int alpha, int beta, int depth, MoveBase pvMove = null)
        {
            CanUseNull = false;
            Result result = new Result();

            MoveBase pv = pvMove;

            var isNotEndGame = Position.GetPhase() != Phase.End;
            var key = Position.GetKey();
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

            var moves = Position.GetAllMoves(Sorters[depth], pv);

            if (CheckMoves(moves, out var res)) return res;

            if (moves.Length > 1)
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

            MoveBase pv = null;
            var key = Position.GetKey();

            var isNotEndGame = Position.GetPhase() != Phase.End;

            bool shouldUpdate = false;
            bool isInTable = false;

            if (isNotEndGame && Table.TryGet(key, out var entry))
            {
                isInTable = true;
                var entryDepth = entry.Depth;
                if (entryDepth >= depth)
                {
                    //if (entry.Type == TranspositionEntryType.Exact)
                    //{
                    //    return entry.Value;
                    //}

                    //if (entry.Type == TranspositionEntryType.LowerBound && entry.Value > alpha)
                    //{
                    //    alpha = entry.Value;
                    //}
                    //else if (entry.Type == TranspositionEntryType.UpperBound && entry.Value < beta)
                    //{
                    //    beta = entry.Value;
                    //}

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

            int value = short.MinValue;
            MoveBase bestMove = null;

            var moves = GenerateMoves(alpha, beta, depth, pv);
            if (moves == null) return alpha;

            if (CheckMoves(alpha, beta, moves, out var defaultValue)) return defaultValue;

            var lastMove = MoveHistory.GetLastMove();
            if (CanUseNull && !lastMove.IsCheck && isNotEndGame && IsValidWindow(alpha, beta))
            {
                int r = depth > 6 ? MaxReduction : MinReduction;
                if (depth > r)
                {
                    MakeNullMove();
                    var v = -Search(-beta, NullWindow - beta, depth - r - 1);
                    UndoNullMove();
                    if (v >= beta)
                    {
                        return v;
                    }
                }
            }

            for (var i = 0; i < moves.Length; i++)
            {
                var move = moves[i];

                Position.Make(move);
                var r = -Search(-beta, -alpha, depth - 1);
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

                Sorters[depth].Add(move.Key);
                break;
            }

            if (IsNull) return value;

            bestMove.History += 1 << depth;

            if (!isNotEndGame) return value;

            if (isInTable && !shouldUpdate) return value;

            return StoreValue((byte) depth, (short) value, bestMove.Key);
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
