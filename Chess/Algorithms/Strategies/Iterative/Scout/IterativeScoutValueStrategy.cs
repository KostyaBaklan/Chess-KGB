using Algorithms.Strategies.Scout;
using Infrastructure.Interfaces.Moves;
using Infrastructure.Interfaces.Position;
using Infrastructure.Sorters.Pv;

namespace Algorithms.Strategies.Iterative.Scout
{
    public class IterativeScoutValueStrategy: IterativeScoutStrategyBase
    {
        public IterativeScoutValueStrategy(short depth, IMoveSorter sorter, IPosition position) : base(depth, sorter, position)
        {
            Strategy = new ScoutValueStrategy(depth,sorter,position);
        }

        public IterativeScoutValueStrategy(short depth, IPosition position) : this(depth,new PvValueMoveSorter(), position)
        {
        }
    }
}