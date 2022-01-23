using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Strategies.AlphaBeta.Null.Heap
{
    public class NullHeapHistoryStrategy : AlphaBetaNullHeapStrategy
    {
        public NullHeapHistoryStrategy(short depth, IPosition position) : base(depth, position, new HistoryComparer())
        {
        }
    }
}