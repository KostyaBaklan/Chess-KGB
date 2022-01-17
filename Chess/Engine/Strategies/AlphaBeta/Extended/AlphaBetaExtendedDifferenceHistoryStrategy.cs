using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Strategies.AlphaBeta.Extended
{
    public class AlphaBetaExtendedDifferenceHistoryStrategy : AlphaBetaExtendedStrategy
    {
        public AlphaBetaExtendedDifferenceHistoryStrategy(short depth, IPosition position) : base(depth, position, new DifferenceHistoryComparer())
        {
        }
    }
}