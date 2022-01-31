﻿using Engine.Interfaces;
using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;

namespace Engine.Strategies.LateMove
{
    public class LmrExtendedHistoryStrategy : LmrStrategyBase
    {
        public LmrExtendedHistoryStrategy(short depth, IPosition position) : base(depth, position)
        {
            Sorter = new ExtendedSorter(position,new HistoryComparer());
        }
    }
}
