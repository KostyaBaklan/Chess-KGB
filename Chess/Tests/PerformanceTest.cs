using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Common;
using CommonServiceLocator;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Strategies;
using Engine.Strategies.AlphaBeta.Advanced;
using Engine.Strategies.AlphaBeta.Extended;
using Engine.Strategies.AlphaBeta.Extended.Heap;
using Engine.Strategies.AlphaBeta.Null;
using Engine.Strategies.AlphaBeta.Null.Heap;
using Engine.Strategies.AlphaBeta.Simple;
using Engine.Strategies.IterativeDeeping.Extended;
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

            Dictionary<string, IStrategy> strategies = new Dictionary<string, IStrategy>
            {
                {"ab_ss_hc", new AlphaBetaHistoryStrategy(depth, position)},
                {"ab_ss_hdc", new AlphaBetaHistoryDifferenceStrategy(depth, position)},
                {"ab_ss_dc", new AlphaBetaDifferenceStrategy(depth, position)},
                {"ab_ss_dhc", new AlphaBetaDifferenceHistoryStrategy(depth, position)},

                {"ab_es_hc", new AlphaBetaExtendedHistoryStrategy(depth, position)},
                {"ab_es_hdc", new AlphaBetaExtendedHistoryDifferenceStrategy(depth, position)},
                {"ab_es_dc", new AlphaBetaExtendedDifferenceStrategy(depth, position)},
                {"ab_es_dhc", new AlphaBetaExtendedDifferenceHistoryStrategy(depth, position)},

                {"ab_hs_hc", new AlphaBetaHeapHistoryStrategy(depth, position)},
                {"ab_hs_hdc", new AlphaBetaHeapHistoryDifferenceStrategy(depth, position)},
                {"ab_hs_dc", new AlphaBetaHeapDifferenceStrategy(depth, position)},
                {"ab_hs_dhc", new AlphaBetaHeapDifferenceHistoryStrategy(depth, position)},

                {"ab_as_hc", new AlphaBetaAdvancedHistoryStrategy(depth, position)},
                {"ab_as_hdc", new AlphaBetaAdvancedHistoryDifferenceStrategy(depth, position)},
                {"ab_as_dc", new AlphaBetaAdvancedDifferenceStrategy(depth, position)},
                {"ab_as_dhc", new AlphaBetaAdvancedDifferenceHistoryStrategy(depth, position)},

                {"abn_es_hc", new AlphaBetaNullHistoryStrategy(depth, position)},
                {"abn_es_hdc", new AlphaBetaNullHistoryDifferenceStrategy(depth, position)},
                {"abn_es_dc", new AlphaBetaNullDifferenceStrategy(depth, position)},
                {"abn_es_dhc", new AlphaBetaNullDifferenceHistoryStrategy(depth, position)},

                {"abn_hs_hc", new NullHeapHistoryStrategy(depth, position)},
                {"abn_hs_hdc", new NullHeapHistoryDifferenceStrategy(depth, position)},
                {"abn_hs_dc", new NullHeapDifferenceStrategy(depth, position)},
                {"abn_hs_dhc", new NullHeapDifferenceHistoryStrategy(depth, position)},

                {"id_es_hc", new IdExtendedHistoryStrategy(depth, position)},
                {"id_es_hdc", new IdExtendedHistoryDifferenceStrategy(depth, position)},
                {"id_es_dc", new IdExtendedDifferenceStrategy(depth, position)},
                {"id_es_dhc", new IdExtendedDifferenceHistoryStrategy(depth, position)}
            };

            IStrategy strategy = strategies[args[0]];
            _model.Strategy = strategy.ToString();

            var file = Path.Combine("Log", $"{strategy}_D{depth}_{DateTime.Now:hh_mm_ss_dd_MM_yyyy}.log");

            Play(iterations, strategy, position);

            _model.Calculate();

            var content = JsonConvert.SerializeObject(_model, Formatting.Indented);
            File.WriteAllText(file,content);

            //position.GetBoard().PrintCache(Path.Combine("Log", $"See_Cache_{strategy}_{DateTime.Now:hh_mm_ss_dd_MM_yyyy}.log"));
        }

        private static void Play(int depth, IStrategy strategy, IPosition position)
        {
            var formatter = ServiceLocator.Current.GetInstance<IMoveFormatter>();
            var check = ServiceLocator.Current.GetInstance<ICheckService>();
            var evaluation = ServiceLocator.Current.GetInstance<IEvaluationService>();
            var moveProvider = ServiceLocator.Current.GetInstance<IMoveProvider>();

            var st = new TestStrategy(position);

            position.Make(moveProvider.GetMoves(Piece.WhitePawn, Squares.E2, position.GetBoard()).FirstOrDefault(m=>m.To == Squares.E4));
            position.Make(moveProvider.GetMoves(Piece.BlackPawn, Squares.E7, position.GetBoard()).FirstOrDefault(m => m.To == Squares.E5));
            position.Make(moveProvider.GetMoves(Piece.WhiteKnight, Squares.G1, position.GetBoard()).FirstOrDefault(m => m.To == Squares.F3));
            position.Make(moveProvider.GetMoves(Piece.BlackKnight, Squares.B8, position.GetBoard()).FirstOrDefault(m => m.To == Squares.C6));
            position.Make(moveProvider.GetMoves(Piece.WhiteBishop, Squares.F1, position.GetBoard()).FirstOrDefault(m => m.To == Squares.B5));
            position.Make(moveProvider.GetMoves(Piece.BlackPawn, Squares.A7, position.GetBoard()).FirstOrDefault(m => m.To == Squares.A6));

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

                Console.WriteLine($"{logMessage} Table = {strategy.Size}, Check = {check.Size}, Evaluation = {evaluation.Size}, Memory = {memory}");

                moveModel.Table = strategy.Size;
                moveModel.Evaluation = evaluation.Size;
                moveModel.Memory = memory;

                var m = st.Get().Move;
                if (m == null)
                {
                    Console.WriteLine($"{i + 1} The opponent has no moves !!!");
                    break;
                }
                position.Make(m);

                moveModel.StaticValue = position.GetValue();
                moveModel.Material = position.GetStaticValue();

                moveModel.White = formatter.Format(move);
                moveModel.Black = formatter.Format(m);

                _model.Moves.Add(moveModel);
            }
        }
    }
}