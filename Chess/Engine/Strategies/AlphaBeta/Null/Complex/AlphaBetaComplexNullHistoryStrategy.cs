using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Strategies.AlphaBeta.Null.Complex
{
    public class AlphaBetaComplexNullHistoryStrategy : AlphaBetaComplexNullStrategy
    {
        public AlphaBetaComplexNullHistoryStrategy(short depth, IPosition position) : base(depth, position, new HistoryComparer())
        {
        }
    }
}