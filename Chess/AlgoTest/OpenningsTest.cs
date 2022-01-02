using System;
using System.Collections.Generic;
using System.IO;
using Algorithms.Strategies.Misc;
using CommonServiceLocator;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.Moves;
using Infrastructure.Positions;

namespace AlgoTest
{
    internal class OpenningsTest
    {
        public static void Opennings()
        {
            Position position = new Position();
            OpenningStrategy strategy = new OpenningStrategy(position);
            ICheckService checkService = ServiceLocator.Current.GetInstance<ICheckService>();
            IOpennigService opennigService = ServiceLocator.Current.GetInstance<IOpennigService>();
            IMoveFormatter formatter = ServiceLocator.Current.GetInstance<IMoveFormatter>();
            List<IMove> bestMoves = new List<IMove>();

            HashSet<byte>[] history = new HashSet<byte>[12];
            for (int i = 0; i < 12; i++)
            {
                history[i] = new HashSet<byte>(6);
            }

            int x = 0;
            using (var wr = new StreamWriter("Data\\Temp.txt") { AutoFlush = true })
            {
                foreach (var m1 in opennigService.GetMoves())
                {
                    var w1 = Do(position, m1, opennigService, formatter, history);
                    foreach (var m2 in opennigService.GetMoves())
                    {
                        var w2 = Do(position, m2, opennigService, formatter, history);

                        foreach (var m3 in opennigService.GetMoves())
                        {
                            var w3 = Do(position, m3, opennigService, formatter, history);

                            foreach (var m4 in opennigService.GetMoves())
                            {
                                var w4 = Do(position, m4, opennigService, formatter, history);

                                foreach (var m5 in opennigService.GetMoves())
                                {
                                    var w5 = Do(position, m5, opennigService, formatter, history);
                                    var now = DateTime.Now;

                                    var moves = strategy.GetResult();

                                    bestMoves.Clear();

                                    //foreach (var move in moves.Moves)
                                    //{
                                    //    if (move.IsAttack())
                                    //    {
                                    //        bestMoves.Add(move);
                                    //        continue;
                                    //    }

                                    //    switch (move.Figure)
                                    //    {
                                    //        case FigureKind.BlackPawn:

                                    //            bestMoves.Add(move);

                                    //            break;
                                    //        case FigureKind.BlackKnight:
                                    //            if (move.To.X != 0 && move.To.X != 7)
                                    //            {

                                    //                bestMoves.Add(move);
                                    //            }

                                    //            break;
                                    //        case FigureKind.BlackBishop:
                                    //            if (move.To.X != 0 && move.To.X != 7)
                                    //            {
                                    //                bestMoves.Add(move);
                                    //            }
                                    //            break;
                                    //        case FigureKind.BlackQueen:
                                    //            bestMoves.Add(move);
                                    //            break;
                                    //        case FigureKind.BlackKing:
                                    //            if (move.IsCastle())
                                    //            {
                                    //                bestMoves.Add(move);
                                    //            }

                                    //            break;
                                    //    }
                                    //}

                                    foreach (var move in bestMoves)
                                    {
                                        var w = $"[{formatter.Format(move)}]";
                                        wr.WriteLine($"{w1}1%{w2}2%{w3}3%{w4}4%{w5}5%{w}");
                                    }

                                    Console.WriteLine($"{++x} -> {DateTime.Now - now}");
                                    UnDo(opennigService, position, history, m5);
                                }

                                UnDo(opennigService, position, history, m4);

                                checkService.Clear();
                                strategy.Clear();
                            }
                            //Console.WriteLine(position);
                            //Console.ReadLine(); 
                            UnDo(opennigService, position, history, m3);

                            //GC.Collect();
                        }

                        UnDo(opennigService, position, history, m2);
                    }

                    UnDo(opennigService, position, history, m1);
                }
            }
        }

        private static void UnDo(IOpennigService opennigService, Position position, HashSet<byte>[] history, IMove m)
        {
            opennigService.Remove();
            position.UnMake();
            history[(int)m.Figure].Remove(m.To.Key);
        }

        private static string Do(Position position, IMove m, IOpennigService opennigService, IMoveFormatter formatter,
            HashSet<byte>[] history)
        {
            position.Make(m);
            opennigService.Add(m);
            var w = $"[{formatter.Format(m)}]";
            history[(int)m.Figure].Add(m.To.Key);
            return w;
        }
    }
}