using Infrastructure.Interfaces.Position;
using Infrastructure.Sorters.Pv;

namespace Algorithms.Strategies.AlphaBeta
{
    public class PvFigureStaticStrategy : PvStrategyBase
    {
        public PvFigureStaticStrategy(short depth, IPosition position)
            : base(depth, position, new PvFigureStaticMoveSorter())
        {
        }
    }
}