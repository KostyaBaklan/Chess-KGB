using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Common;
using CommonServiceLocator;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Moves;
using Engine.Sorting.Sorters;
using Engine.Strategies.Base;
using Newtonsoft.Json;

namespace OpeningTool
{
    class Program
    {
        static void Main(string[] args)
        {
            Boot.SetUp();

            var position = new Position();

            Stopwatch timer = Stopwatch.StartNew();

            var openingService = ServiceLocator.Current.GetInstance<IOpeningService>();
            var moveProvider = ServiceLocator.Current.GetInstance<IMoveProvider>();

            List<Sequence> sequences = new List<Sequence>();
            List<SequenceDescription> sequenceDescriptions = new List<SequenceDescription>();
            foreach (var pair in openingService.GetSequences())
            {
                Sequence sequence = new Sequence {Key = pair.Key};

                var list = pair.Key.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
                SequenceDescription sequenceDescription = new SequenceDescription { Key = CreateValue(list, moveProvider) };

                foreach (var k in pair.Value.Select(short.Parse))
                {
                    var m = moveProvider.Get(k);
                    if (m.IsAttack)
                    {
                        sequence.Attacks.Add(k);
                        sequenceDescription.Attacks.Add(m.ToString());
                    }
                    else
                    {
                        sequence.Moves.Add(k);
                        sequenceDescription.Moves.Add(m.ToString());
                    }
                }
                sequences.Add(sequence);
                sequenceDescriptions.Add(sequenceDescription);
            }

            var json = JsonConvert.SerializeObject(sequences, Formatting.Indented);
            File.WriteAllText("Sequences.json",json);

            json = JsonConvert.SerializeObject(sequenceDescriptions, Formatting.Indented);
            File.WriteAllText("SequenceDescriptions.json", json);

            var timerElapsed = timer.Elapsed;

            Console.WriteLine($"Finished = {timerElapsed}");

            Console.ReadLine();
            //try
            //{
            //    ProcessSequence(args);
            //}
            //catch (Exception)
            //{
            //    Console.WriteLine("0");
            //}

            //RemoveBadSeq();

            //RemoveBadPositions();

            //RemoveBad();

            //CheckBishopMoves();

            //CheckBadPawns();

            //CheckBadWhitePawns();

            //CheckBadBlackPawns();

            //CheckAllPawns();

            //CheckBadCenterPawns();

            //CheckBadWhiteCenterPawns();

            //CheckBadBlackCenterPawns();
        }

        //private static void CheckBadBlackCenterPawns()
        //{
        //    Boot.SetUp();

        //    var position = new Position();

        //    IMoveSorter sorter = new ExtendedSorter(position, new HistoryComparer());

        //    var moveProvider = ServiceLocator.Current.GetInstance<IMoveProvider>();


        //    Stopwatch timer = Stopwatch.StartNew();

        //    HashSet<string> candidates = new HashSet<string>();
        //    HashSet<string> bad = new HashSet<string>();
        //    HashSet<string> toSave = new HashSet<string>();
        //    int x = 0;
        //    HashSet<Square> whiteSet = new HashSet<Square>
        //    {
        //        Squares.A3, Squares.B3, Squares.C3, Squares.D3,Squares.E3, Squares.F3, Squares.G3, Squares.H3
        //    };
        //    HashSet<Square> blackSet = new HashSet<Square>
        //    {
        //        Squares.C5, Squares.D5,Squares.E5, Squares.F5
        //    };
        //    foreach (var keys in File.ReadLines(Path.Combine("Moves", "temp.txt")))
        //    {
        //        var strings = keys.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        //        var ms = new string[] { strings[1], strings[3] }.Select(s => moveProvider.Get(short.Parse(s))).ToList();
        //        if (ms.All(IsPawn) && ms.All(m => !blackSet.Contains(m.To)))
        //        {
        //            var item = CreateValue(strings, moveProvider);
        //            candidates.Add(item);
        //            bad.Add(CreateKey(strings));
        //        }
        //        else
        //        {
        //            toSave.Add(keys);
        //        }
        //    }

