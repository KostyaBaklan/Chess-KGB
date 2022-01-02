using Algorithms.Interfaces;
using Algorithms.Models;
using Algorithms.Strategies.Base;
using Infrastructure.Interfaces.Moves;
using Infrastructure.Interfaces.Position;
using Infrastructure.Sorters.Pv;

namespace Algorithms.Strategies.Misc
{
    public class TestStrategy : StrategyBase
    {
        private readonly PvMoveSorter _sorter;

        public TestStrategy(IPosition position) : base(4, new PvStaticMoveSorter(), position)
        {
            _sorter = Sorter as PvMoveSorter;
        }

        #region Overrides of StrategyBase

        protected override void OnCutOff(IMove move, int depth)
        {
            _sorter.Add(move);
        }

        public override IResult GetResult()
        {
            return GetResult(-1000000, 1000000, Depth);
        }

        public override IResult GetResult(int alpha, int beta, int depth, IMove pvMove = null, IMove cutMove = null)
        {
            Result result = new Result();

            var moves = Position.GetAllMoves(Sorter);
            foreach (var move in moves)
            {
                try
                {
                    Position.MakeTest(move);

                    var isCheck = Position.IsCheckTest();

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
                return Position.GetValue();
            }

            int value = int.MinValue;
            var moves = Position.GetAllMoves(Sorter);

            foreach (var move in moves)
            {
                Position.MakeTest(move);

                var isCheck = Position.IsCheckTest();

                if (!isCheck)
                {
                    var r = -Search(-beta, -alpha, depth - 1);
                    if (r > value)
                    {
                        value = r;
                    }
                }

                Position.UnMake();

                if (isCheck) continue;

                if (value > alpha)
                {
                    alpha = value;
                }

                if (alpha < beta) continue;

                OnCutOff(move, depth);
                break;
            }

            var best = value == int.MinValue ? short.MinValue : value;
            return best;
        }

        #endregion
    }
}
