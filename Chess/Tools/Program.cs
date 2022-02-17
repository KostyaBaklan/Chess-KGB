using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Common;
using CommonServiceLocator;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Newtonsoft.Json;

namespace Tools
{
    public class TableConfiguration
    {
        public int Depth { get; set; }
        public int[] Values { get; set; }
    }
    class Program
    {
        static void Main(string[] args)
        {
            Boot.SetUp();
            var z = File.ReadAllText(@"Config\Table.json");
            var table = JsonConvert.DeserializeObject<Dictionary<int, TableConfiguration>>(z);

            foreach (var key in table.Keys)
            {
                var t = table[key];
                for (var i = 0; i < t.Values.Length; i++)
                {
                    if (t.Values[i] > 2)
                    {
                        t.Values[i] /= 2;
                    }
                }
            }

            var s = JsonConvert.SerializeObject(table, Formatting.Indented);
            File.WriteAllText("Table.json",s);

            Console.ReadLine();
        }

        private static void EvaluationTest()
        {
            var moveProvider = ServiceLocator.Current.GetInstance<IMoveProvider>();
            Dictionary<int, List<string>> attacks = new Dictionary<int, List<string>>();
            for (int i = 0; i < 64; i++)
            {
                var attackPattern = moveProvider.GetAttackPattern(Piece.WhiteQueen.AsByte(), i);
                var count = attackPattern.Count();
                if (attacks.ContainsKey(count))
                {
                    attacks[count].Add(new Square(i).AsString());
                }
                else
                {
                    attacks[count] = new List<string> {new Square(i).AsString()};
                }
            }

            foreach (var pair in attacks)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append($"{pair.Key} -> ");
                foreach (var s in pair.Value)
                {
                    builder.Append($"{s} ");
                }

                Console.WriteLine(builder);
            }

            //var x = File.ReadAllText(@"Config\StaticTables.json");
            //var collection = JsonConvert.DeserializeObject<StaticTableCollection>(x);
            //var whiteTable = collection.Values[Piece.WhiteKnight.AsByte()];
            //var blackTable = collection.Values[Piece.BlackKnight.AsByte()];

            //var values = new Dictionary<int,short>()
            //{
            //    {2,-10 },
            //    {3,-5 },
            //    {4,0 },
            //    {6,10 },
            //    {8,20 }
            //};

            //var labels = new Dictionary<char, char>()
            //{
            //    {'1','8' },
            //    {'2','7' },
            //    {'3','6' },
            //    {'4','5' },
            //    {'5','4' },
            //    {'6','3' },
            //    {'7','2' },
            //    {'8','1' }
            //};

            //for (int i = 0; i < 3; i++)
            //{
            //    PhaseStaticTable wb = whiteTable.Values[(Phase)i];
            //    PhaseStaticTable bb = blackTable.Values[(Phase)i];

            //    foreach (var pair in attacks)
            //    {
            //        foreach (var key in pair.Value)
            //        {
            //            wb.Values[key] = values[pair.Key];
            //            bb.Values[key] = values[pair.Key];
            //        }
            //    }

            //    //foreach (var pair in wb.Values)
            //    //{
            //    //    var l = pair.Key[0];
            //    //    var n = _labels[pair.Key[1]];
            //    //    var key = new string(new[] { l, n });
            //    //    bb.Values[key] = pair.Value;
            //    //}
            //}

            //var q = JsonConvert.SerializeObject(collection, Formatting.Indented);
            //File.WriteAllText(@"StaticTables.json", q);
        }

        private static bool IsIn(int i)
        {
            return i > -1 && i < 64;
        }
        private static  IEnumerable<int> KingMoves(int f)
        {
            if (f == 0)
            {
                return new[] { 1, 9, 8 };
            }

            if (f == 7)
            {
                return new[] { 6, 14, 15 };
            }
            if (f == 56)
            {
                return new[] { 48, 49, 57 };
            }
            if (f == 63)
            {
                return new[] { 62, 54, 55 };
            }

            if (f % 8 == 0) //B1 => A1,C1,B2,A2,C2
            {
                return new[] { f + 8, f + 9, f + 1, f - 7, f - 8 };
            }
            if (f % 8 == 7)//B8 => A8,C8,B7,A7,C7
            {
                return new[] { f + 8, f + 7, f - 1, f - 9, f - 8 };
            }

            if (f / 8 == 0)
            {
                return new[] { f + 1, f - 1, f + 7, f + 9, f + 8 };
            }
            if (f / 8 == 7)
            {
                return new[] { f + 1, f - 7, f - 1, f - 9, f - 8 };
            }

            return new[] { f + 8, f + 7, f - 1, f + 9, f + 1, f - 9, f - 7, f - 8 };
        }

        private static void MagicBbTest()
        {
            BitBoard bitBoard = new BitBoard(0);
            var board = bitBoard.Set(Enumerable.Range(0, 16).ToArray()).Set(Enumerable.Range(48, 16).ToArray());

            var asBitBoard = Squares.B7.AsBitBoard();

            var bishopAttacks = Squares.A4.BishopAttacks(board);
            PrintBits(bishopAttacks);
            var isSet = bishopAttacks.IsSet(asBitBoard);
            Console.WriteLine(isSet);

            var rookAttacks = Squares.B5.RookAttacks(board);
            PrintBits(rookAttacks);
            isSet = rookAttacks.IsSet(asBitBoard);
            Console.WriteLine(isSet);

            var queenAttacks = Squares.C3.QueenAttacks(board);
            PrintBits(queenAttacks);
            isSet = queenAttacks.IsSet(asBitBoard);
            Console.WriteLine(isSet);
        }

        private static void TestBitExtensions()
        {
            BitBoard bitBoard = new BitBoard(0);
            var board = bitBoard.Set(8, 9, 10, 11, 12, 13, 14, 15);

            PrintBits(board);

            board = board.Remove(12).Set(12 + 16);

            PrintBits(board);

            board = board.Remove(9).Set(9 + 8);

            PrintBits(board);

            board = board.Remove(15).Set(15 + 7);

            PrintBits(board);
        }

        private static void PrintBits(BitBoard board)
        {
            Console.WriteLine(board);
            Console.WriteLine(board.ToBitString());

            foreach (var i in board.BitScan())
            {
                Console.WriteLine(i);
            }
        }

        private static void SquareTest()
        {
            List<string> strings = new List<string>(64);
            for (int i = 0; i < 64; i++)
            {
                var s = new Square(i).AsString();
                var line = $"public static string {s} = \"{s}\";";
                strings.Add(line);
            }

            foreach (var s in strings)
            {
                Console.WriteLine(s);
            }

            File.WriteAllLines("yalla.txt", strings);
        }
    }
}