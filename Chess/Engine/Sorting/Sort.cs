using System;
using System.Collections.Generic;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;

namespace Engine.Sorting
{
    public static class Sort
    {
        public static readonly int[] SortMinimum;
        public static readonly IComparer<MoveBase> HistoryComparer;

        static Sort()
        {
            HistoryComparer = new HistoryComparer();
            SortMinimum = new int[128];
            for (var i = 0; i < SortMinimum.Length; i++)
            {
                var min = Math.Min(i / 3, 9);
                if (min == 0)
                {
                    min = 1;
                }
                SortMinimum[i] = min;
            }
            //for (int i = 2; i < 10; i++)
            //{
            //    SortMinimum[i] = i / 2;
            //}
            //for (int i = 10; i < 24; i++)
            //{
            //    SortMinimum[i] = Math.Max(i / 3 + 1,5);
            //}
            //for (int i = 24; i < 30; i++)
            //{
            //    SortMinimum[i] = i / 3;
            //}
            //for (int i = 30; i < 40; i++)
            //{
            //    SortMinimum[i] = 9;
            //}
            //for (int i = 40; i < 128; i++)
            //{
            //    SortMinimum[i] = 10;
            //}
        }
    }
}
