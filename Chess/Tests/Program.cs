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

           PerformanceTest.Test(new[] { "lmrd_es_hc", "9", "2",false.ToString() });

            //OpenningsTest.Opennings();

            Console.WriteLine("Finished");

            //Console.ReadLine();
        }
    }
}
