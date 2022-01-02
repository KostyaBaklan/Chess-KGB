using Infrastructure.Interfaces.Position;
using Infrastructure.Sorters.Pv;

namespace Algorithms.Strategies.AlphaBeta
{
    public class PvFigureValueStrategy : PvStrategyBase
    {
        public PvFigureValueStrategy(short depth, IPosition position)
            : base(depth, position, new PvFigureValueMoveSorter())
        {
        }
    }
}