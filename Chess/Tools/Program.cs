﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Common;
using Engine.Models.Boards;
using Engine.Models.Config;
using Engine.Models.Helpers;
using Newtonsoft.Json;

namespace Tools
{
    class Program
    {
        static void Main(string[] args)
        {
            Boot.SetUp();

            //var evaluationService = ServiceLocator.Current.GetInstance<IEvaluationService>();

            //StaticTableCollection tables = new StaticTableCollection();
            //for (int i = 0; i < 12; i++)
            //{
            //    Piece piece = (Piece) i;
            //    PieceStaticTable table = new PieceStaticTable(piece);

            //    for (int j = 0; j < 3; j++)
            //    {
            //        Phase phase = (Phase)j;
            //        table.AddPhase(phase);
            //        for (byte k = 0; k < 64; k++)
            //        {
            //            Square square = new Square(k);
            //            short value = (short) (evaluationService.GetValue(i, k, phase)/2);
            //            table.AddValue(phase, square.AsString(), value);
            //        }
            //    }

            //    tables.Add(piece, table);
            //}

            //var s = JsonConvert.SerializeObject(tables, Formatting.Indented);
            //File.WriteAllText("StaticTables.json",s);

            var x = File.ReadAllText(@"Config\StaticTables.json");
            var staticTableCollection = JsonConvert.DeserializeObject<StaticTableCollection>(x);

            Console.WriteLine(staticTableCollection);

            Console.ReadLine();
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