using System.Runtime.CompilerServices;
using Engine.Interfaces;

namespace Engine.Sorting.Comparers
{
    public class StaticComparer:IMoveComparer
    {
        #region Implementation of IComparer<in IMove>

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(IMove x, IMove y)
        {
            return y.Static.CompareTo(x.Static);
        }

        #endregion
    }
}