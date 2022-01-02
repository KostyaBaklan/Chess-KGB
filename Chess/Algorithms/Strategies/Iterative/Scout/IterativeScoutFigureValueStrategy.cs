using Algorithms.Strategies.Scout;
using Infrastructure.Interfaces.Moves;
using Infrastructure.Interfaces.Position;
using Infrastructure.Sorters.Pv;

namespace Algorithms.Strategies.Iterative.Scout
{
    public class IterativeScoutFigureValueStrategy : IterativeScoutStrategyBase
    {
        public IterativeScoutFigureValueStrategy(short depth, IMoveSorter sorter, IPosition position) : base(depth, sorter, position)
        {
            Strategy = new ScoutFigureValueStrategy(depth, sorter, position);
        }

        public IterativeScoutFigureValueStrategy(short depth, IPosition position) : this(depth, new PvFigureValueMoveSorter(), position)
        {
        }
    }
}