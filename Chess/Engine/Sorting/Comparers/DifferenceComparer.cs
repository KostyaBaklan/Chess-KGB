using System.Runtime.CompilerServices;
using Engine.Interfaces;

namespace Engine.Sorting.Comparers
{
    public class DifferenceComparer : IMoveComparer
    {
        #region Implementation of IComparer<in IMove>

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(IMove x, IMove y)
        {
            return y.Difference.CompareTo(x.Difference);
        }

        #endregion
    }
}