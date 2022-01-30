using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Strategies.AlphaBeta.Null.Advanced
{
    public class AlphaBetaAdvancedNullDifferenceHistoryStrategy : AlphaBetaAdvancedNullStrategy
    {
        public AlphaBetaAdvancedNullDifferenceHistoryStrategy(short depth, IPosition position) : base(depth, position, new DifferenceHistoryComparer())
        {
        }
    }
}