using Engine.Interfaces;
using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;
using Engine.Strategies.AlphaBeta.Simple;

namespace Engine.Strategies.AlphaBeta.Extended.Heap
{
    public abstract class AlphaBetaHeapStrategy : AlphaBetaStrategy
    {
        protected AlphaBetaHeapStrategy(short depth, IPosition position, IMoveComparer comparer) : base(depth, position)
        {
            Sorter = new ExtendedHeapSorter(position, comparer);
        }
    }
}