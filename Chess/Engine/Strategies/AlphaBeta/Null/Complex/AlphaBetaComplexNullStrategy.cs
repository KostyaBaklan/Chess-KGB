using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Interfaces;
using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;

namespace Engine.Strategies.AlphaBeta.Null.Complex
{
    public abstract class AlphaBetaComplexNullStrategy : NullMoveStrategy
    {
        protected AlphaBetaComplexNullStrategy(short depth, IPosition position, IMoveComparer comparer) : base(depth, position)
        {
            Sorter = new ComplexSorter(position, comparer);
        }
    }
}
