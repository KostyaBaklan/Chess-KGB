using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Strategies.AlphaBeta.Null
{
    public class AlphaBetaNullStaticStrategy : AlphaBetaNullStrategy
    {
        public AlphaBetaNullStaticStrategy(short depth, IPosition position) : base(depth, position, new StaticComparer())
        {
        }
    }
}