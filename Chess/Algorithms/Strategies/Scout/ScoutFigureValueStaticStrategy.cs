using Infrastructure.Interfaces.Moves;
using Infrastructure.Interfaces.Position;
using Infrastructure.Sorters.Pv;

namespace Algorithms.Strategies.Scout
{
    public class ScoutFigureValueStaticStrategy : ScoutStrategyBase
    {
        public ScoutFigureValueStaticStrategy(short depth, IPosition position) : this(depth, new PvFigureValueStaticMoveSorter(), position)
        {
        }

        public ScoutFigureValueStaticStrategy(short depth, IMoveSorter sorter, IPosition position) : base(depth, 4, sorter, position)
        {
        }
    }
}