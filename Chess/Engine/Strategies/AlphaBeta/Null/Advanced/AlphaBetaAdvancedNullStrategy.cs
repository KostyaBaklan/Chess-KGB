using Engine.Interfaces;
using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;

namespace Engine.Strategies.AlphaBeta.Null.Advanced
{
    public abstract class AlphaBetaAdvancedNullStrategy : NullMoveStrategy
    {
        protected AlphaBetaAdvancedNullStrategy(short depth, IPosition position, IMoveComparer comparer) : base(depth, position)
        {
            Sorter = new AdvancedSorter(position, comparer);
        }
    }
}