using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
        public int Pv { get; protected set; }
        public int Cut { get; protected set; }
        public int All { get; protected set; }
        public int Late { get; protected set; }
        public int Bad { get; protected set; }

        public IMove this[int index] => Moves[index];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsLmr(int i)
        {
            return i >= Cut;
        }

        public abstract void Build();
    }
}