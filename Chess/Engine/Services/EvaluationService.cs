using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Enums;
using Engine.Models.Helpers;

namespace Engine.Services
{
    public class EvaluationService: IEvaluationService
    {
        private bool _useCache;
        private int _nextDepth;

        private readonly int _penaltyValue;
        private readonly int _unitValue;
        private readonly int _pawnValue;
        private readonly int[] _values;
        private readonly int[][][] _staticValues;
        private Dictionary<ulong,short> _table;
        private DynamicCollection<ulong>[] _depthTable;
        private readonly IMoveHistoryService _moveHistory;

        #region Piece-Square Tables

        private static readonly int[] _blackPawnSquareTable = {
            0,  0,  0,  0,  0,  0,  0,  0,
            50, 50, 50, 50, 50, 50, 50, 50,
            10, 10, 20, 30, 30, 20, 10, 10,
             5,  5, 10, 25, 25, 10,  5,  5,
             0,  0,  0, 20, 20,  0,  0,  0,
             5, -5,-10,  0,  0,-10, -5,  5,
             5, 10, 10,-20,-20, 10, 10,  5,
             0,  0,  0,  0,  0,  0,  0,  0
        };

        private static readonly int[] _whitePawnSquareTable = {
            0,  0,  0,  0,  0,  0,  0,  0,
            5, 10, 10,-20,-20, 10, 10,  5,
            5, -5,-10,  0,  0,-10, -5,  5,
            0,  0,  0, 20, 20,  0,  0,  0,
            5,  5, 10, 25, 25, 10,  5,  5,
            10, 10, 20, 30, 30, 20, 10, 10,
            50, 50, 50, 50, 50, 50, 50, 50,
            0,  0,  0,  0,  0,  0,  0,  0
        };

        private static readonly int[] _blackKnightSquareTable = {
            -50,-40,-30,-30,-30,-30,-40,-50,
            -40,-20,  0,  0,  0,  0,-20,-40,
            -30,  0, 10, 15, 15, 10,  0,-30,
            -30,  5, 15, 20, 20, 15,  5,-30,
            -30,  0, 15, 20, 20, 15,  0,-30,
            -30,  5, 10, 15, 15, 10,  5,-30,
            -40,-20,  0,  5,  5,  0,-20,-40,
            -50,-40,-30,-30,-30,-30,-40,-50
        };

        private static readonly int[] _whiteKnightSquareTable = {
            -50,-40,-30,-30,-30,-30,-40,-50,
            -40,-20,  0,  5,  5,  0,-20,-40,
            -30,  5, 10, 15, 15, 10,  5,-30,
            -30,  0, 15, 20, 20, 15,  0,-30,
            -30,  5, 15, 20, 20, 15,  5,-30,
            -30,  0, 10, 15, 15, 10,  0,-30,
            -40,-20,  0,  0,  0,  0,-20,-40,
            -50,-40,-30,-30,-30,-30,-40,-50
        };

        private static readonly int[] _blackBishopSquareTable = {
            -20,-10,-10,-10,-10,-10,-10,-20,
            -10,  0,  0,  0,  0,  0,  0,-10,
            -10,  0,  5, 10, 10,  5,  0,-10,
            -10,  5,  5, 10, 10,  5,  5,-10,
            -10,  0, 10, 10, 10, 10,  0,-10,
            -10, 10, 10, 10, 10, 10, 10,-10,
            -10,  5,  0,  0,  0,  0,  5,-10,
            -20,-10,-10,-10,-10,-10,-10,-20
        };

        private static readonly int[] _whiteBishopSquareTable = {
            -20,-10,-10,-10,-10,-10,-10,-20,
            -10,  5,  0,  0,  0,  0,  5,-10,
            -10, 10, 10, 10, 10, 10, 10,-10,
            -10,  0, 10, 10, 10, 10,  0,-10,
            -10,  5,  5, 10, 10,  5,  5,-10,
            -10,  0,  5, 10, 10,  5,  0,-10,
            -10,  0,  0,  0,  0,  0,  0,-10,
            -20,-10,-10,-10,-10,-10,-10,-20
        };

        private static readonly int[] _blackRookSquareTable = {
            0,  0,  0,  0,  0,  0,  0,  0,
            5, 10, 10, 10, 10, 10, 10,  5,
           -5,  0,  0,  0,  0,  0,  0, -5,
           -5,  0,  0,  0,  0,  0,  0, -5,
           -5,  0,  0,  0,  0,  0,  0, -5,
           -5,  0,  0,  0,  0,  0,  0, -5,
           -5,  0,  0,  0,  0,  0,  0, -5,
            0,  0,  0 ,  5,  5,  0,  0,  0
        };

