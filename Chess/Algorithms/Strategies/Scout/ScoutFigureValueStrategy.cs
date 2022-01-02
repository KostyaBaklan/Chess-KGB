using Infrastructure.Interfaces.Moves;
using Infrastructure.Interfaces.Position;
using Infrastructure.Sorters.Pv;

namespace Algorithms.Strategies.Scout
{
    public class ScoutFigureValueStrategy : ScoutStrategyBase
    {
        public ScoutFigureValueStrategy(short depth, IPosition position) : this(depth, new PvFigureValueMoveSorter(), position)
        {
        }

        public ScoutFigureValueStrategy(short depth, IMoveSorter sorter, IPosition position) : base(depth,4, sorter, position)
        {
        }
    }
}