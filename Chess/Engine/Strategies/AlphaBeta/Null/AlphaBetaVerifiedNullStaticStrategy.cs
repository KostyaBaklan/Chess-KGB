using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Strategies.AlphaBeta.Null
{
    public class AlphaBetaVerifiedNullStaticStrategy : AlphaBetaVerifiedNullStrategy
    {
        public AlphaBetaVerifiedNullStaticStrategy(short depth, IPosition position) : base(depth, position, new StaticComparer())
        {
        }
    }
}