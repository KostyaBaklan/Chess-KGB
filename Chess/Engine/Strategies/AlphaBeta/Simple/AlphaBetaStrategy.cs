using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Engine.DataStructures;
using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Models.Enums;
using Engine.Models.Transposition;
using Engine.Strategies.Base;

namespace Engine.Strategies.AlphaBeta.Simple
{
    public abstract class AlphaBetaStrategy : StrategyBase
    {
        protected readonly TranspositionTable Table;

        protected AlphaBetaStrategy(short depth, IPosition position, TranspositionTable table = null) : base(depth,
            position)
        {
            if (table == null)
            {
                int capacity;
                if (depth < 6)
                {
                    capacity = 1131467;
                }
                else if (depth == 6)
                {
                    capacity = 2263139;
                }
                else if (depth == 7)
                {
                    capacity = 5002903;
                }
                else if (depth == 7)
                {
                    capacity = 10023499;
                }
                else if (depth == 7)
                {
                    capacity = 15486997;
                }
                else
                {
                    capacity = 22115983;
                }

                Table = new TranspositionTable(capacity);
            }
            else
            {
                Table = table;
            }
        }

        public override int Size => Table.Count;

        public override IResult GetResult()
        {
            var depth = Depth;
            if (Position.GetPhase() == Phase.End)
            {
                depth++;
            }

            return GetResult(-SearchValue, SearchValue, depth);
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
                        pv = MoveProvider.Get(entry.PvMove);
                    }
                }
            }

            var moves = Position.GetAllMoves(Sorter, pv);

            if (CheckMoves(moves, out var res)) return res;

            if (moves.Count > 1)
            {
                for (var i = 0; i < moves.Count; i++)
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
                result.Move = moves[0];
            }

            result.Move.History++;
            return result;
        }

        #endregion

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
            IMove bestMove = null;

            IMoveCollection moves = GenerateMoves(alpha, beta, depth, pv);
            if (moves == null) return alpha;

            if (CheckMoves(alpha, beta, moves, out var defaultValue)) return defaultValue;

            for (var i = 0; i < moves.Count; i++)
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

            bestMove.History += 1 << depth;

            if (!isNotEndGame) return value;

            if (isInTable && !shouldUpdate) return value;

            return StoreValue(alpha, beta, depth, value, bestMove);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected int StoreValue(int alpha, int beta, int depth, int value, IMove bestMove)
        {
            TranspositionEntry te = new TranspositionEntry
                {Depth = (byte) depth, Value = (short) value, PvMove = bestMove.Key};
            if (value <= alpha)
            {
                te.Type = TranspositionEntryType.LowerBound;
            }
            else if (value >= beta)
            {
                te.Type = TranspositionEntryType.UpperBound;
            }
            else
            {
                te.Type = TranspositionEntryType.Exact;
            }

            Table.Set(Position.GetKey(), te);

            return value;
        }

        public void Clear()
        {
            Table.Clear();
        }

        #region Overrides of StrategyBase

        public override bool IsBlocked()
        {
            return Table.IsBlocked();
        }

        public override void ExecuteAsyncAction()
        {
            Table.Update();
        }

        #endregion
    }
}
