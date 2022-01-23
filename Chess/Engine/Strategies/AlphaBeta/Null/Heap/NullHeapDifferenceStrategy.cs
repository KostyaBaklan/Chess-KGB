﻿using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Strategies.AlphaBeta.Null.Heap
{
    public class NullHeapDifferenceStrategy : AlphaBetaNullHeapStrategy
    {
        public NullHeapDifferenceStrategy(short depth, IPosition position) : base(depth, position, new DifferenceComparer())
        {
        }
    }
}