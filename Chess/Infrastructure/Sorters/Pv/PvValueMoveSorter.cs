using Infrastructure.Sorters.Comparers;

namespace Infrastructure.Sorters.Pv
{
    public class PvValueMoveSorter : PvMoveSorter
    {
        public PvValueMoveSorter()
        {
            Comparer = new ValueComparer();
        }
    }
}