using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;
using Engine.Sorting.Sorters.Basic;
using Engine.Sorting.Sorters.Extended;
using Engine.Sorting.Sorters.Initial;

namespace Engine.Services
{
    public class MoveSorterProvider: IMoveSorterProvider
    {
        private readonly SortType _sortType;

        public MoveSorterProvider(IConfigurationProvider configuration)
        {
            _sortType = configuration.AlgorithmConfiguration.SortingConfiguration.SortType;
        }

        #region Implementation of IMoveSorterProvider

        public MoveSorter GetBasic(IPosition position, IMoveComparer comparer)
        {
            return new BasicSorter(position, comparer);
        }

        public MoveSorter GetInitial(IPosition position, IMoveComparer comparer)
        {
            return new InitialTradeSorter(position, comparer);
        }

        public MoveSorter GetExtended(IPosition position, IMoveComparer comparer)
        {
            return new ExtendedTradeSorter(position, comparer);
        }

        #endregion
    }
}
