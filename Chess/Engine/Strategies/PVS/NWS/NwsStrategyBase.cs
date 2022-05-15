using Engine.DataStructures;
using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Models.Enums;
using Engine.Models.Moves;
using Engine.Models.Transposition;
using Engine.Strategies.AlphaBeta.Extended;
using Engine.Strategies.PVS.Original;

namespace Engine.Strategies.PVS.NWS
{
    public abstract class NwsStrategyBase : PvsStrategyBase
    {
        protected readonly TranspositionTable Table;
        protected NwsStrategyBase(short depth, IPosition position) : base(depth, position)
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
            Table = new TranspositionTable(capacity,depth);
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

        public override IResult GetResult(int alpha, int beta, int depth, MoveBase pvMove = null)
        {
            Result result = new Result(); MoveBase pv = pvMove;
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

            var moves = Position.GetAllMoves(Sorters[depth], pv);

            if (CheckMoves(moves, out var res)) return res;

            if (moves.Length > 1)
            {
                for (var i = 0; i < moves.Length; i++)
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
                        score = -NullWindowSearch(-alpha, depth - 1);
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

                if ((entryDepth - depth) % 2 == 0)
                {
                    pv = MoveProvider.Get(entry.PvMove);
                }
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

                int score;
                if (i < NonPvIterations)
                {
                    score = -Search(-beta, -alpha, depth - 1);
                }
                else
                {
                    score = -NullWindowSearch(-alpha, depth - 1);
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

                Sorters[depth].Add(move);
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

        protected virtual int NullWindowSearch(int beta, int depth)
        {
            var nullWindow = beta+NullWindow;
            if (depth == 0)
            {
                return Evaluate(beta, nullWindow);
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

                    if (entry.Type == TranspositionEntryType.LowerBound && entry.Value > beta)
                    {
                        beta = entry.Value;
                    }
                    else if (entry.Type == TranspositionEntryType.UpperBound && entry.Value < beta)
                    {
                        beta = entry.Value;
                    }
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
            MoveBase bestMove = null;

            var moves = GenerateMoves(beta, nullWindow, depth, pv);
            if (moves == null) return beta;

            if (CheckMoves(beta, nullWindow, moves, out var defaultValue)) return defaultValue;

            for (var i = 0; i < moves.Length; i++)
            {
                var move = moves[i];

                Position.Make(move);

                int score = -NullWindowSearch(-nullWindow, depth - 1);

                Position.UnMake();

                if (score > value)
                {
                    value = score;
                    bestMove = move;
                }
                if (value >= beta)
                {
                    Sorters[depth].Add(move);
                    break;
                }
            }

            bestMove.History += 1 << depth;

            if (!isNotEndGame) return value;

            if (isInTable && !shouldUpdate) return value;

            TranspositionEntry te = new TranspositionEntry
                { Depth = (byte)depth, Value = (short)value, PvMove = bestMove.Key };
            if (value <= beta)
            {
                te.Type = TranspositionEntryType.LowerBound;
            }
            else if (value >= nullWindow)
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
    }
}
