using Infrastructure.Interfaces.Moves;

namespace Infrastructure.Sorters.Comparers
{
    public class ValueStaticComparer : MoveComparerBase
    {
        #region Implementation of IComparer<in IMove>

        public override int Compare(IMove x, IMove y)
        {
            var v = y.Value.CompareTo(x.Value);
            return v == 0 ? y.StaticValue.CompareTo(x.StaticValue) : v;
        }

        #endregion
    }
}