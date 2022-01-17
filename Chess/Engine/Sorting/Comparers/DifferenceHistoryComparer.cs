using System.Runtime.CompilerServices;
using Engine.Interfaces;

namespace Engine.Sorting.Comparers
{
    public class DifferenceHistoryComparer : IMoveComparer
    {
        #region Implementation of IComparer<in IMove>

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(IMove x, IMove y)
        {
            var comparision = y.Difference.CompareTo(x.Difference);
            return comparision != 0 ? comparision : y.History.CompareTo(x.History);
        }

        #endregion
    }
}