using Engine.DataStructures;
using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Models.Enums;
using Engine.Models.Transposition;
using Engine.Strategies.AlphaBeta.Extended;
using Engine.Strategies.PVS.Original;

namespace Engine.Strategies.PVS.Memory
{
    public abstract class PvsMemoryStrategyBase : PvsStrategyBase
    {
        protected readonly TranspositionTable Table;
        protected PvsMemoryStrategyBase(short depth, IPosition position) : base(depth, position)
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
                capacity = 10000139;
            }
            else
            {
                capacity = 22675729;
            }
            Table = new TranspositionTable(capacity);
            InternalStrategy = new AlphaBetaExtendedHistoryStrategy(depth, position, Table);
        }

        public override IResult GetResult()
        {
            var depth = Depth;
            if (Position.GetPhase() == Phase.End)
            {
                depth++;
            }

            var depthOffset = depth - DepthOffset;
            var result = InternalStrategy.GetResult(-SearchValue, SearchValue, depthOffset);
            if (result.GameResult == GameResult.Continue)
            {
                result = GetResult(-SearchValue, SearchValue, depth, result.Move);
                //for (int d = depthOffset + 1; d <= depth; d++)
                //{
                //    result = GetResult(-SearchValue, SearchValue, d, result.Move);
                //    if (result.GameResult != GameResult.Continue) break;
                //}
            }

            return result;
        }

        #region Overrides of ComplexStrategyBase

        public override IResult GetResult(int alpha, int beta, int depth, IMove pvMove = null)
        {
            Result result = new Result(); IMove pv = pvMove;
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
            if (moves.Count == 0)
            {
                result.GameResult = MoveHistory.GetLastMove().IsCheck() ? GameResult.Mate : GameResult.Pat;
                return result;
            }

            if (MoveHistory.IsThreefoldRepetition(key))
            {
                var v = Evaluate(alpha, beta);
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

                    int score;
                    if (i < NonPvIterations)
                    {
                        score = -Search(-beta, -alpha, depth - 1);
                    }
                    else
                    {
                        score = -Search(-alpha - NullWindow, -alpha, depth - 1);
                        if (alpha < score && score < beta && depth > 1)
                        {
                            score = -Search(-beta, -score, depth - 1);
                        }
                    }

                    if (score > result.Value)
                    {
                        result.Value = score;
                        result.Move = move;
                    }

                    Position.UnMake();

                    if (score > alpha)
                    {
                        alpha = score;
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
            if (depth == 0)
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

                int score;
                if (i < NonPvIterations)
                {
                    score = -Search(-beta, -alpha, depth - 1);
                }
                else
                {
                    score = -Search(-alpha - NullWindow, -alpha, depth - 1);
                    if (alpha < score && score < beta && depth > 1)
                    {
                        score = -Search(-beta, -score, depth - 1);
                    }
                }

                if (score > value)
                {
                    value = score;
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
            TranspositionEntry te = new TranspositionEntry
                { Depth = (byte)depth, Value = (short)value, PvMove = bestMove.Key };
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

            Table.Set(key, te);

            return value;
        }

        #endregion
    }
}