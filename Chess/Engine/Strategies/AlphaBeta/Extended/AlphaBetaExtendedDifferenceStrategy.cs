using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Strategies.AlphaBeta.Extended
{
    public class AlphaBetaExtendedDifferenceStrategy : AlphaBetaExtendedStrategy
    {
        public AlphaBetaExtendedDifferenceStrategy(short depth, IPosition position) : base(depth, position, new DifferenceComparer())
        {
        }
    }
}