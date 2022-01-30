using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Strategies.AlphaBeta.Null.Extended
{
    public class AlphaBetaNullDifferenceStrategy : AlphaBetaExtendedNullStrategy
    {
        public AlphaBetaNullDifferenceStrategy(short depth, IPosition position) : base(depth, position, new DifferenceComparer())
        {
        }
    }
}