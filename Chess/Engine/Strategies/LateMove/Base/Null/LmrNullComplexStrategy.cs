﻿using Engine.Interfaces;
using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;

namespace Engine.Strategies.LateMove.Base.Null
{
    public class LmrNullComplexStrategy : LmrNullStrategyBase
    {
        public LmrNullComplexStrategy(short depth, IPosition position) : base(depth, position)
        {
            Sorter = new ComplexSorter(position, new HistoryComparer());
        }
    }
}