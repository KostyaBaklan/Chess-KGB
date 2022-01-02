using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using CommonServiceLocator;
using Infrastructure.DataStructures;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.Position;
using Infrastructure.Models.Enums;
using Infrastructure.Moves;
using Infrastructure.Positions;

namespace Infrastructure.Services
{
    public class FigureMap : IFigureMap
    {
        private readonly IMoveHistoryService _historyService;
        private bool _isWhiteCastled;
        private bool _isBlackCastled;
        private readonly int[] _values;
        //private readonly HashSet<Coordinate>[] _map;
        private readonly BoardSet[] _map;

        public FigureMap(IMoveHistoryService historyService)
        {
            _historyService = historyService;
            _values = new int[12];

            _values[(int) FigureKind.WhitePawn] = 1000;
            _values[(int) FigureKind.BlackPawn] = 1000;
            _values[(int) FigureKind.WhiteKnight] = 3125;
            _values[(int) FigureKind.BlackKnight] = 3125;
            _values[(int) FigureKind.WhiteBishop] = 3125;
            _values[(int) FigureKind.BlackBishop] = 3125;
            _values[(int) FigureKind.WhiteKing] = 0;
            _values[(int) FigureKind.BlackKing] = 0;
            _values[(int) FigureKind.WhiteRook] = 4875;
            _values[(int) FigureKind.BlackRook] = 4875;
            _values[(int) FigureKind.WhiteQueen] = 9875;
            _values[(int) FigureKind.BlackQueen] = 9875;

            _map = new BoardSet[12];
            for (int i = 0; i < 12; i++)
            {
                _map[i] = new BoardSet();
            }

            var coordinateProvider = ServiceLocator.Current.GetInstance<ICoordinateProvider>();
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    var c = coordinateProvider.GetCoordinate(x, y).Key;
                    if (y == 1)
                    {
                        _map[(int) FigureKind.WhitePawn].Add(c);
                    }
                    else if (y == 6)
                    {
                        _map[(int) FigureKind.BlackPawn].Add(c);
                    }
                    else if (y == 0)
                    {
                        if (x == 0 || x == 7)
                            _map[(int) FigureKind.WhiteRook].Add(c);
                        else if (x == 1 || x == 6)
                            _map[(int) FigureKind.WhiteKnight].Add(c);
                        else if (x == 2 || x == 5)
                            _map[(int) FigureKind.WhiteBishop].Add(c);
                        else if (x == 3)
                            _map[(int) FigureKind.WhiteQueen].Add(c);
                        else
                            _map[(int) FigureKind.WhiteKing].Add(c);
                    }
                    else if (y == 7)
                    {
                        if (x == 0 || x == 7)
                            _map[(int) FigureKind.BlackRook].Add(c);
                        else if (x == 1 || x == 6)
                            _map[(int) FigureKind.BlackKnight].Add(c);
                        else if (x == 2 || x == 5)
                            _map[(int) FigureKind.BlackBishop].Add(c);
                        else if (x == 3)
                            _map[(int) FigureKind.BlackQueen].Add(c);
                        else
                            _map[(int) FigureKind.BlackKing].Add(c);
                    }
                }
            }

