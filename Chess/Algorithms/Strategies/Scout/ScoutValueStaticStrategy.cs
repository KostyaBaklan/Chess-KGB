using Infrastructure.Interfaces.Moves;
using Infrastructure.Interfaces.Position;
using Infrastructure.Sorters.Pv;

namespace Algorithms.Strategies.Scout
{
    public class ScoutValueStaticStrategy : ScoutStrategyBase
    {
        public ScoutValueStaticStrategy(short depth, IPosition position) : this(depth, new PvValueStaticMoveSorter(), position)
        {
        }

        public ScoutValueStaticStrategy(short depth, IMoveSorter sorter, IPosition position) : base(depth, 4, sorter, position)
        {
        }
    }
}