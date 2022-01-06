using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Helpers;

namespace Tools
{
    class Program
    {
        static void Main(string[] args)
        {
            BitBoard whites = new BitBoard(0);
            BitBoard blacks = new BitBoard(0);
            var boards = new BitBoard[12];

        boards[Piece.WhitePawn.AsByte()] = boards[Piece.WhitePawn.AsByte()].Set(Squares.D4.AsInt()).Set(Squares.F4.AsInt());
        boards[Piece.WhiteKnight.AsByte()] = boards[Piece.WhiteKnight.AsByte()].Set(Squares.F3.AsInt());
        //    _boards[Piece.WhiteBishop.AsByte()] = _boards[Piece.WhiteBishop.AsByte()].Set(2, 5);
        //_boards[Piece.WhiteRook.AsByte()] = _boards[Piece.WhiteRook.AsByte()].Set(0, 7);
        //_boards[Piece.WhiteQueen.AsByte()] = _boards[Piece.WhiteQueen.AsByte()].Set(3);
        //_boards[Piece.WhiteKing.AsByte()] = _boards[Piece.WhiteKing.AsByte()].Set(4);

        whites = boards[Piece.WhitePawn.AsByte()] |
                  boards[Piece.WhiteKnight.AsByte()] |
                  boards[Piece.WhiteBishop.AsByte()] |
                  boards[Piece.WhiteRook.AsByte()] |
                  boards[Piece.WhiteQueen.AsByte()] |
                  boards[Piece.WhiteKing.AsByte()];

        boards[Piece.BlackPawn.AsByte()] =
            boards[Piece.BlackPawn.AsByte()].Set(Squares.E5.AsInt()).Set(Squares.F6.AsInt());
            //_boards[Piece.BlackRook.AsByte()] = _boards[Piece.BlackRook.AsByte()].Set(56, 63);
        boards[Piece.BlackKnight.AsByte()] = boards[Piece.BlackKnight.AsByte()].Set(Squares.C6.AsInt());
            //_boards[Piece.BlackBishop.AsByte()] = _boards[Piece.BlackBishop.AsByte()].Set(58, 61);
            //_boards[Piece.BlackQueen.AsByte()] = _boards[Piece.BlackQueen.AsByte()].Set(59);
            //_boards[Piece.BlackKing.AsByte()] = _boards[Piece.BlackKing.AsByte()].Set(60);

            blacks = boards[Piece.BlackPawn.AsByte()] |
                  boards[Piece.BlackRook.AsByte()] |
                  boards[Piece.BlackKnight.AsByte()] |
                  boards[Piece.BlackBishop.AsByte()] |
                  boards[Piece.BlackQueen.AsByte()] |
                  boards[Piece.BlackKing.AsByte()];

            BitBoard occupied = whites | blacks;

            int[] values = new[] {1, 1, 1, 1,3,3};
            PrintBits(occupied);

            int[] gain = new int[32];
            int d = 0;
            BitBoard mayXRay = boards[Piece.BlackPawn.AsByte()] |
                               boards[Piece.BlackRook.AsByte()] |
                               boards[Piece.BlackBishop.AsByte()] |
                               boards[Piece.BlackQueen.AsByte()] |
                               boards[Piece.WhitePawn.AsByte()] |
                               boards[Piece.WhiteBishop.AsByte()] |
                               boards[Piece.WhiteRook.AsByte()] |
                               boards[Piece.WhiteQueen.AsByte()];

            BitBoard fromSet = Squares.F4.AsBitBoard();
            Queue<BitBoard> set = new Queue<BitBoard>();
            set.Enqueue(Squares.F6.AsBitBoard());
            set.Enqueue(Squares.D4.AsBitBoard());
            set.Enqueue(Squares.C6.AsBitBoard());
            set.Enqueue(Squares.F3.AsBitBoard());
            set.Enqueue(new BitBoard(0));
            //set.Enqueue(Squares.F4.AsBitBoard());
            //set.Enqueue(Squares.F4.AsBitBoard());

            BitBoard attackers = ~(Squares.E5.AsBitBoard()) & occupied;
            gain[d] = values[d];

            do
            {
                d++;
                gain[d] = values[d] - gain[d - 1]; // speculative store, if defended
                if (Math.Max(-gain[d - 1], gain[d]) < 0) break; // pruning does not influence the result
                attackers ^= fromSet; // reset bit in set to traverse
                occupied ^= fromSet; // reset bit in temporary occupancy (for x-Rays)
                if ((fromSet & mayXRay).Any())
                {
                    //attackers |= ConsiderXrays(occupied);
                }

                fromSet = set.Dequeue();
            } while (fromSet.Any());

            while (--d>0)
            {
                var x = -gain[d - 1];
                var z = gain[d];
                gain[d - 1] = -Math.Max(x, z);
            }

            var value = gain[0];
            Console.WriteLine($"SEE Value = {value}");
            //do
            //{
            //    d++; // next depth and side
            //    gain[d] = value[aPiece] - gain[d - 1]; // speculative store, if defended
            //    if (max(-gain[d - 1], gain[d]) < 0) break; // pruning does not influence the result
            //    attadef ^= fromSet; // reset bit in set to traverse
            //    occ ^= fromSet; // reset bit in temporary occupancy (for x-Rays)
            //    if (fromSet & mayXray)
            //        attadef |= considerXrays(occ, ..);
            //    fromSet = getLeastValuablePiece(attadef, d & 1, aPiece);
            //} while (fromSet);
            //while (--d)
            //    gain[d - 1] = -max(-gain[d - 1], gain[d])
            //return gain[0];

            Console.ReadLine();
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