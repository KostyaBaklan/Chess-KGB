using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Strategies.AlphaBeta.Extended
{
    public abstract class AlphaBetaExtendedStrategy : AlphaBetaStrategy
    {
        protected AlphaBetaExtendedStrategy(short depth, IPosition position, IMoveComparer comparer, TranspositionTable table = null) : base(depth, position, table)
        {
            InitializeSorters(depth, position, MoveSorterProvider.GetExtended(position, comparer));
        }
    }
}