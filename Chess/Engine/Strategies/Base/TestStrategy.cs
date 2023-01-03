using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Enums;
using Engine.Models.Moves;
using Engine.Sorting.Sorters;
using Engine.Sorting.Sorters.Extended;
using Engine.Sorting.Sorters.Initial;
using Engine.Strategies.AlphaBeta.Extended;

namespace Engine.Strategies.Base
{
    public class TestStrategy:AlphaBetaExtendedDifferenceStrategy
    {

        #region Overrides of StrategyBase

        public IResult Get()
        {
            return GetResult(-SearchValue, SearchValue, Depth);
        }

        #endregion

        public TestStrategy(IPosition position) : base(5, position)
        {
        }

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

            var moves = Position.GetAllMoves(Sorters[Depth], pv);

            if (CheckMoves(moves.Length, out var res)) return res;

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

            return result;
        }

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
                    if (entry.Value > alpha)
                    {
                        alpha = entry.Value;
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

            int value = short.MinValue;
            MoveBase bestMove = null;

            var moves = GenerateMoves(alpha, depth, pv);
            if (moves == null) return alpha;

            if (CheckPosition(moves.Length, out var defaultValue)) return defaultValue;

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
                if(!move.IsAttack)Sorters[depth].Add(move.Key);
                break;
            }

            if (!isNotEndGame) return value;

            if (isInTable && !shouldUpdate) return value;

            return StoreValue((byte)depth, (short)value, bestMove.Key);
        }

        #region Overrides of StrategyBase

        protected override void InitializeSorters(short depth, IPosition position, MoveSorter mainSorter)
        {
            Sorters = new IMoveSorter[depth + 2];

            var initialSorter = new InitialTestSorter(position, Sorting.Sort.DifferenceComparer);
            IMoveSorter sorter = new ExtendedTestSorter(position, Sorting.Sort.DifferenceComparer);

            var d = depth - SortDepth[depth];
            for (int i = 0; i < d; i++)
            {
                Sorters[i] = sorter;
            }

            for (var i = d; i < Sorters.Length; i++)
            {
                Sorters[i] = initialSorter;
            }
        }

        #endregion
    }
}
