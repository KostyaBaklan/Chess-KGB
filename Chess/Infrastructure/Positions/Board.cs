using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using CommonServiceLocator;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.Moves;
using Infrastructure.Interfaces.Position;
using Infrastructure.Models.Enums;
using Infrastructure.Moves;

namespace Infrastructure.Positions
{
    public class Board : IBoard
    {
        private readonly FigureKind?[] _board;
        private readonly bool[,] _overBoard;

        private readonly IFigureMap _map;
        private readonly string[] _textValues;
        private readonly IMoveHistoryService _moveHistory;
        private readonly byte[] _values;
        private IMoveProvider _moveProvider;

        public Board()
        {
            _textValues = new string[12];
            _textValues[(int)FigureKind.WhitePawn] = "wp";
            _textValues[(int)FigureKind.BlackPawn] = "bp";
            _textValues[(int)FigureKind.WhiteKnight] = "wN";
            _textValues[(int)FigureKind.BlackKnight] = "bN";
            _textValues[(int)FigureKind.WhiteBishop] = "wB";
            _textValues[(int)FigureKind.BlackBishop] = "bB";
            _textValues[(int)FigureKind.WhiteKing] = "wK";
            _textValues[(int)FigureKind.BlackKing] = "bK";
            _textValues[(int)FigureKind.WhiteRook] = "wR";
            _textValues[(int)FigureKind.BlackRook] = "bR";
            _textValues[(int)FigureKind.WhiteQueen] = "wQ";
            _textValues[(int)FigureKind.BlackQueen] = "bQ";

            _values = new byte[12];

            _values[(int)FigureKind.WhitePawn] = 1;
            _values[(int)FigureKind.BlackPawn] = 1;
            _values[(int)FigureKind.WhiteKnight] = 2;
            _values[(int)FigureKind.BlackKnight] = 2;
            _values[(int)FigureKind.WhiteBishop] = 3;
            _values[(int)FigureKind.BlackBishop] = 3;
            _values[(int)FigureKind.WhiteKing] = 0;
            _values[(int)FigureKind.BlackKing] = 0;
            _values[(int)FigureKind.WhiteRook] = 4;
            _values[(int)FigureKind.BlackRook] = 4;
            _values[(int)FigureKind.WhiteQueen] = 5;
            _values[(int)FigureKind.BlackQueen] = 5;

            //_whites = new[]
            //{
            //    FigureKind.WhiteKnight, FigureKind.WhiteBishop, FigureKind.WhitePawn, FigureKind.WhiteQueen,
            //    FigureKind.WhiteRook, FigureKind.WhiteKing
            //};
            //_blacks = new[]
            //{
            //    FigureKind.BlackKnight, FigureKind.BlackBishop, FigureKind.BlackPawn, FigureKind.BlackQueen,
            //    FigureKind.BlackRook, FigureKind.BlackKing
            //};
            _overBoard = new bool[8, 2];
            _board = new FigureKind?[64];
            _map = ServiceLocator.Current.GetInstance<IFigureMap>();
            _moveHistory = ServiceLocator.Current.GetInstance<IMoveHistoryService>();
            _moveProvider = ServiceLocator.Current.GetInstance<IMoveProvider>();

            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    if (y == 1)
                    {
                        _board[x*8+y] = FigureKind.WhitePawn;
                    }
                    else if (y == 6)
                    {
                        _board[x * 8 + y] = FigureKind.BlackPawn;
                    }
                    else if (y == 0)
                    {
                        if (x == 0 || x == 7)
                            _board[x * 8 + y] = FigureKind.WhiteRook;
                        else if (x == 1 || x == 6)
                            _board[x * 8 + y] = FigureKind.WhiteKnight;
                        else if (x == 2 || x == 5)
                            _board[x * 8 + y] = FigureKind.WhiteBishop;
                        else if (x == 3)
                            _board[x * 8 + y] = FigureKind.WhiteQueen;
                        else
                            _board[x * 8 + y] = FigureKind.WhiteKing;
                    }
                    else if (y == 7)
                    {
                        if (x == 0 || x == 7)
                            _board[x * 8 + y] = FigureKind.BlackRook;
                        else if (x == 1 || x == 6)
                            _board[x * 8 + y] = FigureKind.BlackKnight;
                        else if (x == 2 || x == 5)
                            _board[x * 8 + y] = FigureKind.BlackBishop;
                        else if (x == 3)
                            _board[x * 8 + y] = FigureKind.BlackQueen;
                        else
                            _board[x * 8 + y] = FigureKind.BlackKing;
                    }
                }
            }
        }

        #region Implementation of IBoard

        public FigureKind? this[byte k]
        {
            get => _board[k];
            set
            {
                UpdateMap(value, k);
                _board[k] = value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateMap(FigureKind? value, byte coordinate)
        {
            if (_board[coordinate].HasValue)
            {
                _map.Remove(coordinate, _board[coordinate].Value);
            }

            if (value != null)
            {
                _map.Set(coordinate, value.Value);
            }
        }

        public FigureKind? this[Coordinate cell]
        {
            get => _board[cell.Key];
            set => this[cell.Key] = value;
        }

        public Coordinate WhiteKingPosition => _map.WhiteKingPosition;
        public Coordinate BlackKingPosition => _map.BlackKingPosition;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SmallWhiteCastle(bool isDo)
        {
            if (isDo)
            {
                this[56] = null;
                this[40] = FigureKind.WhiteRook;
            }
            else
            {
                this[40] = null;
                this[56] = FigureKind.WhiteRook;
            }

            _map.WhiteCastle(isDo);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SmallBlackCastle(bool isDo)
        {
            if (isDo)
            {
                this[63] = null;
                this[47] = FigureKind.BlackRook;
            }
            else
            {
                this[47] = null;
                this[63] = FigureKind.BlackRook;
            }

            _map.BlackCastle(isDo);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BigWhiteCastle(bool isDo)
        {
            if (isDo)
            {
                this[0] = null;
                this[24] = FigureKind.WhiteRook;
            }
            else
            {
                this[24] = null;
                this[0] = FigureKind.WhiteRook;
            }

            _map.WhiteCastle(isDo);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BigBlackCastle(bool isDo)
        {
            if (isDo)
            {
                this[7] = null;
                this[31] = FigureKind.BlackRook;
            }
            else
            {
                this[31] = null;
                this[7] = FigureKind.BlackRook;
            }

            _map.BlackCastle(isDo);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEmpty(int coordinate)
        {
            return _board[coordinate] == null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEmpty(List<int> coordinate)
        {
            for (var i = 0; i < coordinate.Count; i++)
            {
                if (_board[coordinate[i]] !=null) return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanDoSmallCastle(FigureKind figure)
        {
            return figure == FigureKind.WhiteKing
                ? _moveHistory.CanDoWhiteSmallCastle && _moveProvider.CanDoWhiteSmallCastle(this)
                : _moveHistory.CanDoBlackSmallCastle && _moveProvider.CanDoBlackSmallCastle(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanDoBigCastle(FigureKind figure)
        {
            return figure == FigureKind.WhiteKing
                ? _moveHistory.CanDoWhiteBigCastle && _moveProvider.CanDoWhiteBigCastle(this)
                : _moveHistory.CanDoBlackBigCastle && _moveProvider.CanDoBlackBigCastle(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanAttackOver(byte x, byte y, FigureKind figure)
        {
            if (_overBoard[x, y - 3])
            {
                return figure == FigureKind.WhitePawn
                    ? _board[x * 8 + y] == FigureKind.BlackPawn
                    : _board[x * 8 + y] == FigureKind.WhitePawn;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetOver(byte x, byte y, bool b)
        {
            _overBoard[x, y - 3] = b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsOnCell(byte cell, FigureKind figure)
        {
            return _board[cell] == figure;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetValue()
        {
            return _map.GetValue(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetValue(byte k)
        {
            return _board[k].HasValue ? _values[(int) _board[k].Value] : (byte) 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<Coordinate> GetWhiteCells()
        {
            return _map.GetWhiteCells();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<Coordinate> GetBlackCells()
        {
            return _map.GetBlackCells();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsOpposite(int cell, bool isWhite)
        {
            var figureKind = _board[cell];
            if (!figureKind.HasValue) return false;

            var figure = figureKind.Value;
            return isWhite ? figure.IsBlack() : figure.IsWhite();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<int> GetPositions(int figure)
        {
            return _map.GetPositions(figure);
        }

        #endregion

        #region Overrides of Object

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            for (int y = 7; y >= 0; y--)
            {
                for (int x = 0; x < 8; x++)
                {
                    string v = _board[x * 8 + y] == null ? "  " : _textValues[(int)_board[x * 8 + y].Value];
                    builder.Append($"[{v}]");
                }

                builder.AppendLine();
            }
            return builder.ToString();
        }

        #endregion
    }
}