        //    BuildChangeCandidates("Seq_x.txt", bad);
        //    BuildChangeCandidates("BadPos.txt", candidates);
        //    BuildChangeCandidates("tempS.txt", toSave);

        //    var timerElapsed = timer.Elapsed;

        //    Console.WriteLine($"Finished = {timerElapsed}");

        //    Console.ReadLine();
        //}

        //private static void CheckBadWhiteCenterPawns()
        //{
        //    Boot.SetUp();

        //    var position = new Position();

        //    IMoveSorter sorter = new ExtendedSorter(position, new HistoryComparer());

        //    var moveProvider = ServiceLocator.Current.GetInstance<IMoveProvider>();


        //    Stopwatch timer = Stopwatch.StartNew();

        //    HashSet<string> candidates = new HashSet<string>();
        //    HashSet<string> bad = new HashSet<string>();
        //    HashSet<string> toSave = new HashSet<string>();
        //    int x = 0;
        //    HashSet<Square> whiteSet = new HashSet<Square>
        //    {
        //        Squares.C4, Squares.D4,Squares.E4, Squares.F4
        //    };
        //    HashSet<Square> blackSet = new HashSet<Square>
        //    {
        //        Squares.A6, Squares.B6, Squares.C6, Squares.D6,Squares.E6, Squares.F6, Squares.G6, Squares.H6
        //    };
        //    foreach (var keys in File.ReadLines(Path.Combine("Moves", "temp.txt")))
        //    {
        //        var strings = keys.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        //        var ms = new string[] { strings[0], strings[2], strings[4] }.Select(s => moveProvider.Get(short.Parse(s))).ToList();
        //        if (ms.All(IsPawn) && ms.All(m => !whiteSet.Contains(m.To)))
        //        {
        //            var item = CreateValue(strings, moveProvider);
        //            candidates.Add(item);
        //            bad.Add(CreateKey(strings));
        //        }
        //        else
        //        {
        //            toSave.Add(keys);
        //        }
        //    }

        //    BuildChangeCandidates("Seq_x.txt", bad);
        //    BuildChangeCandidates("BadPos.txt", candidates);
        //    BuildChangeCandidates("tempS.txt", toSave);

        //    var timerElapsed = timer.Elapsed;

        //    Console.WriteLine($"Finished = {timerElapsed}");

        //    Console.ReadLine();
        //}

        //private static void CheckBadBlackPawns()
        //{
        //    Boot.SetUp();

        //    var position = new Position();

        //    IMoveSorter sorter = new ExtendedSorter(position, new HistoryComparer());

        //    var moveProvider = ServiceLocator.Current.GetInstance<IMoveProvider>();


        //    Stopwatch timer = Stopwatch.StartNew();

        //    HashSet<string> candidates = new HashSet<string>();
        //    HashSet<string> bad = new HashSet<string>();
        //    HashSet<string> toSave = new HashSet<string>();
        //    int x = 0;
        //    HashSet<Square> whiteSet = new HashSet<Square>
        //    {
        //        Squares.A3, Squares.B3, Squares.C3, Squares.D3,Squares.E3, Squares.F3, Squares.G3, Squares.H3
        //    };
        //    HashSet<Square> blackSet = new HashSet<Square>
        //    {
        //        Squares.A6, Squares.B6, Squares.C6, Squares.D6,Squares.E6, Squares.F6, Squares.G6, Squares.H6
        //    };
        //    foreach (var keys in File.ReadLines(Path.Combine("Moves", "temp.txt")))
        //    {
        //        var strings = keys.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        //        var ms = new string[] { strings[1], strings[3] }.Select(s => moveProvider.Get(short.Parse(s))).ToList();
        //        if (ms.All(IsPawn) && ms.All(m => blackSet.Contains(m.To)))
        //        {
        //            var item = CreateValue(strings, moveProvider);
        //            candidates.Add(item);
        //            bad.Add(CreateKey(strings));
        //        }
        //        else
        //        {
        //            toSave.Add(keys);
        //        }
        //    }

