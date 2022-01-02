using Infrastructure.Interfaces.Moves;

namespace Infrastructure.Sorters.Comparers
{
    public abstract class MoveComparerBase : IMoveComparer
    {
        #region Implementation of IComparer<in IMove>

        public abstract int Compare(IMove x, IMove y);

        #endregion

        #region Implementation of IMoveComparer

        public virtual void Initialize() { }

        #endregion
    }
}