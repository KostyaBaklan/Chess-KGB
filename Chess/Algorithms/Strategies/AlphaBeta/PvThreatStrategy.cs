using Algorithms.Strategies.Base;
using CommonServiceLocator;
using Infrastructure.Interfaces.Moves;
using Infrastructure.Interfaces.Position;
using Infrastructure.Sorters.Pv;

namespace Algorithms.Strategies.AlphaBeta
{
    public class PvThreatStrategy : AlphaBetaStrategyBase
    {
        private readonly PvThreatMoveSorter _sorter;

        public PvThreatStrategy(short depth, IPosition position) 
            : base(depth, new PvThreatMoveSorter(position,ServiceLocator.Current.GetInstance<IMoveProvider>()), position)
        {
            _sorter = Sorter as PvThreatMoveSorter;
        }

        protected override void OnCutOff(IMove move, int depth)
        {
            _sorter.Add(move);
        }
    }
}