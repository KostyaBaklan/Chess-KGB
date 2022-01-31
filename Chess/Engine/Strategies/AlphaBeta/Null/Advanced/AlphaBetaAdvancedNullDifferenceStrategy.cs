using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Strategies.AlphaBeta.Null.Advanced
{
    public class AlphaBetaAdvancedNullDifferenceStrategy : AlphaBetaAdvancedNullStrategy
    {
        public AlphaBetaAdvancedNullDifferenceStrategy(short depth, IPosition position) : base(depth, position, new DifferenceComparer())
        {
        }
    }
}