            Hash = new ZobristHash();
            Hash.Initialize(_map);
        }

        #region Implementation of IFigureMap

        public ZobristHash Hash { get; }
        public Coordinate WhiteKingPosition => _map[(int) FigureKind.WhiteKing].Items().FirstOrDefault();
        public Coordinate BlackKingPosition => _map[(int) FigureKind.BlackKing].Items().FirstOrDefault();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetValue(IBoard board)
        {
            var value = 0;
            for (int i = 0; i < 6; i++)
            {
                value = value + _map[i].Count * _values[i];
            }

            for (int i = 6; i < 12; i++)
            {
                value = value - _map[i].Count * _values[i];
            }

            value = GetValue(FigureKind.WhitePawn, value, 125,375);
            value = GetValue(FigureKind.BlackPawn, value, -125,-375);

            var blockedPawns = GetWhiteBlockedPawns(board) - GetBlackBlockedPawns(board);
            value -= blockedPawns;

            if (_map[(int) FigureKind.WhiteBishop].Count < 2)
            {
                value -= 250;
            }
            if (_map[(int)FigureKind.BlackBishop].Count < 2)
            {
                value += 250;
            }

            value = EvaluateCastles(value);

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateCastles(int value)
        {
            if (_isWhiteCastled)
            {
                value += 250;
            }
            else
            {
                if (!_historyService.CanDoWhiteSmallCastle)
                {
                    value -= 125;
                }

                if (!_historyService.CanDoWhiteBigCastle)
                {
                    value -= 125;
                }
            }

            if (_isBlackCastled)
            {
                value -= 250;
            }
            else
            {
                if (!_historyService.CanDoBlackSmallCastle)
                {
                    value += 125;
                }

                if (!_historyService.CanDoBlackBigCastle)
                {
                    value += 125;
                }
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetBlackBlockedPawns(IBoard board)
        {
            int count = 0;
            var pawns = new int[] { -1, -1, -1, -1, -1, -1, -1, -1 };
            foreach (var coordinate in _map[(int)FigureKind.BlackPawn].Coordinates())
            {
                var x = coordinate / 8;
                if (pawns[x] < 0)
                {
                    pawns[x] = coordinate;
                    if (board.IsOpposite(coordinate - 1,false))
                    {
                        count++;
                    }
                }
                else
                {
                    if (coordinate < pawns[x])
                    {
                        pawns[x] = coordinate;
                    }
                    if (board.IsOpposite(coordinate - 1,false))
                    {
                        count++;
                    }
                }
            }

            return count * 125;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetWhiteBlockedPawns(IBoard board)
        {
            int count = 0;
            var pawns = new int[]{-1, -1, -1, -1, -1, -1, -1, -1};
            foreach (var coordinate in _map[(int)FigureKind.WhitePawn].Coordinates())
            {
                var x = coordinate / 8;
                if (pawns[x] < 0)
                {
                    pawns[x] = coordinate;
                    if (board.IsOpposite(coordinate + 1,true))
                    {
                        count++;
                    }
                }
                else
                {
                    if (coordinate > pawns[x])
                    {
                        pawns[x] = coordinate;
                    }
                    if (board.IsOpposite(coordinate + 1, true))
                    {
                        count++;
                    }
                }
            }

            return count * 125;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetValue(FigureKind figureKind, int value, int doubledPawn, int isolatedPawn)
        {
            var pawns = new int[8];
            foreach (var coordinate in _map[(int) figureKind].Coordinates())
            {
                pawns[coordinate / 8]++;
            }

            if (pawns[0] > 0)
            {
                if (pawns[0] > 1)
                {
                    value = value - doubledPawn;
                }

                if (pawns[1] == 0)
                {
                    value = value - isolatedPawn;
                }
            }

            if (pawns[7] > 0)
            {
                if (pawns[7] > 1)
                {
                    value = value - doubledPawn;
                }

                if (pawns[6] == 0)
                {
                    value = value - isolatedPawn;
                }
            }

            for (var i = 1; i < pawns.Length - 1; i++)
            {
                if (pawns[i] <= 0) continue;

                if (pawns[i] > 1)
                {
                    value -= doubledPawn;
                }

                if (pawns[i - 1] == 0 && pawns[i + 1] == 0)
                {
                    value = value - isolatedPawn;
                }
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(byte coordinate, FigureKind figure)
        {
            var i = (int) figure;
            Hash.Update(coordinate, i);
            _map[i].Add(coordinate);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(byte coordinate, FigureKind figure)
        {
            var i = (int) figure;
            Hash.Update(coordinate, i);
            _map[i].Remove(coordinate);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<Coordinate> GetWhiteCells()
        {
            List<Coordinate> cells = new List<Coordinate>(16);
            for (int i = 0; i < 6; i++)
            {
                cells.AddRange(_map[i].Items());
            }
            return cells;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<Coordinate> GetBlackCells()
        {
            List<Coordinate> cells = new List<Coordinate>(16);
            for (int i = 6; i < 12; i++)
            {
                cells.AddRange(_map[i].Items());
            }
            return cells;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WhiteCastle(bool isDo)
        {
            _isWhiteCastled = isDo;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BlackCastle(bool isDo)
        {
            _isBlackCastled = isDo;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<int> GetPositions(int figure)
        {
            return _map[figure].Coordinates();
        }

        #endregion
    }
}
