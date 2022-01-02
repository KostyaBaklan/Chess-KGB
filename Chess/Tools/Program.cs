using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Models.Boards;
using Engine.Models.Helpers;

namespace Tools
{
    class Program
    {
        static void Main(string[] args)
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
            File.WriteAllLines("yalla.txt",strings);
            Console.ReadLine();
        }
    }
}
