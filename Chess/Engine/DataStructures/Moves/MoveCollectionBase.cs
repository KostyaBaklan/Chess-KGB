using System.Collections.Generic;
using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.DataStructures.Moves
{
    public abstract class MoveCollectionBase : IMoveCollection
    {
        protected List<IMove> Moves;
        protected readonly IMoveComparer Comparer;

        protected MoveCollectionBase(IMoveComparer comparer)
        {
            Comparer = comparer;
        }

        public int Count { get; protected set; }

        public IMove this[int index] => Moves[index];

        public abstract void Build();

        #region Overrides of Object

        public override string ToString()
        {
            return $"Count={Count}";
        }

        #endregion
    }
}