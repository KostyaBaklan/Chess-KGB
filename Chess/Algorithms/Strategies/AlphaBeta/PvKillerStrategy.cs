using Algorithms.Strategies.Base;
using Infrastructure.Interfaces.Moves;
using Infrastructure.Interfaces.Position;
using Infrastructure.Sorters.Pv;

namespace Algorithms.Strategies.AlphaBeta
{
    public abstract class PvStrategyBase : AlphaBetaStrategyBase
    {
        protected PvMoveSorter PvMoveSorter;

        protected PvStrategyBase(short depth, IPosition position, PvMoveSorter sorter) : base(depth, sorter, position)
        {
            Sorter = sorter;
            PvMoveSorter = sorter;
        }

        protected override void OnCutOff(IMove move, int depth)
        {
            PvMoveSorter.Add(move);
        }
    }
}