        //    BuildChangeCandidates("Seq_x.txt", bad);
        //    BuildChangeCandidates("BadPos.txt", candidates);
        //    BuildChangeCandidates("tempS.txt", toSave);

        //    var timerElapsed = timer.Elapsed;

        //    Console.WriteLine($"Finished = {timerElapsed}");

        //    Console.ReadLine();
        //}

        //private static void CheckBadWhitePawns()
        //{
        //    Boot.SetUp();

        //    var position = new Position();

        //    IMoveSorter sorter = new ExtendedSorter(position, new HistoryComparer());

        //    var moveProvider = ServiceLocator.Current.GetInstance<IMoveProvider>();


        //    Stopwatch timer = Stopwatch.StartNew();

        //    HashSet<string> candidates = new HashSet<string>();
        //    HashSet<string> bad = new HashSet<string>();
        //    HashSet<string> toSave = new HashSet<string>();
        //    int x = 0;
        //    HashSet<Square> whiteSet = new HashSet<Square>
        //    {
        //        Squares.A3, Squares.B3, Squares.C3, Squares.D3,Squares.E3, Squares.F3, Squares.G3, Squares.H3
        //    };
        //    HashSet<Square> blackSet = new HashSet<Square>
        //    {
        //        Squares.A6, Squares.B6, Squares.C6, Squares.D6,Squares.E6, Squares.F6, Squares.G6, Squares.H6
        //    };
        //    foreach (var keys in File.ReadLines(Path.Combine("Moves", "temp.txt")))
        //    {
        //        var strings = keys.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        //        var ms = new string[]{strings[0],strings[2],strings[4]}.Select(s => moveProvider.Get(short.Parse(s))).ToList();
        //        if (ms.All(IsPawn) && ms.All(m=>whiteSet.Contains(m.To)))
        //        {
        //            var item = CreateValue(strings, moveProvider);
        //            candidates.Add(item);
        //            bad.Add(CreateKey(strings));
        //        }
        //        else
        //        {
        //            toSave.Add(keys);
        //        }
        //    }

        //    BuildChangeCandidates("Seq_x.txt", bad);
        //    BuildChangeCandidates("BadPos.txt", candidates);
        //    BuildChangeCandidates("tempS.txt", toSave);

        //    var timerElapsed = timer.Elapsed;

        //    Console.WriteLine($"Finished = {timerElapsed}");

        //    Console.ReadLine();
        //}

        //private static void CheckAllPawns()
        //{
        //    Boot.SetUp();

        //    var position = new Position();

        //    IMoveSorter sorter = new ExtendedSorter(position, new HistoryComparer());

        //    var moveProvider = ServiceLocator.Current.GetInstance<IMoveProvider>();


        //    Stopwatch timer = Stopwatch.StartNew();

        //    HashSet<string> candidates = new HashSet<string>();
        //    HashSet<string> bad = new HashSet<string>();
        //    HashSet<string> toSave = new HashSet<string>();
        //    int x = 0;
        //    HashSet<Square> whiteSet = new HashSet<Square>
        //    {
        //        Squares.A3, Squares.B3, Squares.C3, Squares.D3,Squares.E3, Squares.F3, Squares.G3, Squares.H3
        //    };
        //    HashSet<Square> blackSet = new HashSet<Square>
        //    {
        //        Squares.A6, Squares.B6, Squares.C6, Squares.D6,Squares.E6, Squares.F6, Squares.G6, Squares.H6
        //    };
        //    foreach (var keys in File.ReadLines(Path.Combine("Moves", "temp.txt")))
        //    {
        //        var strings = keys.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        //        var ms = strings.Select(s => moveProvider.Get(short.Parse(s))).ToList();
        //        if (ms.All(IsPawn))
        //        {
        //            var item = CreateValue(strings, moveProvider);
        //            candidates.Add(item);
        //            bad.Add(CreateKey(strings));
        //        }
        //        else
        //        {
        //            toSave.Add(keys);
        //        }
        //    }

        //    BuildChangeCandidates("Seq_x.txt", bad);
        //    BuildChangeCandidates("BadPos.txt", candidates);
        //    BuildChangeCandidates("tempS.txt", toSave);

