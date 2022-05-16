using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using CommonServiceLocator;
using Engine.Interfaces;
using Engine.Models.Boards;

namespace OpeningGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            var sync = new object();

            Boot.SetUp();

            short depth = 6;

            var evaluationService = ServiceLocator.Current.GetInstance<IEvaluationService>();
            evaluationService.Initialize(depth);

            IPosition position = new Position();

            var moveProvider = ServiceLocator.Current.GetInstance<IMoveProvider>();

            var log = "Moves";

            Stopwatch timer = Stopwatch.StartNew();
            Dictionary<string, string> moves = new Dictionary<string, string>();

            int x = 0;
            int z = 0;

            var path = Path.Combine(log, "Sequences.txt");

            //var enumerable = position.GetAllMoves(new ExtendedSorter(position, new HistoryComparer()))
            //    .Select(m => m.Key.ToString());

            //File.WriteAllLines(path, enumerable);

            double count = 0;
            var lines = File.ReadAllLines(path);
            var size = lines.Length;

            Parallel.For(0, size, new ParallelOptions
            {
                MaxDegreeOfParallelism = 2*Environment.ProcessorCount
            }, i =>
            {
                var s = lines[i];
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = @"OpeningTool.exe",
                        Arguments = s,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                process.WaitForExit();

                var output = process.StandardOutput.ReadToEnd();
                if (!string.IsNullOrWhiteSpace(output) && output != "0")
                {
                    lock (sync)
                    {
                        moves.Add(s, output);
                        //if (i % 10 == 9)
                        {
                            Console.WriteLine($"{++count/size*100.0}% - {timer.Elapsed}");
                        } 
                    }
                }
            });


            using (var movesWriter = new StreamWriter(Path.Combine(log, "moves.txt")))
            {
                using (var temp = new StreamWriter(Path.Combine(log, "temp.txt")))
                {
                    using (var writer = new StreamWriter(Path.Combine(log, "output.txt")))
                    {
                        foreach (var pair in moves)
                        {
                            writer.WriteLine($"{pair.Key}-{pair.Value}");

                            foreach (var s in pair.Value.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries))
                            {
                                temp.WriteLine($"{pair.Key},{s}");

                                StringBuilder builder = new StringBuilder();

                                foreach (var moveBase in pair.Key.Split(new[] {','},
                                        StringSplitOptions.RemoveEmptyEntries)
                                    .Select(k => moveProvider.Get(short.Parse(k))))
                                {
                                    builder.Append(moveBase);
                                }

                                movesWriter.WriteLine(
                                    $"{builder}-{moveProvider.Get(short.Parse(s))}");
                            }
                        }
                    }
                }
            }

            Console.WriteLine($"Finished - {timer.Elapsed}");

            Console.ReadLine();
        }
    }
}
