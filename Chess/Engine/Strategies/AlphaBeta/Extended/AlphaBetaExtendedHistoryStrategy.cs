using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Strategies.AlphaBeta.Extended
{
    public class AlphaBetaExtendedHistoryStrategy : AlphaBetaExtendedStrategy
    {
        public AlphaBetaExtendedHistoryStrategy(short depth, IPosition position, TranspositionTable table = null) : base(depth, position, new HistoryComparer(), table)
        {
        }
    }
}