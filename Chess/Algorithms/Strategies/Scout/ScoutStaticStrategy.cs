using Infrastructure.Interfaces.Moves;
using Infrastructure.Interfaces.Position;
using Infrastructure.Sorters.Pv;

namespace Algorithms.Strategies.Scout
{
    public class ScoutStaticStrategy : ScoutStrategyBase
    {
        public ScoutStaticStrategy(short depth, IPosition position) : this(depth, new PvStaticMoveSorter(), position)
        {
        }

        public ScoutStaticStrategy(short depth, IMoveSorter sorter, IPosition position) : base(depth, 4, sorter, position)
        {
        }
    }
}