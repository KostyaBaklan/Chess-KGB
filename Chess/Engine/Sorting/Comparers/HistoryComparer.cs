﻿using System.Runtime.CompilerServices;
using Engine.Interfaces;

namespace Engine.Sorting.Comparers
{
    public class HistoryComparer : IMoveComparer
    {
        #region Implementation of IComparer<in IMove>

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(IMove x, IMove y)
        {
            return y.History.CompareTo(x.History);
        }

        #endregion
    }
}
