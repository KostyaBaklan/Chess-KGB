using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Strategies.AlphaBeta.Null.Complex
{
    public class AlphaBetaComplexNullHistoryDifferenceStrategy : AlphaBetaComplexNullStrategy
    {
        public AlphaBetaComplexNullHistoryDifferenceStrategy(short depth, IPosition position) : base(depth, position, new HistoryDifferenceComparer())
        {
        }
    }
}