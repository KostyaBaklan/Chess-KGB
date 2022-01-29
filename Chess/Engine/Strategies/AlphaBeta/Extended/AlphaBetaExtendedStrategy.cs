using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;
using Engine.Strategies.AlphaBeta.Simple;

namespace Engine.Strategies.AlphaBeta.Extended
{
    public abstract class AlphaBetaExtendedStrategy : AlphaBetaStrategy
    {
        protected AlphaBetaExtendedStrategy(short depth, IPosition position, IMoveComparer comparer, TranspositionTable table = null) : base(depth, position, table)
        {
            Sorter = new ExtendedSorter(position, comparer);
        }
    }
}