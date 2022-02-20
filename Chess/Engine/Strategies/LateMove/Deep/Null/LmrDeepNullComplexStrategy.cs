using Engine.Interfaces;
using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;

namespace Engine.Strategies.LateMove.Deep.Null
{
    public class LmrDeepNullComplexStrategy : LmrDeepNullStrategyBase
    {
        public LmrDeepNullComplexStrategy(short depth, IPosition position) : base(depth, position)
        {
            Sorter = new ComplexSorter(position, new HistoryComparer());
        }
    }
}