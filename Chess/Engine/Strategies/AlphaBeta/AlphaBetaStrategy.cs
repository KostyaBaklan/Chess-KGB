﻿using System.Runtime.CompilerServices;
using Engine.DataStructures;
using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using Engine.Models.Transposition;
using Engine.Strategies.Base;

namespace Engine.Strategies.AlphaBeta
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
                else if (depth == 8)
                {
                    capacity = 10023499;
                }
                else if (depth == 9)
                {
                    capacity = 15486997;
                }
                else
                {
                    capacity = 22115983;
                }
                //else
                //{
                //    capacity = 10023499;
                //}

                Table = new TranspositionTable(capacity, depth);
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

            if (UseAging)
            {
                MoveProvider.AgeHistory(); 
            }

            return GetResult(-SearchValue, SearchValue, depth);
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
                    pv = GetPv(entry.PvMove);
                }
            }

            var moves = Position.GetAllMoves(Sorters[depth], pv);

            if (CheckMoves(moves, out var res)) return res;

            if (moves.Length > 1)
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

                pv = GetPv(entry.PvMove);
            }

            int value = int.MinValue;
            MoveBase bestMove = null;

            var moves = GenerateMoves(alpha, beta, depth, pv);
            if (moves == null) return alpha;

            if (CheckMoves(alpha, beta, moves, out var defaultValue)) return defaultValue;

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

            bestMove.History += 1 << depth;

            if (!isNotEndGame) return value;

            if (isInTable && !shouldUpdate) return value;

            return StoreValue(alpha, beta, depth, value, bestMove);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected int StoreValue(int alpha, int beta, int depth, int value, MoveBase bestMove)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected MoveBase GetPv(short entry)
        {
            var pv = MoveProvider.Get(entry);
            var turn = Position.GetTurn();
            return pv.Piece.IsWhite() && turn != Turn.White || pv.Piece.IsBlack() && turn != Turn.Black ? null : pv;
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
