using Infrastructure.Interfaces.Moves;
using Infrastructure.Interfaces.Position;
using Infrastructure.Sorters.Pv;

namespace Algorithms.Strategies.Scout
{
    public class ScoutFigureStaticValueStrategy : ScoutStrategyBase
    {
        public ScoutFigureStaticValueStrategy(short depth, IPosition position) : this(depth, new PvFigureStaticValueMoveSorter(), position)
        {
        }

        public ScoutFigureStaticValueStrategy(short depth, IMoveSorter sorter, IPosition position) : base(depth, 4, sorter, position)
        {
        }
    }
}