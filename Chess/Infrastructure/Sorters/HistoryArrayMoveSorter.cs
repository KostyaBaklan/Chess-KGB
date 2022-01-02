using System.Collections.Generic;
using System.Linq;
using Infrastructure.Interfaces.Moves;

namespace Infrastructure.Sorters
{
    public class HistoryArrayMoveSorter : IMoveSorter
    {
        #region Implementation of IMoveSorter

        public IEnumerable<IMove> Order(IEnumerable<IMove> moves, ICollection<IMove> pvNodes)
        {
            var array = moves.ToArray();
            int last = array.Length;
            while (last > 1)
            {
                var maximum = Maximum(array, last);
                yield return array[maximum];

                last--;
                var t = array[maximum];
                array[maximum] = array[last];
                array[last] = t;
            }

            yield return array[0];
        }

        private int Maximum(IMove[] array, int last)
        {
            int maxIndex = 0;
            for (int i = 1; i < last; i++)
            {
                if (array[i].History > array[maxIndex].History)
                {
                    maxIndex = i;
                }
            }

            return maxIndex;
        }

        #endregion

        public void Add(IMove move, ulong value)
        {
            move.History += value;
        }
    }
}