using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Strategies.AlphaBeta.Null.Extended
{
    public class AlphaBetaNullHistoryDifferenceStrategy : AlphaBetaExtendedNullStrategy
    {
        public AlphaBetaNullHistoryDifferenceStrategy(short depth, IPosition position) : base(depth, position, new HistoryDifferenceComparer())
        {
        }
    }
}