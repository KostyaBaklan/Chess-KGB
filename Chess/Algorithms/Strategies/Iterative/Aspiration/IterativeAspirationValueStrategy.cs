using Algorithms.Strategies.AlphaBeta;
using Infrastructure.Interfaces.Position;

namespace Algorithms.Strategies.Iterative.Aspiration
{
    public class IterativeAspirationValueStrategy: IterativeAspirationStrategyBase
    {
        public IterativeAspirationValueStrategy(short depth, IPosition position) : base(depth, position)
        {
            Strategy = new PvValueStrategy(depth,position);
        }
    }
}