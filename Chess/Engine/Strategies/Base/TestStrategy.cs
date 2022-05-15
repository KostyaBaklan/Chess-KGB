using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;
using Engine.Strategies.AlphaBeta.Extended;

namespace Engine.Strategies.Base
{
    public class TestStrategy:AlphaBetaExtendedDifferenceHistoryStrategy
    {
        //protected short Depth;
        //protected MoveSorter Sorter;
        //protected IPosition Position;
        //public TestStrategy(IPosition position)
        //{
        //    Position = position;
        //    Depth = 5;
        //    InitializeSorters(depth, position, new ExtendedSorter(position,new DifferenceComparer()));
        //}

        #region Overrides of StrategyBase

        public IResult Get()
        {
            return GetResult(-10000, 10000, Depth);
        }

        public IResult Get(int alpha, int beta, int depth, MoveBase pvMove = null, MoveBase cutMove = null)
        {
            Result result = new Result();

            var moves = Position.GetAllMoves(Sorters[depth]);
            for (var i = 0; i < moves.Length; i++)
            {
                var move = moves[i];
                Position.Make(move);

                var value = -Find(-beta, -alpha, depth - 1);

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

            return result;
        }

        public int Find(int alpha, int beta, int depth)
        {
            if (depth == 0)
            {
                return Position.GetValue();
            }

            int value = int.MinValue;
            var moves = Position.GetAllMoves(Sorters[depth]);
            for (var i = 0; i < moves.Length; i++)
            {
                var move = moves[i];
                Position.Make(move);

                var r = -Find(-beta, -alpha, depth - 1);
                if (r > value)
                {
                    value = r;
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

            var best = value == int.MinValue ? short.MinValue : value;
            return best;
        }

        #endregion

        public TestStrategy(IPosition position) : base(5, position)
        {
        }
    }
}
