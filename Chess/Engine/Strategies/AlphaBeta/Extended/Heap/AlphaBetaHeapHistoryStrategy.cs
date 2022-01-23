using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Strategies.AlphaBeta.Extended.Heap
{
    public class AlphaBetaHeapHistoryStrategy : AlphaBetaHeapStrategy
    {
        public AlphaBetaHeapHistoryStrategy(short depth, IPosition position) : base(depth, position, new HistoryComparer())
        {
        }
    }
}