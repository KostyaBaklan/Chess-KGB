using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Strategies.AlphaBeta.Extended.Heap
{
    public class AlphaBetaHeapDifferenceHistoryStrategy : AlphaBetaHeapStrategy
    {
        public AlphaBetaHeapDifferenceHistoryStrategy(short depth, IPosition position) : base(depth, position, new DifferenceHistoryComparer())
        {
        }
    }
}