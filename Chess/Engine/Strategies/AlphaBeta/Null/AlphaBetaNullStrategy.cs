using System.Runtime.CompilerServices;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Helpers;
using Engine.Models.Transposition;
using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;

namespace Engine.Strategies.AlphaBeta.Null
{
    public abstract class AlphaBetaNullStrategy : AlphaBetaStrategy
    {
        protected bool CanUseNull;
        protected int Reduction;
        protected int NullWindow;

        protected AlphaBetaNullStrategy(short depth, IPosition position, IMoveComparer comparer) : base(depth, position)
        {
            CanUseNull = false;
            Reduction = 2;
            NullWindow = EvaluationService.GetPenaltyValue();
            Sorter = new ExtendedSorter(position, comparer);
        }

        public override IResult GetResult(int alpha, int beta, int depth, IMove pvMove = null, IMove cutMove = null)
        {
            CanUseNull = false;
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
                try
                {
                    if (i > 0)
                    {
                        SwitchNull(); 
                    }
                    Position.Make(move);

                    var isCheck = Position.IsCheck();

                    if (isCheck) continue;

                    var value = -Search(-beta, -alpha, depth - 1);
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
                finally
                {
                    Position.UnMake();
                    if (i>0)
                    {
                        SwitchNull(); 
                    }
                }
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

            var lastMove = MoveHistory.GetLastMove();
            var moves = Position.GetAllMoves(Sorter, pv, cut);
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
                var v = -Search(-beta, NullWindow - beta, depth - Reduction - 1);
                UndoNullMove();
                if (v >= beta)
                {
                    return v;
                }

            }

            for (var i = 0; i < moves.Count; i++)
            {
                var move = moves[i];
                Position.Make(move);

                var isCheck = Position.IsCheck();

                if (!isCheck)
                {
                    var r = -Search(-beta, -alpha, depth - 1);
                    if (r > value)
                    {
                        value = r;
                        bestMove = move;
                    }
                }

                Position.UnMake();

                if (isCheck) continue;

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void UndoNullMove()
        {
            SwitchNull();
            Position.SwapTurn();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MakeNullMove()
        {
            SwitchNull();
            Position.SwapTurn();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void SwitchNull()
        {
            CanUseNull = !CanUseNull;
        }
    }
}
