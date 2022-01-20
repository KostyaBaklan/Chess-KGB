using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Strategies.AlphaBeta.Advanced
{
    public class AlphaBetaAdvancedDifferenceStrategy : AlphaBetaAdvancedStrategy
    {
        public AlphaBetaAdvancedDifferenceStrategy(short depth, IPosition position) : base(depth, position, new DifferenceComparer())
        {
        }
    }
}