using Infrastructure.Interfaces.Moves;
using Infrastructure.Interfaces.Position;
using Infrastructure.Sorters.Pv;

namespace Algorithms.Strategies.Aspiration
{
    public class AspirationValueStrategy : AspirationStrategyBase
    {
        private readonly PvValueMoveSorter _sorter;

        public AspirationValueStrategy(short depth, IPosition position) : base(depth, new PvValueMoveSorter(),position)
        {
            _sorter = Sorter as PvValueMoveSorter;
        }

        protected override void OnCutOff(IMove move, int depth)
        {
            _sorter.Add(move);
        }
    }
}