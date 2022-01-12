using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Strategies.AlphaBeta.Null
{
    public class AlphaBetaNullDifferenceStrategy : AlphaBetaNullStrategy
    {
        public AlphaBetaNullDifferenceStrategy(short depth, IPosition position) : base(depth, position, new DifferenceComparer())
        {
        }
    }
}