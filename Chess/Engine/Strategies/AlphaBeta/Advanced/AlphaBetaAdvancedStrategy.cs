using Engine.Interfaces;
using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;

namespace Engine.Strategies.AlphaBeta.Advanced
{
    public abstract class AlphaBetaAdvancedStrategy : AlphaBetaStrategy
    {
        protected AlphaBetaAdvancedStrategy(short depth, IPosition position, IMoveComparer comparer) : base(depth, position)
        {
            InitializeSorters(depth, position, new AdvancedSorter(position, comparer));
        }
    }
}
