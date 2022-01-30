using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Strategies.AlphaBeta.Null.Advanced
{
    public class AlphaBetaAdvancedNullHistoryStrategy : AlphaBetaAdvancedNullStrategy
    {
        public AlphaBetaAdvancedNullHistoryStrategy(short depth, IPosition position) : base(depth, position, new HistoryComparer())
        {
        }
    }
}