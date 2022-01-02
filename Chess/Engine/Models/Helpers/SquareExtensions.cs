using System.Runtime.CompilerServices;
using Engine.Models.Boards;

namespace Engine.Models.Helpers
{
    public static class SquareExtensions
    {
        private static readonly string[] _names = new string[64];
        private static readonly BitBoard[] _values = new BitBoard[64];

        static SquareExtensions()
        {
            string Ab = "ABCDEFGH";
            string n = "12345678";

            int index = 0;
            foreach (var d in n)
            {
                foreach (var t in Ab)
                {
                    _names[index] = $"{t}{d}";
                    index++;
                }
            }

            for (int i = 0; i < 64; i++)
            {
                _values[i] = new BitBoard(1ul << i);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string AsString(this Square square)
        {
            return _names[square.AsByte()];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BitBoard AsBitBoard(this int square)
        {
            return _values[square];
        }
    }
}