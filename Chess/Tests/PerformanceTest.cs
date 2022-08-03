using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Common;
using CommonServiceLocator;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using Engine.Strategies.Aspiration.Adaptive;
using Engine.Strategies.Aspiration.LateMove;
using Engine.Strategies.Aspiration.Null;
using Engine.Strategies.Base;
using Engine.Strategies.LateMove.Base;
using Engine.Strategies.LateMove.Base.Null;
using Engine.Strategies.LateMove.Deep;
using Engine.Strategies.LateMove.Deep.Null;
using Engine.Strategies.ProbCut;
using Newtonsoft.Json;

namespace Tests
{
    internal class PerformanceTest
    {
        private static readonly TestModel _model = new TestModel();

        public static void Test(string[] args)
        {
            if (!Directory.Exists("Log"))
            {
                Directory.CreateDirectory("Log");
            }

            var depth = short.Parse(args[1]);

            var iterations = int.Parse(args[2]);
            var game = args[3];
            //bool shouldPrintPosition = args.Length<=4 || bool.Parse(args[4]);

            _model.Depth = depth;
            _model.Game = game;

            var evaluationService = ServiceLocator.Current.GetInstance<IEvaluationService>();
            evaluationService.Initialize(depth);
            IPosition position = new Position();

            var moveProvider = ServiceLocator.Current.GetInstance<IMoveProvider>();

            Dictionary<string, List<MoveBase>> movesDictionary = new Dictionary<string, List<MoveBase>>
            {
                {
                    "king", new List<MoveBase>
                    {
                        moveProvider.GetMoves(Piece.WhitePawn, Squares.E2).FirstOrDefault(m => m.To == Squares.E4),
                        moveProvider.GetMoves(Piece.BlackPawn, Squares.E7).FirstOrDefault(m => m.To == Squares.E5)
                    }
                },
                {
                    "queen", new List<MoveBase>
                    {
                        moveProvider.GetMoves(Piece.WhitePawn, Squares.D2).FirstOrDefault(m => m.To == Squares.D4),
                        moveProvider.GetMoves(Piece.BlackKnight, Squares.G8).FirstOrDefault(m => m.To == Squares.F6)
                    }
                },
                {
                    "sicilian", new List<MoveBase>
                    {
                        moveProvider.GetMoves(Piece.WhitePawn, Squares.E2).FirstOrDefault(m => m.To == Squares.E4),
                        moveProvider.GetMoves(Piece.BlackPawn, Squares.C7).FirstOrDefault(m => m.To == Squares.C5)
                    }
                },
                {
                    "english", new List<MoveBase>
                    {
                        moveProvider.GetMoves(Piece.WhitePawn, Squares.C2).FirstOrDefault(m => m.To == Squares.C4),
                        moveProvider.GetMoves(Piece.BlackPawn, Squares.E7).FirstOrDefault(m => m.To == Squares.E5)
                    }
                }
            };

            var moves = movesDictionary[game];

            foreach (var moveBase in moves)
            {
                position.Make(moveBase);
            }

            Dictionary<string, Func<short, IPosition, StrategyBase>> strategyFactories =
                new Dictionary<string, Func<short, IPosition, StrategyBase>>
                {
                    {"lmr_bs_hc", (d, p) => new LmrBasicHistoryStrategy(d, p)},
                    {"lmr_es_hc", (d, p) => new LmrExtendedHistoryStrategy(d, p)},
                    {"lmr_as_hc", (d, p) => new LmrAdvancedHistoryStrategy(d, p)},

                    {"lmrd_bs_hc", (d, p) => new LmrDeepBasicStrategy(d, p)},
                    {"lmrd_es_hc", (d, p) => new LmrDeepExtendedStrategy(d, p)},
                    {"lmrd_as_hc", (d, p) => new LmrDeepAdvancedStrategy(d, p)},

                    {"lmrn_bs_hc", (d, p) => new LmrNullBasicStrategy(d, p)},
                    {"lmrn_es_hc", (d, p) => new LmrNullExtendedStrategy(d, p)},
                    {"lmrn_as_hc", (d, p) => new LmrNullAdvancedStrategy(d, p)},

                    {"lmrdn_bs_hc", (d, p) => new LmrDeepNullBasicStrategy(d, p)},
                    {"lmrdn_es_hc", (d, p) => new LmrDeepNullExtendedStrategy(d, p)},
                    {"lmrdn_as_hc", (d, p) => new LmrDeepNullAdvancedStrategy(d, p)},

                    {"lmranr_es_hc", (d, p) => new LmrAnrStrategy(d, p)},
                    {"lmrenr_es_hc", (d, p) => new LmrEnrStrategy(d, p)},

                    {"lmra_es_hc", (d, p) => new LmrAspirationExtendedStrategy(d, p)},
                    {"lmrda_es_hc", (d, p) => new LmrAspirationDeepExtendedStrategy(d, p)},
                    {"lmrdan_es_hc", (d, p) => new LmrAspirationDeepNullExtendedStrategy(d, p)},

                    {"lmrad_es_hc", (d, p) => new LmrAdaptiveAspirationStrategy(d, p)},
                    {"lmradn_es_hc", (d, p) => new LmrNullAdaptiveAspirationStrategy(d, p)},
                    {"lmradan_es_hc", (d, p) => new AnrAdaptiveAspirationStrategy(d, p)},
                    {"lmraden_es_hc", (d, p) => new EnrAdaptiveAspirationStrategy(d, p)},
                    {"lmrads_es_hc", (d, p) => new LmrSoftAdaptiveAspirationStrategy(d, p)},

                    {"pc_es_hc", (d, p) => new ProbCutStrategy(d, p)},
                    {"pcl_es_hc", (d, p) => new ProbCutLmrStrategy(d, p)},
                    {"pcld_es_hc", (d, p) => new ProbCutLmrDeepStrategy(d, p)}
                };

            StrategyBase strategy = strategyFactories[args[0]](depth, position);
            _model.Strategy = strategy.ToString();

            var file = Path.Combine("Log", $"{strategy}_D{depth}_{game}_{DateTime.Now:hh_mm_ss_dd_MM_yyyy}.log");

            Play(iterations, strategy, position);

            _model.Calculate();
            _model.Position = position.ToString();

            var content = JsonConvert.SerializeObject(_model, Formatting.Indented);
            File.WriteAllText(file,content, Encoding.BigEndianUnicode);

            //position.GetBoard().PrintCache(Path.Combine("Log", $"See_Cache_{strategy}_{DateTime.Now:hh_mm_ss_dd_MM_yyyy}.log"));
        }

