using Engine.Interfaces;
using Engine.Sorting.Sorters;

namespace Engine.Strategies.AlphaBeta
{
    public class AlphaBetaComplexStrategy : AlphaBetaStrategy
    {
        public AlphaBetaComplexStrategy(short depth, IPosition position) : base(depth, position)
        {
            Sorter = new ComplexSorter(position);
        }
    }
}