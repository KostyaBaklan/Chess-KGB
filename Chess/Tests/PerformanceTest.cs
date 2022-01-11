using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using CommonServiceLocator;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Strategies;
using Engine.Strategies.AlphaBeta;

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
                {"aa", new AlphaBetaAdvancedStrategy(depth,position) }
                //{"av", new AspirationValueStrategy(depth,position)},
                //{"p", new PvsKillerStrategy(depth,position)},
                //{"ps", new PvStaticStrategy(depth,position)},
                //{"pv", new PvValueStrategy(depth,position)},
                //{"psv", new PvStaticValueStrategy(depth,position)},
                //{"pvs", new PvValueStaticStrategy(depth,position)},
                //{"pfs", new PvFigureStaticStrategy(depth,position)},
                //{"pfv", new PvFigureValueStrategy(depth,position)},
                //{"mk", new MtdKillerStrategy(depth,position)},
                //{"mv", new MtdValueStrategy(depth,position)},
                //{"is", new IterativeStaticStrategy(depth,position)},
                //{"iv", new IterativeValueStrategy(depth,position)},
                //{"ifs", new IterativeFigureStaticStrategy(depth,position)},
                //{"ifv", new IterativeFigureValueStrategy(depth,position)},
                //{"sv", new ScoutValueStrategy(depth,position)},
                //{"ss", new ScoutStaticStrategy(depth,position)},
                //{"svs", new ScoutValueStaticStrategy(depth,position)},
                //{"ssv", new ScoutStaticValueStrategy(depth,position)},
                //{"sfv", new ScoutFigureValueStrategy(depth,position)},
                //{"sfs", new ScoutFigureStaticStrategy(depth,position)},
                //{"sfvs", new ScoutFigureValueStaticStrategy(depth,position)},
                //{"sfsv", new ScoutFigureStaticValueStrategy(depth,position)},
                //{"isv", new IterativeScoutValueStrategy(depth,position)},
                //{"iss", new IterativeScoutStaticStrategy(depth,position)},
                //{"isfv", new IterativeScoutFigureValueStrategy(depth,position)},
                //{"isfs", new IterativeScoutFigureStaticStrategy(depth,position)}
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
                    PrintKey(position, move, log);
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

        private static void PrintKey(IPosition position, IMove move, StreamWriter log)
        {
            var p1 = position.GetKey();
            position.UnMake();
            var p2 = position.GetKey();
            position.Make(move);
            var p3 = position.GetKey();
            if (p1 != p3)
            {
                throw new Exception("Pizdetz!!!");
            }
            log.WriteLine($"{p1} {p2} {p3}");
            log.WriteLine();
        }
    }
}