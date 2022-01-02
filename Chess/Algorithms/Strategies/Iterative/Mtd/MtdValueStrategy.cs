using Algorithms.Strategies.AlphaBeta;
using Infrastructure.Interfaces.Position;
using Infrastructure.Sorters.Pv;

namespace Algorithms.Strategies.Iterative.Mtd
{
    public class MtdValueStrategy : MtdStrategyBase
    {
        public MtdValueStrategy(short depth, IPosition position) : base(depth, new PvValueMoveSorter(), position)
        {
            Strategy = new PvValueStrategy(depth,position);
        }
    }
}