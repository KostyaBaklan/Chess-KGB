using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Strategies.MultiCut
{
    public class MultiCutExtendedHistoryStrategy: MultiCutStrategyBase
    {
        public MultiCutExtendedHistoryStrategy(short depth, IPosition position, TranspositionTable table = null)
            : base(depth, position, table)
        {
            InitializeSorters(depth, position, MoveSorterProvider.GetExtended(position, new HistoryComparer()));
        }
    }
}