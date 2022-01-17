using Engine.Interfaces;
using Engine.Sorting.Sorters;
using Engine.Strategies.AlphaBeta.Simple;

namespace Engine.Strategies.AlphaBeta.Temp
{
    public class AlphaBetaComplexStrategy : AlphaBetaStrategy
    {
        public AlphaBetaComplexStrategy(short depth, IPosition position) : base(depth, position)
        {
            Sorter = new ComplexSorter(position);
        }
    }
}