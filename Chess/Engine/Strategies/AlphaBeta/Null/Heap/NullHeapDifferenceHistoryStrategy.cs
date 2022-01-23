using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Strategies.AlphaBeta.Null.Heap
{
    public class NullHeapDifferenceHistoryStrategy : AlphaBetaNullHeapStrategy
    {
        public NullHeapDifferenceHistoryStrategy(short depth, IPosition position) : base(depth, position, new DifferenceHistoryComparer())
        {
        }
    }
}