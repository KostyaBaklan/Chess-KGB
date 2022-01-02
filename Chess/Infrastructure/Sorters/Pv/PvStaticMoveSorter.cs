using Infrastructure.Sorters.Comparers;

namespace Infrastructure.Sorters.Pv
{
    public class PvStaticMoveSorter : PvMoveSorter
    {
        public PvStaticMoveSorter()
        {
            Comparer = new StaticComparer();
        }
    }
}