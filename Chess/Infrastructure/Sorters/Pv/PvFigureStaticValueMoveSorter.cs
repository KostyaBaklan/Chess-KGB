using Infrastructure.Sorters.Comparers;

namespace Infrastructure.Sorters.Pv
{
    public class PvFigureStaticValueMoveSorter : PvFigureMoveSorterBase
    {
        public PvFigureStaticValueMoveSorter()
        {
            Comparer = new StaticValueComparer();
        }
    }
}