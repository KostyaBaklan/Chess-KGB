using Engine.Interfaces;
using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;
using Engine.Strategies.AlphaBeta.Simple;

namespace Engine.Strategies.AlphaBeta.Advanced
{
    public abstract class AlphaBetaAdvancedStrategy : AlphaBetaStrategy
    {
        protected AlphaBetaAdvancedStrategy(short depth, IPosition position, IMoveComparer comparer) : base(depth, position)
        {
            Sorter = new AdvancedSorter(position, comparer);
        }
    }
}