        //    var timerElapsed = timer.Elapsed;

        //    Console.WriteLine($"Finished = {timerElapsed}");

        //    Console.ReadLine();
        //}

        //private static void CheckBadCenterPawns()
        //{
        //    Boot.SetUp();

        //    var position = new Position();

        //    IMoveSorter sorter = new ExtendedSorter(position, new HistoryComparer());

        //    var moveProvider = ServiceLocator.Current.GetInstance<IMoveProvider>();


        //    Stopwatch timer = Stopwatch.StartNew();

        //    HashSet<string> candidates = new HashSet<string>();
        //    HashSet<string> bad = new HashSet<string>();
        //    HashSet<string> toSave = new HashSet<string>();
        //    int x = 0;
        //    HashSet<Square> whiteSet = new HashSet<Square>
        //    {
        //        Squares.C4, Squares.D4,Squares.E4, Squares.F4
        //    };
        //    HashSet<Square> blackSet = new HashSet<Square>
        //    {
        //        Squares.C5, Squares.D5,Squares.E5, Squares.F5
        //    };
        //    foreach (var keys in File.ReadLines(Path.Combine("Moves", "temp.txt")))
        //    {
        //        var strings = keys.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        //        var ms = strings.Select(s => moveProvider.Get(short.Parse(s))).ToList();
        //        if (ms.All(IsPawn))
        //        {
        //            bool isBad = ms.Count(m=>!whiteSet.Contains(m.To)) <= 1 && ms.Count(m => !blackSet.Contains(m.To)) <=1;
        //            if (isBad)
        //            {
        //                var item = CreateValue(strings, moveProvider);
        //                candidates.Add(item);
        //                bad.Add(CreateKey(strings));
        //            }
        //            else
        //            {
        //                toSave.Add(keys);
        //            }
        //        }
        //        else
        //        {
        //            toSave.Add(keys);
        //        }
        //    }

        //    BuildChangeCandidates("Seq_x.txt", bad);
        //    BuildChangeCandidates("BadPos.txt", candidates);
        //    BuildChangeCandidates("tempS.txt", toSave);

        //    var timerElapsed = timer.Elapsed;

        //    Console.WriteLine($"Finished = {timerElapsed}");

        //    Console.ReadLine();
        //}

        //private static void CheckBadPawns()
        //{
        //    Boot.SetUp();

        //    var position = new Position();

        //    IMoveSorter sorter = new ExtendedSorter(position, new HistoryComparer());

        //    var moveProvider = ServiceLocator.Current.GetInstance<IMoveProvider>();


        //    Stopwatch timer = Stopwatch.StartNew();

        //    HashSet<string> candidates = new HashSet<string>();
        //    HashSet<string> bad = new HashSet<string>();
        //    HashSet<string> toSave = new HashSet<string>();
        //    int x = 0;
        //    HashSet<Square> whiteSet = new HashSet<Square>
        //    {
        //        Squares.A3, Squares.B3, Squares.C3, Squares.D3,Squares.E3, Squares.F3, Squares.G3, Squares.H3
        //    };
        //    HashSet<Square> blackSet = new HashSet<Square>
        //    {
        //        Squares.A6, Squares.B6, Squares.C6, Squares.D6,Squares.E6, Squares.F6, Squares.G6, Squares.H6
        //    };
        //    foreach (var keys in File.ReadLines(Path.Combine("Moves", "temp.txt")))
        //    {
        //        var strings = keys.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        //        var ms = strings.Select(s => moveProvider.Get(short.Parse(s))).ToList();
        //        if (ms.All(IsPawn))
        //        {
        //            bool isBad = true;
        //            for (var i = 0; i < ms.Count; i++)
        //            {
        //                if (i % 2 == 0)
        //                {
        //                    if (!whiteSet.Contains(ms[i].To))
        //                    {
        //                        isBad = false;
        //                        break;
        //                    }
        //                }
        //                else
        //                {
        //                    if (!blackSet.Contains(ms[i].To))
        //                    {
        //                        isBad = false;
        //                        break;
        //                    }
        //                }
        //            }
        //            if (isBad)
        //            {
        //                var item = CreateValue(strings, moveProvider);
        //                candidates.Add(item);
        //                bad.Add(CreateKey(strings));
        //            }
        //            else
        //            {
        //                toSave.Add(keys);
        //            }
        //        }
        //        else
        //        {
        //            toSave.Add(keys);
        //        }
        //    }

