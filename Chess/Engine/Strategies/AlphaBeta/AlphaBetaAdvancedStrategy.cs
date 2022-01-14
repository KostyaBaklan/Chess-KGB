using CommonServiceLocator;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Transposition;
using Engine.Sorting.Sorters;

namespace Engine.Strategies.AlphaBeta
{
    public class AlphaBetaAdvancedStrategy : AlphaBetaStrategy
    {
        private readonly IHistoryHeuristic _historyHeuristic = ServiceLocator.Current.GetInstance<IHistoryHeuristic>();

        public AlphaBetaAdvancedStrategy(short depth, IPosition position) : base(depth, position)
        {
            Sorter = new AdvancedSorter(position);
        }

        #region Overrides of AlphaBetaStrategy

        public override IResult GetResult(int alpha, int beta, int depth, IMove pvMove = null)
        {
            Result result = new Result();

            IMove pv = pvMove;
            if (Table.TryGet(Position.GetKey(), out var entry))
            {
                pv = entry.PvMove;
            }

            bool isHistoryUpdated = false;
            var moves = Position.GetAllMoves(Sorter, pv);
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
                    if (result.Value>int.MinValue)
                    {
                        _historyHeuristic.Update(move);
                        isHistoryUpdated = true;
                    }
                }

                if (value > alpha)
                {
                    alpha = value;
                }

                if (alpha < beta) continue;

                if (!isHistoryUpdated)
                {
                    _historyHeuristic.Update(move);
                }
                break;
            }

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

            bool isHistoryUpdated = false;
            int value = int.MinValue;
            IMove bestMove = null;
            var moves = Position.GetAllMoves(Sorter, pv);

            for (var i = 0; i < moves.Count; i++)
            {
                var move = moves[i];
                Position.Make(move);

                var r = -Search(-beta, -alpha, depth - 1);
                if (r > value)
                {
                    value = r;
                    bestMove = move;
                    if (value > int.MinValue)
                    {
                        _historyHeuristic.Update(move);
                        isHistoryUpdated = true;
                    }
                }

                Position.UnMake();

                if (value > alpha)
                {
                    alpha = value;
                }

                if (alpha < beta) continue;

                if (!isHistoryUpdated)
                {
                    _historyHeuristic.Update(move);
                }

                Sorter.Add(move);
                break;
            }

            var best = value == int.MinValue ? short.MinValue : value;

            TranspositionEntry te = new TranspositionEntry { Depth = (byte) depth, Value = (short) best, PvMove = bestMove };
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

        #endregion
    }
}