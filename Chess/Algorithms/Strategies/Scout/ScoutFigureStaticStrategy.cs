using Infrastructure.Interfaces.Moves;
using Infrastructure.Interfaces.Position;
using Infrastructure.Sorters.Pv;

namespace Algorithms.Strategies.Scout
{
    public class ScoutFigureStaticStrategy : ScoutStrategyBase
    {
        public ScoutFigureStaticStrategy(short depth, IPosition position) : this(depth, new PvFigureStaticMoveSorter(), position)
        {
        }

        public ScoutFigureStaticStrategy(short depth, IMoveSorter sorter, IPosition position) : base(depth, 4, sorter, position)
        {
        }
    }
}