        //    BuildChangeCandidates("Seq_x.txt", bad);
        //    BuildChangeCandidates("BadPos.txt", candidates);
        //    BuildChangeCandidates("tempS.txt", toSave);

        //    var timerElapsed = timer.Elapsed;

        //    Console.WriteLine($"Finished = {timerElapsed}");

        //    Console.ReadLine();
        //}

        private static bool IsPawn(MoveBase moveBase)
        {
            return moveBase.Piece == Piece.WhitePawn || moveBase.Piece == Piece.BlackPawn;
        }

        //private static void CheckBishopMoves()
        //{
        //    Boot.SetUp();

        //    var position = new Position();

        //    IMoveSorter sorter = new ExtendedSorter(position, new HistoryComparer());

        //    var moveProvider = ServiceLocator.Current.GetInstance<IMoveProvider>();


        //    Stopwatch timer = Stopwatch.StartNew();

        //    HashSet<string> candidates = new HashSet<string>();
        //    HashSet<string> bad = new HashSet<string>();
        //    HashSet<string> toSave = new HashSet<string>();
        //    int x = 0;
        //    HashSet<Square> whiteSet = new HashSet<Square>
        //    {
        //        Squares.A3, Squares.H3, Squares.E3, Squares.D3
        //    };
        //    HashSet<Square> blackSet = new HashSet<Square>
        //    {
        //        Squares.A6, Squares.H6, Squares.E6, Squares.D6
        //    };
        //    foreach (var keys in File.ReadLines(Path.Combine("Moves", "temp.txt")))
        //    {
        //        var strings = keys.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        //        var moveBase = moveProvider.Get(short.Parse(strings.Last()));
        //        if (moveBase.Piece == Piece.WhiteBishop && whiteSet.Contains(moveBase.To))
        //        {
        //            var item = CreateValue(strings, moveProvider);
        //            candidates.Add(item);
        //            bad.Add(CreateKey(strings));
        //        }
        //        else if (moveBase.Piece == Piece.BlackBishop && blackSet.Contains(moveBase.To))
        //        {
        //            var item = CreateValue(strings, moveProvider);
        //            candidates.Add(item);
        //            bad.Add(CreateKey(strings));
        //        }
        //        else
        //        {
        //            toSave.Add(keys);
        //        }
        //    }

        //    BuildChangeCandidates("Seq_x.txt", bad);
        //    BuildChangeCandidates("BadPos.txt", candidates);
        //    BuildChangeCandidates("tempS.txt", toSave);

        //    var timerElapsed = timer.Elapsed;

        //    Console.WriteLine($"Finished = {timerElapsed}");

        //    Console.ReadLine();
        //}

        //private static void RemoveBad()
        //{
        //    Boot.SetUp();

        //    var position = new Position();

        //    IMoveSorter sorter = new ExtendedSorter(position, new HistoryComparer());

        //    var moveProvider = ServiceLocator.Current.GetInstance<IMoveProvider>();

        //    Stopwatch timer = Stopwatch.StartNew();

        //    HashSet<string> toRemove = new HashSet<string>();
        //    HashSet<string> toSave = new HashSet<string>();

        //    MoveFilter filter = new MoveFilter(moveProvider);

        //    foreach (var keys in File.ReadLines(Path.Combine("Moves", "temp.txt")))
        //    {
        //        var collection = keys.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        //        if (collection.Any(key => filter.IsBad(key)))
        //        {
        //            var value = CreateValue(collection, moveProvider);
        //            toRemove.Add(value);
        //        }
        //        else
        //        {
        //            toSave.Add(keys);
        //        }
        //    }

