using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Strategies.AlphaBeta.Advanced
{
    public class AlphaBetaAdvancedHistoryStrategy : AlphaBetaAdvancedStrategy
    {
        public AlphaBetaAdvancedHistoryStrategy(short depth, IPosition position) : base(depth, position, new HistoryComparer())
        {
        }
    }
}