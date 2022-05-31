using System;
using Common;

namespace Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            Boot.SetUp();

            //PerformanceTest.Test(args);

           //PerformanceTest.Test(new[] { "lmr_bs_hc", "7", "6", false.ToString() });
           //PerformanceTest.Test(new[] { "lmrd_bs_hc", "7", "6", false.ToString() });
           //PerformanceTest.Test(new[] { "lmr_es_hc", "7", "6", false.ToString() });
           PerformanceTest.Test(new[] { "lmrd_es_hc", "8", "6", false.ToString() });
           //PerformanceTest.Test(new[] { "lmr_as_hc", "7", "6", false.ToString() });
           //PerformanceTest.Test(new[] { "lmrd_as_hc", "7", "6", false.ToString() });

            //OpenningsTest.Opennings();

            Console.WriteLine("Finished");

            //Console.ReadLine();
        }
    }
}
