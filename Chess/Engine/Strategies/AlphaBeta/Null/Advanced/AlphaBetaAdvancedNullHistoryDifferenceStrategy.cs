using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Strategies.AlphaBeta.Null.Advanced
{
    public class AlphaBetaAdvancedNullHistoryDifferenceStrategy : AlphaBetaAdvancedNullStrategy
    {
        public AlphaBetaAdvancedNullHistoryDifferenceStrategy(short depth, IPosition position) : base(depth, position, new HistoryDifferenceComparer())
        {
        }
    }
}