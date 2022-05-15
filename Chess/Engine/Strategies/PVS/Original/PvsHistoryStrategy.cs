﻿using Engine.Interfaces;
using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;

namespace Engine.Strategies.PVS.Original
{
    public class PvsHistoryStrategy:PvsStrategyBase
    {
        public PvsHistoryStrategy(short depth, IPosition position) : base(depth, position)
        {
            InitializeSorters(depth, position, new ExtendedSorter(position,new HistoryComparer()));
        }
    }
}
