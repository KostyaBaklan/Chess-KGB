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

        public override IResult GetResult(int alpha, int beta, int depth, IMove pvMove = null, IMove cutMove = null)
        {
            Result result = new Result();

            IMove pv = pvMove, cut = cutMove;
            if (Table.TryGet(Position.GetKey(), out var entry))
            {
                pv = entry.PvMove;
                cut = cutMove;
            }

            bool isHistoryUpdated = false;
            var moves = Position.GetAllMoves(Sorter, pv, cut);
            for (var i = 0; i < moves.Count; i++)
            {
                var move = moves[i];
                try
                {
                    Position.Make(move);

                    var isCheck = Position.IsNotLegal(move);

                    if (isCheck) continue;

                    var value = -Search(-beta, -alpha, depth - 1);
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
                    result.Cut = move;
                    break;
                }
                finally
                {
                    Position.UnMake();
                }
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

            bool isHistoryUpdated = false;
            int value = int.MinValue;
            IMove bestMove = null;
            IMove cutMove = null;
            var moves = Position.GetAllMoves(Sorter, pv, cut);

            for (var i = 0; i < moves.Count; i++)
            {
                var move = moves[i];
                Position.Make(move);

                var isCheck = Position.IsNotLegal(move);

                if (!isCheck)
                {
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
                }

                Position.UnMake();

                if (isCheck) continue;

                if (value > alpha)
                {
                    alpha = value;
                }

                if (alpha < beta) continue;

                if (!isHistoryUpdated)
                {
                    _historyHeuristic.Update(move);
                }
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

        #endregion
    }
}