        private static readonly int[] _whiteRookSquareTable = {
            0,  0,  0,  5,  5,  0,  0,  0,
           -5,  0,  0,  0,  0,  0,  0, -5,
           -5,  0,  0,  0,  0,  0,  0, -5,
           -5,  0,  0,  0,  0,  0,  0, -5,
           -5,  0,  0,  0,  0,  0,  0, -5,
           -5,  0,  0,  0,  0,  0,  0, -5,
            5, 10, 10, 10, 10, 10, 10,  5,
            0,  0,  0,  0,  0,  0,  0,  0
        };

        private static readonly int[] _blackQueenSquareTable = {
            -20,-10,-10, -5, -5,-10,-10,-20,
            -10,  0,  0,  0,  0,  0,  0,-10,
            -10,  0,  5,  5,  5,  5,  0,-10,
             -5,  0,  5,  5,  5,  5,  0, -5,
              0,  0,  5,  5,  5,  5,  0, -5,
            -10,  5,  5,  5,  5,  5,  0,-10,
            -10,  0,  5,  0,  0,  0,  0,-10,
            -20,-10,-10, -5, -5,-10,-10,-20
        };

        private static readonly int[] _whiteQueenSquareTable = {
            -20,-10,-10, -5, -5,-10,-10,-20,
            -10,  0,  5,  0,  0,  0,  0,-10,
            -10,  5,  5,  5,  5,  5,  0,-10,
             0,  0,  5,  5,  5,  5,  0, -5,
            -5,  0,  5,  5,  5,  5,  0, -5,
            -10,  0,  5,  5,  5,  5,  0,-10,
            -10,  0,  0,  0,  0,  0,  0,-10,
            -20,-10,-10, -5, -5,-10,-10,-20
          };

        private static readonly int[] _blackKingMiddleGameSquareTable = {
            -30,-40,-40,-50,-50,-40,-40,-30,
            -30,-40,-40,-50,-50,-40,-40,-30,
            -30,-40,-40,-50,-50,-40,-40,-30,
            -30,-40,-40,-50,-50,-40,-40,-30,
            -20,-30,-30,-40,-40,-30,-30,-20,
            -10,-20,-20,-20,-20,-20,-20,-10,
             20, 20,  0,  0,  0,  0, 20, 20,
             20, 30, 10,  0,  0, 10, 30, 20
        };

        private static readonly int[] _whiteKingMiddleGameSquareTable = {
            20, 30, 10,  0,  0, 10, 30, 20,
            20, 20,  0,  0,  0,  0, 20, 20,
            -10,-20,-20,-20,-20,-20,-20,-10,
            -20,-30,-30,-40,-40,-30,-30,-20,
            -30,-40,-40,-50,-50,-40,-40,-30,
            -30,-40,-40,-50,-50,-40,-40,-30,
            -30,-40,-40,-50,-50,-40,-40,-30,
            -30,-40,-40,-50,-50,-40,-40,-30
        };

        private static readonly int[] _blackKingEndGameSquareTable = {
            -50,-40,-30,-20,-20,-30,-40,-50,
            -30,-20,-10,  0,  0,-10,-20,-30,
            -30,-10, 20, 30, 30, 20,-10,-30,
            -30,-10, 30, 40, 40, 30,-10,-30,
            -30,-10, 30, 40, 40, 30,-10,-30,
            -30,-10, 20, 30, 30, 20,-10,-30,
            -30,-30,  0,  0,  0,  0,-30,-30,
            -50,-30,-30,-30,-30,-30,-30,-50
        };

        private static readonly int[] _whiteKingEndGameSquareTable = {
            -50,-30,-30,-30,-30,-30,-30,-50,
            -30,-30,  0,  0,  0,  0,-30,-30,
            -30,-10, 20, 30, 30, 20,-10,-30,
            -30,-10, 30, 40, 40, 30,-10,-30,
            -30,-10, 30, 40, 40, 30,-10,-30,
            -30,-10, 20, 30, 30, 20,-10,-30,
            -30,-20,-10,  0,  0,-10,-20,-30,
            -50,-40,-30,-20,-20,-30,-40,-50
        };


        #endregion

