﻿using System.Collections.Generic;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Transposition;

namespace Engine.Strategies.AlphaBeta
{
    public abstract class AlphaBetaStrategy : StrategyBase
    {
        protected readonly TranspositionTable Table;

        protected readonly IMoveHistoryService MoveHistory =
            CommonServiceLocator.ServiceLocator.Current.GetInstance<IMoveHistoryService>();

        protected int SearchValue = 1000000;

        protected AlphaBetaStrategy(short depth, IPosition position) : base(depth, position)
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
                capacity = 4356733;
            }
            else
            {
                capacity = 7910731;
            }
            Table = new TranspositionTable(capacity);
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

        public override IResult GetResult(int alpha, int beta, int depth, IMove pvMove = null, IMove cutMove = null)
        {
            Result result = new Result();

            IMove pv = pvMove, cut = cutMove;
            if (Table.TryGet(Position.GetKey(), out var entry))
            {
                pv = entry.PvMove;
                cut = cutMove;
            }

            var moves = Position.GetAllMoves(Sorter, pv, cut);
            if (moves.Count == 0)
            {
                result.GameResult = MoveHistory.GetLastMove().IsCheck() ? GameResult.Mate : GameResult.Pat;
            }

            if (MoveHistory.IsThreefoldRepetition(Position.GetKey()))
            {
                var v = Evaluate(alpha,beta);
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

                    result.Cut = move;
                    break;
                } 
            }
            else
            {
                result.Move = moves[0];
            }

            return result;
        }

        #endregion

        protected virtual IMove GetBestMove(HashSet<IMove> bestMoves)
        {
            var heap = new Heap(2,bestMoves.Count);
            foreach (var move in bestMoves)
            {
                Position.Make(move);

                move.Value =  -Evaluate(short.MinValue, short.MaxValue);

                Position.UnMake();

                heap.Insert(move);
            }

            return heap.Maximum();
        }

        public override int Search(int alpha, int beta, int depth)
        {
            if (depth == 0)
            {
                return Evaluate(alpha, beta);
            }

            IMove pv = null;
            IMove cut = null;
            var key = Position.GetKey();

            if (Table.TryGet(key, out var entry))
            {
                if (entry.Depth >= depth)
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

                pv = entry.PvMove;
                cut = entry.CutMove;
            }

            int value = int.MinValue;
            IMove bestMove = null;
            IMove cutMove = null;

            var moves = Position.GetAllMoves(Sorter, pv, cut);
            if (moves.Count == 0)
            {
                var lastMove = MoveHistory.GetLastMove();
                return lastMove.IsCheck()
                    ? EvaluationService.GetMateValue(lastMove.Piece.IsWhite())
                    : -EvaluationService.Evaluate(Position);
            }

            if (MoveHistory.IsThreefoldRepetition(Position.GetKey()))
            {
                var v = Evaluate(alpha,beta);
                if (v < 0)
                {
                    return -v;
                }
            }

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

                cutMove = move;
                Sorter.Add(move);
                break;
            }

            var best = value == int.MinValue ? short.MinValue : value;

            TranspositionEntry te = new TranspositionEntry { Depth = depth, Value = best, PvMove = bestMove, CutMove = cutMove };
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

            Table.Add(key, te);
            return best;
        }

        public void Clear()
        {
            Table.Clear();
        }
    }
}
