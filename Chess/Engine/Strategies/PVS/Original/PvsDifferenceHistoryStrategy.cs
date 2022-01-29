﻿using Engine.Interfaces;
using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;

namespace Engine.Strategies.PVS.Original
{
    public class PvsDifferenceHistoryStrategy : PvsStrategyBase
    {
        public PvsDifferenceHistoryStrategy(short depth, IPosition position) : base(depth, position)
        {
            Sorter = new ExtendedSorter(position, new DifferenceHistoryComparer());
        }
    }
}