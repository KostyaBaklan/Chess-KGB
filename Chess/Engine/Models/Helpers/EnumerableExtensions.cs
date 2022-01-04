using System.Collections.Generic;
using System.Linq;

namespace Engine.Models.Helpers
{
    public static class EnumerableExtensions
    {
        public static T[] Slice<T>(this IEnumerable<T> source, int start, int length)
        {
            return source.Skip(start).Take(length).ToArray();
        }
    }
}
