using Infrastructure.Interfaces.Moves;
using Infrastructure.Interfaces.Position;
using Infrastructure.Sorters.Pv;

namespace Algorithms.Strategies.Aspiration
{
    public class AspirationKillerStrategy : AspirationStrategyBase
    {
        private readonly PvStaticMoveSorter _sorter;

        public AspirationKillerStrategy(short depth, IPosition position) : base(depth, new PvStaticMoveSorter(), position)
        {
            _sorter = Sorter as PvStaticMoveSorter;
        }

        protected override void OnCutOff(IMove move, int depth)
        {
            _sorter.Add(move);
        }
    }
}