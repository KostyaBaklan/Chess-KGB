using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Common;
using CommonServiceLocator;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Strategies.Aspiration.LateMove;
using Engine.Strategies.Base;
using Engine.Strategies.LateMove.Base;
using Engine.Strategies.LateMove.Base.Null;
using Engine.Strategies.LateMove.Deep;
using Engine.Strategies.LateMove.Deep.Null;
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
            //bool shouldPrintPosition = args.Length<=4 || bool.Parse(args[4]);

            _model.Depth = depth;

            var evaluationService = ServiceLocator.Current.GetInstance<IEvaluationService>();
            evaluationService.Initialize(depth);
            IPosition position = new Position();

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

                    {"lmra_es_hc", (d, p) => new LmrAspirationExtendedStrategy(d, p)},
                    {"lmrda_es_hc", (d, p) => new LmrAspirationDeepExtendedStrategy(d, p)}
                };
            //Dictionary<string, StrategyBase> strategies = new Dictionary<string, StrategyBase>
            //{
            //    {"ab_es_hc", new AlphaBetaExtendedHistoryStrategy(depth, position)},
            //    {"ab_es_hdc", new AlphaBetaExtendedHistoryDifferenceStrategy(depth, position)},
            //    {"ab_es_dc", new AlphaBetaExtendedDifferenceStrategy(depth, position)},
            //    {"ab_es_dhc", new AlphaBetaExtendedDifferenceHistoryStrategy(depth, position)},

            //    {"ab_as_hc", new AlphaBetaAdvancedHistoryStrategy(depth, position)},
            //    {"ab_as_hdc", new AlphaBetaAdvancedHistoryDifferenceStrategy(depth, position)},
            //    {"ab_as_dc", new AlphaBetaAdvancedDifferenceStrategy(depth, position)},
            //    {"ab_as_dhc", new AlphaBetaAdvancedDifferenceHistoryStrategy(depth, position)},

            //    {"abn_es_hc", new AlphaBetaNullHistoryStrategy(depth, position)},
            //    {"abn_es_hdc", new AlphaBetaNullHistoryDifferenceStrategy(depth, position)},
            //    {"abn_es_dc", new AlphaBetaNullDifferenceStrategy(depth, position)},
            //    {"abn_es_dhc", new AlphaBetaNullDifferenceHistoryStrategy(depth, position)},

            //    {"abn_as_hc", new AlphaBetaAdvancedNullHistoryStrategy(depth, position)},
            //    {"abn_as_hdc", new AlphaBetaAdvancedNullHistoryDifferenceStrategy(depth, position)},
            //    {"abn_as_dc", new AlphaBetaAdvancedNullDifferenceStrategy(depth, position)},
            //    {"abn_as_dhc", new AlphaBetaAdvancedNullDifferenceHistoryStrategy(depth, position)},

            //    {"abn_cs_hc", new AlphaBetaComplexNullHistoryStrategy(depth, position)},
            //    {"abn_cs_hdc", new AlphaBetaComplexNullHistoryDifferenceStrategy(depth, position)},

            //    {"id_es_hc", new IdExtendedHistoryStrategy(depth, position)},
            //    {"id_es_hdc", new IdExtendedHistoryDifferenceStrategy(depth, position)},
            //    {"id_es_dc", new IdExtendedDifferenceStrategy(depth, position)},
            //    {"id_es_dhc", new IdExtendedDifferenceHistoryStrategy(depth, position)},

            //    {"a_es_hc", new AspirationHistoryStrategy(depth, position)},
            //    {"a_es_hdc", new AspirationHistoryDifferenceStrategy(depth, position)},
            //    {"a_es_dc", new AspirationDifferenceStrategy(depth, position)},
            //    {"a_es_dhc", new AspirationDifferenceHistoryStrategy(depth, position)},

            //    {"pv_es_hc", new PvsHistoryStrategy(depth, position)},
            //    {"pv_es_hdc", new PvsHistoryDifferenceStrategy(depth, position)},
            //    {"pv_es_dc", new PvsDifferenceStrategy(depth, position)},
            //    {"pv_es_dhc", new PvsDifferenceHistoryStrategy(depth, position)},

            //    {"pvm_es_hc", new PvsMemoryHistoryStrategy(depth, position)},
            //    {"pvm_es_hdc", new PvsMemoryHistoryDifferenceStrategy(depth, position)},
            //    {"pvm_es_dc", new PvsMemoryDifferenceStrategy(depth, position)},
            //    {"pvm_es_dhc", new PvsMemoryDifferenceHistoryStrategy(depth, position)},

            //    {"lmr_bs_hc", new LmrBasicHistoryStrategy(depth, position)},
            //    {"lmr_es_hc", new LmrExtendedHistoryStrategy(depth, position)},
            //    {"lmr_es_hdc", new LmrExtendedHistoryDifferenceStrategy(depth, position)},
            //    {"lmr_as_hc", new LmrAdvancedHistoryStrategy(depth, position)},
            //    {"lmr_as_hdc", new LmrAdvancedHistoryDifferenceStrategy(depth, position)},
            //    {"lmr_cs_hc", new LmrComplexHistoryStrategy(depth, position)},
            //    {"lmr_cs_hdc", new LmrComplexHistoryDifferenceStrategy(depth, position)},

            //    {"nmr_es_hc", new NmrExtendedHistoryStrategy(depth, position)},
            //    {"nmr_es_hdc", new NmrExtendedHistoryDifferenceStrategy(depth, position)},
            //    {"nmr_as_hc", new NmrAdvancedHistoryStrategy(depth, position)},
            //    {"nmr_as_hdc", new NmrAdvancedHistoryDifferenceStrategy(depth, position)},
            //    {"nmr_cs_hc", new NmrComplexHistoryStrategy(depth, position)},
            //    {"nmr_cs_hdc", new NmrComplexHistoryDifferenceStrategy(depth, position)},

            //    {"mc_es_hc", new MultiCutExtendedHistoryStrategy(depth, position)},
            //    {"mc_es_hdc", new MultiCutExtendedHistoryDifferenceStrategy(depth, position)},
            //    {"mc_as_hc", new MultiCutAdvancedHistoryStrategy(depth, position)},
            //    {"mc_as_hdc", new MultiCutAdvancedHistoryDifferenceStrategy(depth, position)},
            //    {"mc_cs_hc", new MultiCutComplexHistoryStrategy(depth, position)},
            //    {"mc_cs_hdc", new MultiCutComplexHistoryDifferenceStrategy(depth, position)},

            //    {"lmrd_bs_hc", new LmrDeepBasicStrategy(depth, position)},
            //    {"lmrd_es_hc", new LmrDeepExtendedStrategy(depth, position)},
            //    {"lmrd_as_hc", new LmrDeepAdvancedStrategy(depth, position)},

            //    {"lmrn_bs_hc", new LmrNullBasicStrategy(depth, position)},
            //    {"lmrn_es_hc", new LmrNullExtendedStrategy(depth, position)},
            //    {"lmrn_as_hc", new LmrNullAdvancedStrategy(depth, position)},

            //    {"lmrnd_bs_hc", new LmrDeepNullBasicStrategy(depth, position)},
            //    {"lmrnd_es_hc", new LmrDeepNullExtendedStrategy(depth, position)},
            //    {"lmrnd_as_hc", new LmrDeepNullAdvancedStrategy(depth, position)}
            //};

            StrategyBase strategy = strategyFactories[args[0]](depth, position);
            _model.Strategy = strategy.ToString();

            var file = Path.Combine("Log", $"{strategy}_D{depth}_{DateTime.Now:hh_mm_ss_dd_MM_yyyy}.log");

            Play(iterations, strategy, position);

            _model.Calculate();

            var content = JsonConvert.SerializeObject(_model, Formatting.Indented);
            File.WriteAllText(file,content);

            //position.GetBoard().PrintCache(Path.Combine("Log", $"See_Cache_{strategy}_{DateTime.Now:hh_mm_ss_dd_MM_yyyy}.log"));
        }

        private static void Play(int depth, StrategyBase strategy, IPosition position)
        {
            var formatter = ServiceLocator.Current.GetInstance<IMoveFormatter>();
            var check = ServiceLocator.Current.GetInstance<ICheckService>();
            var evaluation = ServiceLocator.Current.GetInstance<IEvaluationService>();
            var moveProvider = ServiceLocator.Current.GetInstance<IMoveProvider>();

            var st = new TestStrategy(position);

            position.Make(moveProvider.GetMoves(Piece.WhitePawn, Squares.E2).FirstOrDefault(m=>m.To == Squares.E4));
            position.Make(moveProvider.GetMoves(Piece.BlackPawn, Squares.E7).FirstOrDefault(m => m.To == Squares.E5));
            position.Make(moveProvider.GetMoves(Piece.WhiteKnight, Squares.G1).FirstOrDefault(m => m.To == Squares.F3));
            position.Make(moveProvider.GetMoves(Piece.BlackKnight, Squares.B8).FirstOrDefault(m => m.To == Squares.C6));
            position.Make(moveProvider.GetMoves(Piece.WhiteBishop, Squares.F1).FirstOrDefault(m => m.To == Squares.B5));
            position.Make(moveProvider.GetMoves(Piece.BlackPawn, Squares.A7).FirstOrDefault(m => m.To == Squares.A6));
            position.Make(moveProvider.GetMoves(Piece.WhiteBishop, Squares.B5).FirstOrDefault(m => m.To == Squares.A4));
            position.Make(moveProvider.GetMoves(Piece.BlackPawn, Squares.B7).FirstOrDefault(m => m.To == Squares.B5));
            position.Make(moveProvider.GetMoves(Piece.WhiteBishop, Squares.A4).FirstOrDefault(m => m.To == Squares.B3));
            position.Make(moveProvider.GetMoves(Piece.BlackBishop, Squares.F8).FirstOrDefault(m => m.To == Squares.C5));

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