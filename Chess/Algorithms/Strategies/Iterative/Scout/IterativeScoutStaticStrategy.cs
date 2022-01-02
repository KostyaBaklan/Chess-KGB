using Algorithms.Strategies.Scout;
using Infrastructure.Interfaces.Moves;
using Infrastructure.Interfaces.Position;
using Infrastructure.Sorters.Pv;

namespace Algorithms.Strategies.Iterative.Scout
{
    public class IterativeScoutStaticStrategy : IterativeScoutStrategyBase
    {
        public IterativeScoutStaticStrategy(short depth, IMoveSorter sorter, IPosition position) : base(depth, sorter, position)
        {
            Strategy = new ScoutStaticStrategy(depth, sorter, position);
        }

        public IterativeScoutStaticStrategy(short depth, IPosition position) : this(depth, new PvStaticMoveSorter(), position)
        {
        }
    }
}