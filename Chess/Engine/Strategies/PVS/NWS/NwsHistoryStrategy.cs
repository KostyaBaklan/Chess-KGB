using Engine.Interfaces;
using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;

namespace Engine.Strategies.PVS.NWS
{
    public class NwsHistoryStrategy : NwsStrategyBase
    {
        public NwsHistoryStrategy(short depth, IPosition position) : base(depth, position)
        {
            InitializeSorters(depth, position, new AdvancedSorter(position, new HistoryComparer()));
        }
    }
}
