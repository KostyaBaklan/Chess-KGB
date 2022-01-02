using Infrastructure.Sorters.Comparers;

namespace Infrastructure.Sorters.Pv
{
    public class PvValueStaticMoveSorter : PvMoveSorter
    {
        public PvValueStaticMoveSorter()
        {
            Comparer = new ValueStaticComparer();
        }
    }
}