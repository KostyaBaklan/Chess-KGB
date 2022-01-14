using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Helpers;
using Engine.Models.Transposition;
using Engine.Sorting.Comparers;

namespace Engine.Strategies.AlphaBeta.Null
{
    public abstract class AlphaBetaVerifiedNullStrategy : AlphaBetaNullStrategy
    {
        protected int Reduction;

        protected AlphaBetaVerifiedNullStrategy(short depth, IPosition position, IMoveComparer comparer) : base(depth, position, comparer)
        {
            Reduction = 4;
            MinReduction = 3;
            MaxReduction = 4;
        }
        public override IResult GetResult(int alpha, int beta, int depth, IMove pvMove = null)
        {
            CanUseNull = false;
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
            for (var i = 0; i < moves.Count; i++)
            {
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
            }

            int value = int.MinValue;
            IMove bestMove = null;

            var lastMove = MoveHistory.GetLastMove();
            var moves = Position.GetAllMoves(Sorter, pv);
            if (moves.Count == 0)
            {
                return lastMove.IsCheck()
                    ? EvaluationService.GetMateValue(lastMove.Piece.IsWhite())
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

            if (!lastMove.IsCheck() && CanUseNull && beta - alpha > NullWindow)
            {
                MakeNullMove();
                var r = depth > 6 ? MaxReduction : MinReduction;
                var v = -Search(-beta, NullWindow - beta, depth - r - 1);
                UndoNullMove();
                if (v >= beta)
                {
                    depth -= Reduction;
                    if (depth <= 0)
                    {
                        return Evaluate(alpha, beta);
                    }
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

            var best = value == int.MinValue ? short.MinValue : value;

            TranspositionEntry te = new TranspositionEntry { Depth = (byte) depth, Value = (short) best, PvMove = bestMove};
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