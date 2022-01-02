using System.Collections.Generic;
using Infrastructure.DataStructures;
using Infrastructure.Interfaces.Moves;

namespace Infrastructure.Sorters
{
    public class HistoryMoveSorter : IMoveSorter
    {
        #region Implementation of IMoveSorter

        public IEnumerable<IMove> Order(IEnumerable<IMove> moves, ICollection<IMove> pvNodes)
        {
            Heap heap = new Heap(moves);
            return heap.Sort();
        }

        #endregion

        public void Add(IMove move, ulong value)
        {
            move.History += value;
        }
    }
}
