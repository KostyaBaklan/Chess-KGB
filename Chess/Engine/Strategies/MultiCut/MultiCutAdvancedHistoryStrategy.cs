﻿using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;

namespace Engine.Strategies.MultiCut
{
    public class MultiCutAdvancedHistoryStrategy : MultiCutStrategyBase
    {
        public MultiCutAdvancedHistoryStrategy(short depth, IPosition position, TranspositionTable table = null)
            : base(depth, position, table)
        {
            Sorter = new AdvancedSorter(position, new HistoryComparer());
        }
    }
}