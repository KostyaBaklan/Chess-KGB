using Algorithms.Strategies.Scout;
using Infrastructure.Interfaces.Moves;
using Infrastructure.Interfaces.Position;
using Infrastructure.Sorters.Pv;

namespace Algorithms.Strategies.Iterative.Scout
{
    public class IterativeScoutFigureStaticStrategy : IterativeScoutStrategyBase
    {
        public IterativeScoutFigureStaticStrategy(short depth, IMoveSorter sorter, IPosition position) : base(depth, sorter, position)
        {
            Strategy = new ScoutFigureStaticStrategy(depth, sorter, position);
        }

        public IterativeScoutFigureStaticStrategy(short depth, IPosition position) : this(depth, new PvFigureStaticMoveSorter(), position)
        {
        }
    }
}