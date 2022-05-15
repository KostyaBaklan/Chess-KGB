using Engine.Interfaces;
using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;

namespace Engine.Strategies.AlphaBeta.Null.Extended
{
    public abstract class AlphaBetaExtendedNullStrategy : NullMoveStrategy
    {
        protected AlphaBetaExtendedNullStrategy(short depth, IPosition position, IMoveComparer comparer) : base(depth, position)
        {
            InitializeSorters(depth, position, new ExtendedSorter(position, comparer));
        }
    }
}