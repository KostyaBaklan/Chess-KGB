using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Engine.Models.Boards;
using Engine.Models.Helpers;

namespace Engine.DataStructures.Hash
{
    public class ZobristHash
    {
        private ulong _key;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong Key()
        {
            return _key;
        }

        public void Initialize(BitBoard[] map)
        {
            _key = 0L;
            for (var index = 0; index < map.Length; index++)
            {
                var set = map[index];
                var coordinates = set.Coordinates(index);
                for (var i = 0; i < coordinates.Count; i++)
                {
                    var coordinate = coordinates[i];
                    _key = _key ^ _table[coordinate][index];
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update(byte coordinate, int figure)
        {
            _key = _key ^ _table[coordinate][figure];
        }

        public void Update(byte from, byte to, byte figure)
        {
            _key = _key ^ _table[from][figure] ^ _table[to][figure];
        }
    }
}
