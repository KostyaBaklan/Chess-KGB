﻿using Engine.Interfaces;
using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;

namespace Engine.Strategies.LateMove.Deep
{
    public class LmrDeepAdvancedStrategy : LmrDeepStrategyBase
    {
        public LmrDeepAdvancedStrategy(short depth, IPosition position) : base(depth, position)
        {
            InitializeSorters(depth, position, new AdvancedSorter(position, new HistoryComparer()));
        }
    }
}