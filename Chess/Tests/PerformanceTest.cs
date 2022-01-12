using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using CommonServiceLocator;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Strategies;
using Engine.Strategies.AlphaBeta;
using Engine.Strategies.AlphaBeta.Extended;
using Engine.Strategies.AlphaBeta.Null;

namespace Tests
{
    internal class PerformanceTest
    {
        public static void Test(string[] args)
        {
            if (!Directory.Exists("Log"))
            {
                Directory.CreateDirectory("Log");
            }

            var depth = short.Parse(args[1]);
            var iterations = int.Parse(args[2]);
            bool shouldPrintPosition = args.Length>3 && bool.Parse(args[3]);
            IPosition position = new Position();

            Dictionary<string, IStrategy> strategies = new Dictionary<string, IStrategy>
            {
                {"as", new AlphaBetaStaticStrategy(depth, position)},
                {"ad", new AlphaBetaDifferenceStrategy(depth, position)},
                {"ac", new AlphaBetaComplexStrategy(depth, position)},
                {"aa", new AlphaBetaAdvancedStrategy(depth,position) },
                {"aed", new AlphaBetaExtendedDifferenceStrategy(depth,position)},
                {"aes", new AlphaBetaExtendedStaticStrategy(depth,position)},
                {"and", new AlphaBetaNullDifferenceStrategy(depth,position)},
                {"ans", new AlphaBetaNullStaticStrategy(depth,position)}
            };

            IStrategy strategy = strategies[args[0]];
            var file = Path.Combine("Log", $"{strategy}_{DateTime.Now:hh_mm_ss_dd_MM_yyyy}.log");
            using (var log = new StreamWriter(file))
            {
                log.WriteLine($"{strategy}. Depth = {depth}");
                Play(log, iterations, strategy, position, shouldPrintPosition);
            }

            //position.GetBoard().PrintCache(Path.Combine("Log", $"See_Cache_{strategy}_{DateTime.Now:hh_mm_ss_dd_MM_yyyy}.log"));
        }

        private static void Play(StreamWriter log, int depth, IStrategy strategy, IPosition position,
            bool shouldPrintPosition)
        {
            var formatter = ServiceLocator.Current.GetInstance<IMoveFormatter>();
            var check = ServiceLocator.Current.GetInstance<ICheckService>();
            var evaluation = ServiceLocator.Current.GetInstance<IEvaluationService>();

            var st = new TestStrategy(position);

            if (shouldPrintPosition)
            {
                log.WriteLine(position); 
            }
            log.WriteLine();

            TimeSpan total = TimeSpan.Zero;

            for (int i = 0; i < depth; i++)
            {
                var timer = new Stopwatch();
                timer.Start();
                var result = strategy.GetResult();
                var move = result.Move;
                if (move != null)
                {
                    position.Make(move);
                    timer.Stop();
                    log.WriteLine(formatter.Format(move));
                }
                else
                {
                    var pizdetsZdesNull = "Pizdets zdes NULL !!!";
                    Console.WriteLine(pizdetsZdesNull);
                    break;
                }

                var timerElapsed = timer.Elapsed;
                total += timerElapsed;
                var logMessage = $"{i + 1} - Elapsed {timerElapsed}, Total = {total}";
                log.WriteLine(logMessage);
                log.WriteLine($"Table = {strategy.Size}, Check = {check.Size}, Evaluation = {evaluation.Size}");
                Console.WriteLine($"{logMessage} Table = {strategy.Size}, Check = {check.Size}, Evaluation = {evaluation.Size}");

                var m = st.Get().Move;
                if (m == null)
                {
                    Console.WriteLine($"{i+1} The opponent has no moves !!!");
                    break;
                }
                position.Make(m);
                log.WriteLine(formatter.Format(m));
                if (shouldPrintPosition)
                {
                    log.WriteLine(position);
                }
                //Console.WriteLine(position);
                log.WriteLine();
                // Console.ReadLine();
            }
        }
    }
}