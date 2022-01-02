using Algorithms.Strategies.AlphaBeta;
using Infrastructure.Interfaces.Position;
using Infrastructure.Sorters.Pv;

namespace Algorithms.Strategies.Iterative.Mtd
{
    public class MtdKillerStrategy: MtdStrategyBase
    {
        public MtdKillerStrategy(short depth, IPosition position) : base(depth, new PvStaticMoveSorter(), position)
        {
            Strategy = new PvStaticStrategy(depth, position);
        }
    }
}