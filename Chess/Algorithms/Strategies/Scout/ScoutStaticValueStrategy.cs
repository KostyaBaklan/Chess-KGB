using Infrastructure.Interfaces.Moves;
using Infrastructure.Interfaces.Position;
using Infrastructure.Sorters.Pv;

namespace Algorithms.Strategies.Scout
{
    public class ScoutStaticValueStrategy : ScoutStrategyBase
    {
        public ScoutStaticValueStrategy(short depth, IPosition position) : this(depth, new PvStaticValueMoveSorter(), position)
        {
        }

        public ScoutStaticValueStrategy(short depth, IMoveSorter sorter, IPosition position) : base(depth, 4, sorter, position)
        {
        }
    }
}