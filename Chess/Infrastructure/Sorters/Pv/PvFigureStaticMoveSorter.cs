using Infrastructure.Sorters.Comparers;

namespace Infrastructure.Sorters.Pv
{
    public class PvFigureStaticMoveSorter : PvFigureMoveSorterBase
    {
        public PvFigureStaticMoveSorter()
        {
            Comparer = new StaticComparer();
        }
    }
}