using Infrastructure.Interfaces.Moves;

namespace Infrastructure.Sorters.Comparers
{
    public class StaticValueComparer : MoveComparerBase
    {
        #region Implementation of IComparer<in IMove>

        public override int Compare(IMove x, IMove y)
        {
            var v = y.StaticValue.CompareTo(x.StaticValue);
            return v == 0 ? y.Value.CompareTo(x.Value) : v;
        }

        #endregion
    }
}