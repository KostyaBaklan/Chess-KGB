﻿using Engine.Interfaces;
using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;

namespace Engine.Strategies.LateMove.Base
{
    public class LmrComplexHistoryStrategy : LmrStrategyBase
    {
        public LmrComplexHistoryStrategy(short depth, IPosition position) : base(depth, position)
        {
            InitializeSorters(depth, position, new ComplexSorter(position, new HistoryComparer()));
        }
    }
}