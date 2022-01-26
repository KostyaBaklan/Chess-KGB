using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Strategies.AlphaBeta.Extended
{
    public class AlphaBetaExtendedHistoryDifferenceStrategy : AlphaBetaExtendedStrategy
    {
        public AlphaBetaExtendedHistoryDifferenceStrategy(short depth, IPosition position) : base(depth, position, new HistoryDifferenceComparer())
        {
        }
    }
}