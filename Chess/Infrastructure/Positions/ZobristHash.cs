using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Infrastructure.DataStructures;
using Infrastructure.Helpers;

namespace Infrastructure.Positions
{
    public class ZobristHash
    {
        private readonly ulong[][] _table;

        public ZobristHash()
        {
            HashSet<ulong> set = new HashSet<ulong>();

            _table = new ulong[64][];
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    _table[i * 8 + j] = new ulong[12];
                    for (int k = 0; k < 12; k++)
                    {
                        var x = RandomHelpers.NextLong();
                        while (!set.Add(x))
                        {
                            x = RandomHelpers.NextLong();
                        }

                        _table[i * 8 + j][k] = x;
                    }
                }
            }
        }

        public ulong Key { get; private set; }

        public void Initialize(BoardSet[] map)
        {
            Key = 0L;
            for (var index = 0; index < map.Length; index++)
            {
                var set = map[index];
                foreach (var coordinate in set.Coordinates())
                {
                    Key = Key ^ _table[coordinate][index];
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update(byte coordinate, int figure)
        {
            Key = Key ^ _table[coordinate][figure];
        }
    }
}
