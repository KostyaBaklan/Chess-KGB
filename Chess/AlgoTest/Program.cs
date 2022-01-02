using System;

namespace AlgoTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Boot.SetUp();

            //PerformanceTest.Test(args);

            PerformanceTest.Test(new []{ "pv", "6","4"});

            //OpenningsTest.Opennings();

            Console.WriteLine("Finished");

            //Console.ReadLine();
        }
    }
}
