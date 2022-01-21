using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Strategies.AlphaBeta.Extended.Heap
{
    public class AlphaBetaHeapDifferenceStrategy : AlphaBetaHeapStrategy
    {
        public AlphaBetaHeapDifferenceStrategy(short depth, IPosition position) : base(depth, position, new DifferenceComparer())
        {
        }
    }
}