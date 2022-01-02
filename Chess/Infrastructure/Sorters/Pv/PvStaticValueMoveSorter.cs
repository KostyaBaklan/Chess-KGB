using Infrastructure.Sorters.Comparers;

namespace Infrastructure.Sorters.Pv
{
    public class PvStaticValueMoveSorter : PvMoveSorter
    {
        public PvStaticValueMoveSorter()
        {
            Comparer = new StaticValueComparer();
        }
    }
}