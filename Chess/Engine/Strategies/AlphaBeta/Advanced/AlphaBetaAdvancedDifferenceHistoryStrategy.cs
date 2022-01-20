using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Strategies.AlphaBeta.Advanced
{
    public class AlphaBetaAdvancedDifferenceHistoryStrategy : AlphaBetaAdvancedStrategy
    {
        public AlphaBetaAdvancedDifferenceHistoryStrategy(short depth, IPosition position) : base(depth, position, new DifferenceHistoryComparer())
        {
        }
    }
}