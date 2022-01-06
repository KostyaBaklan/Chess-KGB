using Engine.Interfaces;
using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;

namespace Engine.Strategies.AlphaBeta
{
    public class AlphaBetaDifferenceStrategy : AlphaBetaStrategy
    {
        public AlphaBetaDifferenceStrategy(short depth, IPosition position) : base(depth, position)
        {
            Sorter = new SimpleMoveSorter(new DifferenceComparer(),position);
        }
    }
}