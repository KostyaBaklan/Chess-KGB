using Infrastructure.Interfaces.Moves;

namespace Infrastructure.Sorters.Comparers
{
    public class StaticComparer : MoveComparerBase
    {
        #region Implementation of IComparer<in IMove>

        public override int Compare(IMove x, IMove y)
        {
            return y.StaticValue.CompareTo(x.StaticValue);
        }

        #endregion
    }
}