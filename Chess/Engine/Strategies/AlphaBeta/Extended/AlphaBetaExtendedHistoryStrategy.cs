using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Strategies.AlphaBeta.Extended
{
    public class AlphaBetaExtendedHistoryStrategy : AlphaBetaExtendedStrategy
    {
        public AlphaBetaExtendedHistoryStrategy(short depth, IPosition position) : base(depth, position, new HistoryComparer())
        {
        }
    }
}