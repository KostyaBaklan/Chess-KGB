using Infrastructure.Sorters.Comparers;

namespace Infrastructure.Sorters.Pv
{
    public class PvFigureValueMoveSorter : PvFigureMoveSorterBase
    {
        public PvFigureValueMoveSorter()
        {
            Comparer = new ValueComparer();
        }
    }
}