using Engine.Interfaces;
using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;

namespace Engine.Strategies.AlphaBeta.Extended
{
    public abstract class AlphaBetaExtendedStrategy : AlphaBetaStrategy
    {
        protected AlphaBetaExtendedStrategy(short depth, IPosition position, IMoveComparer comparer) : base(depth, position)
        {
            Sorter = new ExtendedSorter(position, comparer);
        }
    }
}