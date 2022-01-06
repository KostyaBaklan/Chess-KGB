using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Strategies.AlphaBeta.Extended
{
    public class AlphaBetaExtendedStaticStrategy : AlphaBetaExtendedStrategy
    {
        public AlphaBetaExtendedStaticStrategy(short depth, IPosition position) : base(depth, position, new StaticComparer())
        {
        }
    }
}