        //    BuildChangeCandidates("Change.txt", toRemove);
        //    BuildChangeCandidates("tempS.txt", toSave);

        //    //RemoveSpecifiedMoves(moveProvider);

        //    //var openingService = ServiceLocator.Current.GetInstance<IOpeningService>();

        //    //CheckSequences(openingService, moveProvider, position);

        //    var timerElapsed = timer.Elapsed;

        //    Console.WriteLine($"Finished = {timerElapsed}");

        //    Console.ReadLine();
        //}

        private static void RemoveBadSeq()
        {
            Boot.SetUp();

            Stopwatch timer = Stopwatch.StartNew();

            HashSet<string> candidates = new HashSet<string>();
            HashSet<string> toSave = new HashSet<string>();

            var xFiles = File.ReadLines(Path.Combine("Moves", "Seq_x.txt")).ToHashSet();

            foreach (var keys in File.ReadLines(Path.Combine("Moves", "temp.txt")))
            {
                if (xFiles.Any(x => keys.StartsWith(x)))
                {
                    candidates.Add(keys);
                }
                else
                {
                    toSave.Add(keys);
                }
            }

            BuildChangeCandidates("BadPos.txt", candidates);
            BuildChangeCandidates("tempS.txt", toSave);

            var timerElapsed = timer.Elapsed;

            Console.WriteLine($"Finished = {timerElapsed}");

            Console.ReadLine();
        }

        //private static void RemoveBadPositions()
        //{
        //    Boot.SetUp();

        //    var position = new Position();

        //    IMoveSorter sorter = new ExtendedSorter(position, new HistoryComparer());

        //    var moveProvider = ServiceLocator.Current.GetInstance<IMoveProvider>();

        //    Stopwatch timer = Stopwatch.StartNew();

        //    HashSet<string> candidates = new HashSet<string>();
        //    HashSet<string> bad = new HashSet<string>();
        //    HashSet<string> toSave = new HashSet<string>();

        //    foreach (var keys in File.ReadLines(Path.Combine("Moves", "temp.txt")))
        //    {
        //        var strings = keys.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        //        var moves = strings.Select(s => moveProvider.Get(short.Parse(s))).ToArray();
        //        foreach (var move in moves)
        //        {
        //            position.Make(move);
        //        }

        //        var v = Evaluate(short.MinValue, short.MaxValue, position, sorter);
        //        if (moves.Length % 2 == 0 && v < -80 || moves.Length % 2 == 1 && v > 100)
        //        {
        //            var item = $"{keys}-{CreateValue(moves.Select(m => m.Key.ToString()).ToList(), moveProvider)}-V={v}";
        //            candidates.Add(item);
        //            bad.Add(CreateKey(strings));
        //        }
        //        else
        //        {
        //            toSave.Add(keys);
        //        }

        //        foreach (var move in moves)
        //        {
        //            position.UnMake();
        //        }
        //    }

        //    BuildChangeCandidates("Seq_x.txt", bad);
        //    BuildChangeCandidates("BadPos.txt", candidates);
        //    BuildChangeCandidates("tempS.txt", toSave);

        //    var timerElapsed = timer.Elapsed;

        //    Console.WriteLine($"Finished = {timerElapsed}");

        //    Console.ReadLine();
        //}

        protected static int Evaluate(int alpha, int beta,IPosition position, IMoveSorter sorter)
        {
            int standPat = position.GetValue();
            if (standPat >= beta)
            {
                return beta;
            }

            if (alpha < standPat)
                alpha = standPat;

            var moves = position.GetAllAttacks(sorter);
            for (var i = 0; i < moves.Length; i++)
            {
                var move = moves[i];
                position.Make(move);

                int score = -Evaluate(-beta, -alpha, position, sorter);

                position.UnMake();

                if (score >= beta)
                {
                    return beta;
                }

                if (score > alpha)
                    alpha = score;
            }

            return alpha;
        }

