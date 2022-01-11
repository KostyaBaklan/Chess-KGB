using System.Collections.Generic;
using Engine.Interfaces;

namespace Engine.Sorting.Comparers
{
    public class AttackComparer : IComparer<IAttack>
    {
        #region Implementation of IComparer<in IAttack>

        public int Compare(IAttack x, IAttack y)
        {
            return x.Captured.CompareTo(y.Captured);
        }

        #endregion
    }
}