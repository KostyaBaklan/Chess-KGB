﻿using Engine.Interfaces;
using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;

namespace Engine.Strategies.AlphaBeta.Simple
{
    public class AlphaBetaHistoryDifferenceStrategy : AlphaBetaStrategy
    {
        public AlphaBetaHistoryDifferenceStrategy(short depth, IPosition position) : base(depth, position)
        {
            Sorter = new SimpleMoveSorter(new HistoryDifferenceComparer(), position);
        }
    }
}