        private static void CheckSequences(IOpeningService openingService, IMoveProvider moveProvider, Position position)
        {
            HashSet<string> toRemove = new HashSet<string>();
            HashSet<string> toThink = new HashSet<string>();
            foreach (var keys in openingService.GetMoveKeys())
            {
                if (keys.Count < 3) continue;

                foreach (var key in keys)
                {
                    var move = moveProvider.Get(key);
                    Console.Write(move);
                    position.Make(move);
                }

                Console.WriteLine(position);

                Console.WriteLine("Continue?");

                ConsoleKeyInfo info = Console.ReadKey();
                if (info.KeyChar == 'd')
                {
                    toRemove.Add(CreateKey(keys));
                }
                else if (info.KeyChar == 'm')
                {
                    toThink.Add(CreateKey(keys));
                }

                foreach (var key in keys)
                {
                    position.UnMake();
                }
            }
            //BuildSequenceFiles(openingService, moveProvider);

            BuildChangeCandidates("toRemove.txt", toRemove);
            BuildChangeCandidates("toThink.txt", toThink);
        }

        private static void BuildChangeCandidates(string path, IEnumerable<string> set)
        {
            File.WriteAllLines(Path.Combine("Moves", path),set);
        }

        private static void BuildSequenceFiles(IOpeningService openingService, IMoveProvider moveProvider)
        {
            int x = 0;
            using (var writer = new StreamWriter(Path.Combine("Moves", "Seq.txt")))
            {
                using (var mWriter = new StreamWriter(Path.Combine("Moves", "Mo.txt")))
                {
                    foreach (var pair in openingService.GetSequences())
                    {
                        writer.WriteLine($"{pair.Key}-{CreateKey(pair.Value)}");

                        var value =
                            $"{CreateValue(pair.Key.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries), moveProvider)}-{CreateValue(pair.Value, moveProvider)}";
                        mWriter.WriteLine(value);
                        Console.WriteLine($"{++x}");
                    }
                }
            }
        }

        private static string CreateKey<T>(ICollection<T> keys)
        {
            StringBuilder builder = new StringBuilder();

            foreach (var s in keys.Take(keys.Count - 1))
            {
                builder.Append($"{s},");
            }
            builder.Append($"{keys.Last()}");

            return builder.ToString();
        }

        //private static string CreateKey(ICollection<string> keys)
        //{
        //    StringBuilder builder = new StringBuilder();

        //    foreach (var s in keys.Take(keys.Count - 1))
        //    {
        //        builder.Append($"{s},");
        //    }
        //    builder.Append($"{keys.Last()}");

        //    return builder.ToString();
        //}

        private static string CreateValue(ICollection<string> keys, IMoveProvider moveProvider)
        {
            StringBuilder builder = new StringBuilder();

            foreach (var s in keys)
            {
                builder.Append(moveProvider.Get(short.Parse(s)));
            }

            return builder.ToString();
        }

        private static void ProcessSequence(string[] args)
        {
            if (args.Length < 1) throw new ArgumentException("Expected move sequence");

            var moveSequence = args[0].Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                .Select(short.Parse);

            //var file = Path.Combine("Log", $"{strategy}_D{depth}_{DateTime.Now:hh_mm_ss_dd_MM_yyyy}.opening");
            Boot.SetUp();

            short depth = 7;

            var evaluationService = ServiceLocator.Current.GetInstance<IEvaluationService>();
            evaluationService.Initialize(depth);

            IPosition position = new Position();

            var moveProvider = ServiceLocator.Current.GetInstance<IMoveProvider>();

            foreach (var key in moveSequence)
            {
                var move = moveProvider.Get(key);
                position.Make(move);
            }

            StrategyBase strategy = new OpeningStrategy(depth, position);
            var openingResult = strategy.GetResult() as OpeningResult;

            StringBuilder builder = new StringBuilder();
            for (var index = 0; index < openingResult.Moves.Count - 1; index++)
            {
                var move = openingResult.Moves[index];
                //Console.WriteLine($"Number = {index + 1}, Move = {move}, Key = {move.Key}, Sequence = {args[0]},{move.Key}");
                builder.Append($"{move.Key},");
            }

            builder.Append($"{openingResult.Moves.Last().Key}");

            Console.Write(builder);
        }
    }
}
