using Algorithms.Strategies.Base;
using Infrastructure.Interfaces.Position;
using Infrastructure.Sorters;

namespace Algorithms.Strategies.Misc
{
    public class OpenningStrategy : AlphaBetaStrategyBase
    {
        public OpenningStrategy(IPosition position) : base(6, new ValueMoveSorter(), position)
        {
        }

        public void Clear()
        {
            Table?.Clear();
        }
    }
}
