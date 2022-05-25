using Engine.Interfaces;
using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;
using Engine.Sorting.Sorters.Basic;
using Engine.Sorting.Sorters.Extended;
using Engine.Sorting.Sorters.Initial;

namespace Engine.Services
{
    public class MoveSorterProvider: IMoveSorterProvider
    {
        #region Implementation of IMoveSorterProvider

        public MoveSorter GetBasic(IPosition position, IMoveComparer comparer)
        {
            return new BasicSorter(position, comparer);
        }

        public MoveSorter GetInitial(IPosition position, IMoveComparer comparer)
        {
            return new InitialKillerSorter(position, comparer);
        }

        public MoveSorter GetExtended(IPosition position, IMoveComparer comparer)
        {
            return new ExtendedKillerSorter(position, comparer);
        }

        public MoveSorter GetAdvanced(IPosition position, IMoveComparer comparer)
        {
            return new AdvancedSorter(position, comparer);
        }

        #endregion
    }
}
