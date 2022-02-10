﻿using Engine.Interfaces;
using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;

namespace Engine.Strategies.NullMove
{
    public class NmrExtendedHistoryDifferenceStrategy : NmrStrategyBase
    {
        public NmrExtendedHistoryDifferenceStrategy(short depth, IPosition position) : base(depth, position)
        {
            Sorter = new ExtendedSorter(position, new HistoryDifferenceComparer());
        }
    }
}