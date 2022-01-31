using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Strategies.AlphaBeta.Null.Extended
{
    public class AlphaBetaNullHistoryStrategy : AlphaBetaExtendedNullStrategy
    {
        public AlphaBetaNullHistoryStrategy(short depth, IPosition position) : base(depth, position, new HistoryComparer())
        {
        }
    }
}