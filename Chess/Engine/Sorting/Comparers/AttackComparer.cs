using System.Collections.Generic;
using Engine.Models.Moves;

namespace Engine.Sorting.Comparers
{
    public class AttackComparer : IComparer<AttackBase>
    {
        #region Implementation of IComparer<in IAttack>

        public int Compare(AttackBase x, AttackBase y)
        {
            return x.Captured.CompareTo(y.Captured);
        }

        #endregion
    }
}