        private static void Play(int depth, StrategyBase strategy, IPosition position)
        {
            var formatter = ServiceLocator.Current.GetInstance<IMoveFormatter>();
            var check = ServiceLocator.Current.GetInstance<ICheckService>();
            var evaluation = ServiceLocator.Current.GetInstance<IEvaluationService>();

            var st = new TestStrategy(position);

            TimeSpan total = TimeSpan.Zero;

            for (int i = 0; i < depth; i++)
            {
                var timer = new Stopwatch();
                timer.Start();
                while (strategy.IsBlocked())
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(0.01));
                }
                var result = strategy.GetResult();
                strategy.ExecuteAsyncAction();

                var move = result.Move;
                if (move != null)
                {
                    position.Make(move);
                    timer.Stop();
                }
                else
                {
                    var pizdetsZdesNull = "Pizdets zdes NULL !!!";
                    Console.WriteLine(pizdetsZdesNull);
                    Console.WriteLine($"Game Result = {result.GameResult} !!!");
                    Console.WriteLine(position);
                    break;
                }

                MoveModel moveModel = new MoveModel();
                var timerElapsed = timer.Elapsed;
                total += timerElapsed;
                var logMessage = $"{i + 1} - Elapsed {timerElapsed}, Total = {total}";

                moveModel.Number = i + 1;
                moveModel.Time = timerElapsed;

                var currentProcess = Process.GetCurrentProcess();
                var memory = currentProcess.WorkingSet64;

                Console.WriteLine($"{logMessage} Table = {strategy.Size}, Check = {check.Size}, Evaluation = {evaluation.Size}, Memory = {memory/1024} KB");

                moveModel.Table = strategy.Size;
                moveModel.Evaluation = evaluation.Size;
                moveModel.Memory = memory/1024;
                moveModel.White = formatter.Format(move);

                var m = st.Get().Move;
                if (m == null)
                {
                    Console.WriteLine($"{i + 1} The opponent has no moves !!!");
                }
                else
                {
                    position.Make(m);
                    moveModel.Black = formatter.Format(m);
                }

                moveModel.StaticValue = position.GetValue();
                moveModel.Material = position.GetStaticValue();

                _model.Moves.Add(moveModel);

                if (m == null) break;
            }
        }
    }
}