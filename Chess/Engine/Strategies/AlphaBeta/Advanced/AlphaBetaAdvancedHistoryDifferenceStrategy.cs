using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Strategies.AlphaBeta.Advanced
{
    public class AlphaBetaAdvancedHistoryDifferenceStrategy : AlphaBetaAdvancedStrategy
    {
        public AlphaBetaAdvancedHistoryDifferenceStrategy(short depth, IPosition position) : base(depth, position, new HistoryDifferenceComparer())
        {
        }
    }
}