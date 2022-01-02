using Infrastructure.Interfaces.Moves;
using Infrastructure.Interfaces.Position;
using Infrastructure.Sorters.Comparers;

namespace Infrastructure.Sorters.Pv
{
    public class PvThreatMoveSorter : PvMoveSorter
    {
        public PvThreatMoveSorter(IPosition position, IMoveProvider moveProvider)
        {
            Comparer = new MoveThreatComparer(position,moveProvider);
        }
    }
}