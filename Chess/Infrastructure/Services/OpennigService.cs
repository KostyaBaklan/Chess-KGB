using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Helpers;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.Moves;
using Infrastructure.Models;

namespace Infrastructure.Services
{
    public class OpennigService : IOpennigService
    {
        private int _current;
        private readonly Dictionary<IMove, Vertex> _graph;
        private readonly IMove[] _moves;

        public OpennigService(IMoveFormatter formatter)
        {
            _moves = new IMove[6];
            //var lines = File.ReadAllLines(@"Data\openings.txt");
            //_graph = new Dictionary<IMove, Vertex>();

            //Dictionary<string, string[]> fMoves = Split(lines, 1);

            //if (fMoves != null)
            //{
            //    foreach (var move in fMoves)
            //    {
            //        var key = move.Key.Split(new[] {'[', ']'}, StringSplitOptions.RemoveEmptyEntries)[0];
            //        Vertex vertex = new Vertex(key, 0, formatter);
            //        _graph.Add(vertex.Move, vertex);

            //        var f1 = Split(move.Value, 2);
            //        if (f1 == null) continue;

            //        foreach (var m1 in f1)
            //        {
            //            Vertex v1 = ParseMove(formatter, m1.Key, 1, vertex);
            //            var f2 = Split(m1.Value, 3);
            //            if (f2 != null)
            //            {
            //                foreach (var m2 in f2)
            //                {
            //                    Vertex v2 = ParseMove(formatter, m2.Key, 2, v1);
            //                    var f3 = Split(m2.Value, 4);
            //                    if (f3 != null)
            //                    {
            //                        foreach (var m3 in f3)
            //                        {
            //                            Vertex v3 = ParseMove(formatter, m3.Key, 3, v2);
            //                            var f4 = Split(m3.Value, 5);
            //                            if (f4 != null)
            //                            {
            //                                foreach (var m4 in f4)
            //                                {
            //                                    Vertex v4 = ParseMove(formatter, m4.Key, 4, v3);
            //                                    foreach (var s in m4.Value)
            //                                    {
            //                                        ParseMove(formatter, s, 5, v4);
            //                                    }
            //                                }
            //                            }
            //                            else
            //                            {
            //                                foreach (var s in m3.Value)
            //                                {
            //                                    ParseMove(formatter, s, 4, v3);
            //                                }
            //                            }

            //                        }
            //                    }
            //                    else
            //                    {
            //                        foreach (var s in m2.Value)
            //                        {
            //                            ParseMove(formatter, s, 3, v2);
            //                        }
            //                    }
            //                }
            //            }
            //            else
            //            {
            //                foreach (var s in m1.Value)
            //                {
            //                    ParseMove(formatter, s, 2, v1);
            //                }
            //            }
            //        }
            //    }
            //}
        }

        private static Vertex ParseMove(IMoveFormatter formatter, string s, int depth, Vertex parent)
        {
            var k = s.Split(new[] {'[', ']'}, StringSplitOptions.RemoveEmptyEntries)[0];
            Vertex vertex = new Vertex(k, depth, formatter);
            parent.Children.Add(vertex.Move, vertex);
            return vertex;
        }

        private static Dictionary<string, string[]> Split(string[] lines, int level)
        {
            RandomHelpers.Shuffle(lines);
            var l1 = lines[0].Split(new[] {'%'}, StringSplitOptions.RemoveEmptyEntries);
            var s = l1.Length - 1;
            if (s > 0)
            {
                return lines.Select(l => l.Split(new[] {$"{level}%"}, StringSplitOptions.RemoveEmptyEntries))
                    .GroupBy(g => g[0])
                    .ToDictionary(k => k.Key, v => v.Select(a => a[1]).ToArray());
            }

            return null;
        }

        public void Add(IMove move)
        {
            if (_current == 6)
            {

            }
            _moves[_current++] = move;
        }

        public void Remove()
        {
            if (_current == 0)
            {

            }
            _current--;
        }

        public ICollection<IMove> GetMoves()
        {
            Vertex vertex = null;
            for (var index = 0; index < _current; index++)
            {
                if (vertex == null)
                {
                    if (!_graph.TryGetValue(_moves[index], out vertex))
                    {
                        return new List<IMove>();
                    }
                }
                else
                {
                    if (!vertex.Children.TryGetValue(_moves[index], out vertex))
                    {
                        return new List<IMove>();
                    }
                }
            }

            return vertex == null ? _graph.Keys : vertex.Children.Keys;
        }
    }
}
