using Infrastructure.Sorters.Comparers;

namespace Infrastructure.Sorters.Pv
{
    public class PvFigureValueStaticMoveSorter : PvFigureMoveSorterBase
    {
        public PvFigureValueStaticMoveSorter()
        {
            Comparer = new ValueStaticComparer();
        }
    }
}