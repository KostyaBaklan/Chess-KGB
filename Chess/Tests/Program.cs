using System;
using Common;

namespace Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            Boot.SetUp();

            PerformanceTest.Test(args);

           // PerformanceTest.Test(new[] { "ad", "6", "4",false.ToString() });

            //OpenningsTest.Opennings();

            Console.WriteLine("Finished");

            //Console.ReadLine();
        }
    }
}
