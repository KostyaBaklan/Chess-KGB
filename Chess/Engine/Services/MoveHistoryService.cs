using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Enums;
using Engine.Models.Helpers;

namespace Engine.Services
{
    public class MoveHistoryService: IMoveHistoryService
    {
        private int _ply = -1;
        private readonly bool[] _whiteSmallCastleHistory;
        private readonly bool[] _whiteBigCastleHistory;
        private readonly bool[] _blackSmallCastleHistory;
        private readonly bool[] _blackBigCastleHistory;
        private readonly ArrayStack<IMove> _history;
        private readonly ArrayStack<ulong> _boardHistory;

        public MoveHistoryService()
        {
            var historyDepth = 256;
            _whiteSmallCastleHistory = new bool[historyDepth];
            _whiteBigCastleHistory = new bool[historyDepth];
            _blackSmallCastleHistory = new bool[historyDepth];
            _blackBigCastleHistory = new bool[historyDepth];
            _history = new ArrayStack<IMove>(historyDepth);
            _boardHistory = new ArrayStack<ulong>(historyDepth);
        }

        #region Implementation of IMoveHistoryService

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetPly()
        {
            return _ply;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IMove GetLastMove()
        {
            return _history.Peek();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(IMove move)
        {
            _history.Push(move);
            var ply = _ply;
            _ply++;
            if (_ply > 0)
            {
                if (_ply % 2 == 0)
                {
                    _blackSmallCastleHistory[_ply] = _blackSmallCastleHistory[ply];
                    _blackBigCastleHistory[_ply] = _blackBigCastleHistory[ply];

                    var figure = move.Piece;
                    if (figure == Piece.WhiteKing)
                    {
                        _whiteSmallCastleHistory[_ply] = false;
                        _whiteBigCastleHistory[_ply] = false;
                        return;
                    }

                    if (!_whiteSmallCastleHistory[ply])
                    {
                        _whiteSmallCastleHistory[_ply] = false;
                    }
                    else
                    {
                        _whiteSmallCastleHistory[_ply] = figure != Piece.WhiteRook || move.From != Squares.H1;
                    }
                    if (!_whiteBigCastleHistory[ply])
                    {
                        _whiteBigCastleHistory[_ply] = false;
                    }
                    else
                    {
                        _whiteBigCastleHistory[_ply] = figure != Piece.WhiteRook || move.From != Squares.A1;
                    }
                }
                else
                {
                    _whiteSmallCastleHistory[_ply] = _whiteSmallCastleHistory[ply];
                    _whiteBigCastleHistory[_ply] = _whiteBigCastleHistory[ply];

                    var figure = move.Piece;
                    if (figure == Piece.BlackKing)
                    {
                        _blackSmallCastleHistory[_ply] = false;
                        _blackBigCastleHistory[_ply] = false;
                        return;
                    }

                    if (!_blackSmallCastleHistory[ply])
                    {
                        _blackSmallCastleHistory[_ply] = false;
                    }
                    else
                    {
                        _blackSmallCastleHistory[_ply] = figure != Piece.BlackRook || move.From != Squares.H8;
                    }
                    if (!_blackBigCastleHistory[ply])
                    {
                        _blackBigCastleHistory[_ply] = false;
                    }
                    else
                    {
                        _blackBigCastleHistory[_ply] = figure != Piece.BlackRook || move.From != Squares.A8;
                    }
                }
            }
            else
            {
                _whiteSmallCastleHistory[_ply] = true;
                _whiteBigCastleHistory[_ply] = true;
                _blackSmallCastleHistory[_ply] = true;
                _blackBigCastleHistory[_ply] = true;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IMove Remove()
        {
            _ply--;
            return _history.Pop();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanDoBlackCastle()
        {
            return _ply < 0 || _blackSmallCastleHistory[_ply] || _blackBigCastleHistory[_ply];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanDoWhiteCastle()
        {
            return _ply < 0 || _whiteSmallCastleHistory[_ply] || _whiteBigCastleHistory[_ply];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanDoWhiteSmallCastle()
        {
            return _ply < 0 || _whiteSmallCastleHistory[_ply];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanDoWhiteBigCastle()
        {
            return _ply < 0 || _whiteBigCastleHistory[_ply];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanDoBlackSmallCastle()
        {
            return _ply < 0 || _blackSmallCastleHistory[_ply];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanDoBlackBigCastle()
        {
            return _ply < 0 || _blackBigCastleHistory[_ply];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsAdditionalDebutMove(IMove move)
        {
            if (_ply >= 17) return false;

            int x = 1;
            if (move.Piece.IsWhite())
            {
                x = 0;
            }

            for (int i = x; i < _ply; i += 2)
            {
                if (_history[i].Piece == move.Piece && _history[i].To == move.From)
                {
                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<IMove> GetHistory()
        {
            return _history.Items();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsThreefoldRepetition(ulong board)
        {
            if (_ply < 6) return false;

            int count = 1;
            for (var i = _boardHistory.Count - 3; i >= 4; i-=2)
            {
                if (_boardHistory[i] != board)
                {
                    continue;
                }

                count++;
                if (count >= 3)
                {
                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(ulong board)
        {
            _boardHistory.Push(board);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(ulong board)
        {
            _boardHistory.Pop();
        }

        #endregion

        #region Overrides of Object

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            for (var i = 0; i < _history.Count; i++)
            {
                builder.Append(_history[i]);
            }
            return builder.ToString();
        }

        #endregion
    }
}