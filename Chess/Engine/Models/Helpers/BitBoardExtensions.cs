using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Engine.DataStructures;
using Engine.Models.Boards;

namespace Engine.Models.Helpers
{
    public static class BitBoardExtensions
    {
        private const ulong _magic = 0x07EDD5E59A4E28C2;

        private static readonly int[] _magicTable;
        private static readonly DynamicArray<int>[] _positions;

        static BitBoardExtensions()
        {
            _magicTable = new int[64];
            _positions = new DynamicArray<int>[12];
            for (var x = 0; x < _positions.Length; x++)
            {
                _positions[x] = new DynamicArray<int>();
            }

            ulong bit = 1;
            int i = 0;
            do
            {
                _magicTable[(bit * _magic) >> 58] = i;
                i++;
                bit <<= 1;
            } while (bit != 0);

            var set = _magicTable.ToHashSet();
            if (set.Count < 64)
            {
                throw new Exception();
            }

            if (set.Min() != 0)
            {
                throw new Exception();
            }
            if (set.Max() != 63)
            {
                throw new Exception();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<int> BitScan(this BitBoard b)
        {
            while (!b.IsZero())
            {
                int position = BitScanForward(b);
                yield return position;
                b = b.Remove(position);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BitScanForward(this BitBoard b)
        {
            return _magicTable[(b.Lsb() * _magic) >> 58];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Count(this BitBoard b)
        {
            int count = 0;
            while (!b.IsZero())
            {
                int position = BitScanForward(b);
                count++;
                b = b.Remove(position);
            }

            return count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DynamicArray<int> Coordinates(this BitBoard b, int index)
        {
            var coordinates = _positions[index];
            coordinates.Clear();
            while (!b.IsZero())
            {
                int position = BitScanForward(b);
                coordinates.Add(position);
                b = b.Remove(position);
            }

            return coordinates;
        }

        public static string ToBitString(this BitBoard b)
        {
            StringBuilder builder = new StringBuilder();

            BitBoard mask = new BitBoard(1);

            for (int i = 63; i >= 0; i--)
            {
                var x = b >> i;
                x = x & mask;
                if (x.IsZero())
                {
                    builder.Append('0');
                }
                else if (x == mask)
                {
                    builder.Append('1');
                }
                else
                {
                    throw new Exception("Dabeg");
                }
            }

            return builder.ToString();
        }
    }
}