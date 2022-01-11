using System.Collections.Generic;
using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.DataStructures.Moves
{
    public abstract class MoveCollectionBase : IMoveCollection
    {
        protected List<IMove> _moves;
        protected readonly IMoveComparer _comparer;

        protected MoveCollectionBase(IMoveComparer comparer)
        {
            _comparer = comparer;
        }

        public int Count { get; protected set; }

        public IMove this[int index] => _moves[index];

        public abstract void Build();
    }
}