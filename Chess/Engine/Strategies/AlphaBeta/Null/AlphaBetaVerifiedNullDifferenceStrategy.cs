using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Strategies.AlphaBeta.Null
{
    public class AlphaBetaVerifiedNullDifferenceStrategy : AlphaBetaVerifiedNullStrategy
    {
        public AlphaBetaVerifiedNullDifferenceStrategy(short depth, IPosition position) : base(depth, position, new DifferenceComparer())
        {
        }
    }
}