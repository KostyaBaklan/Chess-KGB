using Infrastructure.Interfaces.Moves;

namespace Infrastructure.Sorters.Comparers
{
    public class ValueComparer : MoveComparerBase
    {
        #region Implementation of IComparer<in IMove>

        public override int Compare(IMove x, IMove y)
        {
            return y.Value.CompareTo(x.Value);
        }

        #endregion
    }
}