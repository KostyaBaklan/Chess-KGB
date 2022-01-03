using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Engine.Models.Boards;
using Engine.Models.Helpers;

namespace Tools
{
    class Program
    {
        static void Main(string[] args)
        {
            BitBoard bitBoard = new BitBoard(0);
            var board = bitBoard.Set(Enumerable.Range(0,16).ToArray()).Set(Enumerable.Range(48, 16).ToArray());

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


            Console.ReadLine();
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