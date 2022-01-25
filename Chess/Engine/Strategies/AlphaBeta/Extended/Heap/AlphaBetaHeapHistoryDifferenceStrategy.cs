using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Strategies.AlphaBeta.Extended.Heap
{
    public class AlphaBetaHeapHistoryDifferenceStrategy : AlphaBetaHeapStrategy
    {
        public AlphaBetaHeapHistoryDifferenceStrategy(short depth, IPosition position) : base(depth, position, new HistoryDifferenceComparer())
        {
        }
    }
}