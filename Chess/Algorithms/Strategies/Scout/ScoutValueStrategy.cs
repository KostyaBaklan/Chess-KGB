using Infrastructure.Interfaces.Moves;
using Infrastructure.Interfaces.Position;
using Infrastructure.Sorters.Pv;

namespace Algorithms.Strategies.Scout
{
    public class ScoutValueStrategy:ScoutStrategyBase
    {
        public ScoutValueStrategy(short depth, IPosition position) : this(depth,new PvValueMoveSorter(), position)
        {
        }

        public ScoutValueStrategy(short depth, IMoveSorter sorter, IPosition position) : base(depth,4, sorter, position)
        {
        }
    }
}