using System;
using System.Collections.Generic;
using CommonServiceLocator;
using Engine.Interfaces.Config;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;

namespace Engine.Sorting
{
    public static class Sort
    {
        public static readonly int[] SortMinimum;
        public static readonly IComparer<MoveBase> Comparer;
        public static readonly IMoveComparer DifferenceComparer;
        public static readonly IMoveComparer HistoryComparer;

        static Sort()
        {
            var itemsToSort = ServiceLocator.Current.GetInstance<IConfigurationProvider>()
                .AlgorithmConfiguration.SortingConfiguration
                .ItemsToSort;
            var historyComparer = new HistoryComparer();
            Comparer = historyComparer;
            HistoryComparer = historyComparer;

            DifferenceComparer = new DifferenceComparer();

            SortMinimum = new int[128];
            for (var i = 0; i < SortMinimum.Length; i++)
            {
                var min = Math.Min(i / 3, itemsToSort);
                if (min == 0)
                {
                    min = 1;
                }
                SortMinimum[i] = min;
            }
        }
    }
}
