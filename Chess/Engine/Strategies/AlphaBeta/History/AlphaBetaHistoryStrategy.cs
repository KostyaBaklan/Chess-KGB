using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Transposition;

namespace Engine.Strategies.AlphaBeta.History
{
    public abstract class AlphaBetaHistoryStrategy : AlphaBetaStrategy
    {
        protected AlphaBetaHistoryStrategy(short depth, IPosition position) : base(depth, position)
        {
        }

        #region Overrides of StrategyBase

        public override IResult GetResult(int alpha, int beta, int depth, IMove pvMove = null)
        {
            Result result = new Result();

            IMove pv = pvMove;
            if (Table.TryGet(Position.GetKey(), out var entry))
            {
                pv = entry.PvMove;
            }

            var moves = Position.GetAllMoves(Sorter, pv);
            if (moves.Count == 0)
            {
                result.GameResult = MoveHistory.GetLastMove().IsCheck() ? GameResult.Mate : GameResult.Pat;
            }

            if (MoveHistory.IsThreefoldRepetition(Position.GetKey()))
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
            if (depth == 0)
            {
                return Evaluate(alpha, beta);
            }

            IMove pv = null;
            var key = Position.GetKey();

            var isNotEndGame = Position.GetPhase() != Phase.End;

            if (isNotEndGame && Table.TryGet(key, out var entry))
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
            }

            int value = int.MinValue;
            IMove bestMove = null;

            var moves = Position.GetAllMoves(Sorter, pv);
            if (moves.Count == 0)
            {
                var lastMove = MoveHistory.GetLastMove();
                return lastMove.IsCheck()
                    ? -EvaluationService.GetMateValue(lastMove.Piece.IsWhite())
                    : -EvaluationService.Evaluate(Position);
            }

            if (MoveHistory.IsThreefoldRepetition(Position.GetKey()))
            {
                var v = Evaluate(alpha, beta);
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

                Sorter.Add(move);
                break;
            }


            int best;
            if (bestMove == null)
            {
                best = short.MinValue;
            }
            else
            {
                bestMove.History += 1 << depth;
                best = value;
            }

            if (!isNotEndGame) return best;

            TranspositionEntry te = new TranspositionEntry { Depth = (byte)depth, Value = (short)best, PvMove = bestMove };
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
    }
}