        public EvaluationService(IMoveHistoryService moveHistory)
        {
            _moveHistory = moveHistory;
            _unitValue = 1;
            _penaltyValue = 5;
            _pawnValue = 25;
            _values = new int[12];
            _values[Piece.WhitePawn.AsByte()] = 200;
            _values[Piece.BlackPawn.AsByte()] = 200;
            _values[Piece.WhiteKnight.AsByte()] = 625;
            _values[Piece.BlackKnight.AsByte()] = 625;
            _values[Piece.WhiteBishop.AsByte()] = 625;
            _values[Piece.BlackBishop.AsByte()] = 625;
            _values[Piece.WhiteKing.AsByte()] = 6000;
            _values[Piece.BlackKing.AsByte()] = 6000;
            _values[Piece.WhiteRook.AsByte()] = 975;
            _values[Piece.BlackRook.AsByte()] = 975;
            _values[Piece.WhiteQueen.AsByte()] = 1925;
            _values[Piece.BlackQueen.AsByte()] = 1925;

            _staticValues = new int[12][][];
            for (var i = 0; i < _staticValues.Length; i++)
            {
                _staticValues[i] = new int[3][];
            }

            var factor = 2;
            for (int i = 0; i < 3; i++)
            {
                _staticValues[Piece.WhitePawn.AsByte()][i] = _whitePawnSquareTable.Factor(factor);
                _staticValues[Piece.BlackPawn.AsByte()][i] = _blackPawnSquareTable.Factor(factor);
                _staticValues[Piece.WhiteKnight.AsByte()][i] = _whiteKnightSquareTable.Factor(factor);
                _staticValues[Piece.BlackKnight.AsByte()][i] = _blackKnightSquareTable.Factor(factor);
                _staticValues[Piece.WhiteBishop.AsByte()][i] = _whiteBishopSquareTable.Factor(factor);
                _staticValues[Piece.BlackBishop.AsByte()][i] = _blackBishopSquareTable.Factor(factor);
                _staticValues[Piece.WhiteRook.AsByte()][i] = _whiteRookSquareTable.Factor(factor);
                _staticValues[Piece.BlackRook.AsByte()][i] = _blackRookSquareTable.Factor(factor);
                _staticValues[Piece.WhiteQueen.AsByte()][i] = _whiteQueenSquareTable.Factor(factor);
                _staticValues[Piece.BlackQueen.AsByte()][i] = _blackQueenSquareTable.Factor(factor);
                _staticValues[Piece.WhiteKing.AsByte()][i] = i == 2
                    ? _whiteKingEndGameSquareTable.Factor(factor)
                    : _whiteKingMiddleGameSquareTable.Factor(factor);
                _staticValues[Piece.BlackKing.AsByte()][i] = i == 2
                    ? _blackKingEndGameSquareTable.Factor(factor)
                    : _blackKingMiddleGameSquareTable.Factor(factor);
            }

            _table = new Dictionary<ulong, short>(20000213);
        }

        #region Implementation of ICacheService

        public int Size => _table.Count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _table.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetValue(int piece)
        {
            return _values[piece];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetValue(int piece, int square, Phase phase)
        {
            return _staticValues[piece][(int)phase][square];
        }

        public int GetFullValue(int piece, int square, Phase phase)
        {
            return _values[piece] + _staticValues[piece][(int)phase][square];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetPawnValue(int factor = 1)
        {
            return _pawnValue * factor;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetPenaltyValue(int factor = 1)
        {
            return _penaltyValue * factor;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetUnitValue(int factor = 1)
        {
            return _unitValue * factor;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetMateValue(bool isWhite)
        {
            return isWhite ? _values[Piece.WhiteKing.AsByte()] : -_values[Piece.BlackKing.AsByte()];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Evaluate(IPosition position)
        {
            short value;
            if (_useCache)
            {
                var key = position.GetKey();
                if (_table.TryGetValue(key, out value))
                {
                    return value;
                }

                if (_table.Count > 10000000)
                {
                    var nextDepth = _nextDepth % _depthTable.Length;
                    var dynamicCollection = _depthTable[nextDepth];
                    foreach (var k in dynamicCollection)
                    {
                        _table.Remove(k);
                    }
                    _depthTable[nextDepth].Clear();
                    _nextDepth++;
                }

                value = position.GetValue();
                _table.Add(key, value);
                var depth = _moveHistory.GetPly();
                _depthTable[depth % _depthTable.Length].Add(key);
            }
            else
            {
                value = position.GetValue();
            }

            return value;
        }

        public void Initialize(short level)
        {
            if (level > 6)
            {
                _useCache = true;
                _table = new Dictionary<ulong, short>(20000213);
                _depthTable = new DynamicCollection<ulong>[12];
                for (var i = 0; i < _depthTable.Length; i++)
                {
                    _depthTable[i] = new DynamicCollection<ulong>();
                }
            }
            else
            {
                _useCache = false;
                _table = new Dictionary<ulong, short>();
            }
        }

        #endregion
    }
}
