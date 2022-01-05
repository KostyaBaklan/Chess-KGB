using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Engine.Interfaces;
using Engine.Models.Enums;
using Engine.Models.Helpers;

namespace Engine.Services
{
    public class EvaluationService: IEvaluationService
    {
        private readonly int _penaltyValue;
        private readonly int _unitValue;
        private readonly int _pawnValue;
        private readonly int[] _values;
        private readonly int[][] _staticValues;
        private readonly Dictionary<ulong,int> _table;

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

        private static int[] _blackKingEndGameSquareTable = {
            -50,-40,-30,-20,-20,-30,-40,-50,
            -30,-20,-10,  0,  0,-10,-20,-30,
            -30,-10, 20, 30, 30, 20,-10,-30,
            -30,-10, 30, 40, 40, 30,-10,-30,
            -30,-10, 30, 40, 40, 30,-10,-30,
            -30,-10, 20, 30, 30, 20,-10,-30,
            -30,-30,  0,  0,  0,  0,-30,-30,
            -50,-30,-30,-30,-30,-30,-30,-50
        };

        private static int[] _whiteKingEndGameSquareTable = {
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

        public EvaluationService()
        {
            _unitValue = 5;
            _penaltyValue = 25;
            _pawnValue = 125;
            _values = new int[12];
            _values[Piece.WhitePawn.AsByte()] = 1000;
            _values[Piece.BlackPawn.AsByte()] = 1000;
            _values[Piece.WhiteKnight.AsByte()] = 3125;
            _values[Piece.BlackKnight.AsByte()] = 3125;
            _values[Piece.WhiteBishop.AsByte()] = 3125;
            _values[Piece.BlackBishop.AsByte()] = 3125;
            _values[Piece.WhiteKing.AsByte()] = 100000;
            _values[Piece.BlackKing.AsByte()] = 100000;
            _values[Piece.WhiteRook.AsByte()] = 4875;
            _values[Piece.BlackRook.AsByte()] = 4875;
            _values[Piece.WhiteQueen.AsByte()] = 9625;
            _values[Piece.BlackQueen.AsByte()] = 9625;

            _staticValues = new int[12][];
            _staticValues[Piece.WhitePawn.AsByte()] = _whitePawnSquareTable.Factor(10);
            _staticValues[Piece.BlackPawn.AsByte()] = _blackPawnSquareTable.Factor(10);
            _staticValues[Piece.WhiteKnight.AsByte()] = _whiteKnightSquareTable.Factor(10);
            _staticValues[Piece.BlackKnight.AsByte()] = _blackKnightSquareTable.Factor(10);
            _staticValues[Piece.WhiteBishop.AsByte()] = _whiteBishopSquareTable.Factor(10);
            _staticValues[Piece.BlackBishop.AsByte()] = _blackBishopSquareTable.Factor(10);
            _staticValues[Piece.WhiteKing.AsByte()] = _whiteKingMiddleGameSquareTable.Factor(10);
            _staticValues[Piece.BlackKing.AsByte()] = _blackKingMiddleGameSquareTable.Factor(10);
            _staticValues[Piece.WhiteRook.AsByte()] = _whiteRookSquareTable.Factor(10);
            _staticValues[Piece.BlackRook.AsByte()] = _blackRookSquareTable.Factor(10);
            _staticValues[Piece.WhiteQueen.AsByte()] = _whiteQueenSquareTable.Factor(10);
            _staticValues[Piece.BlackQueen.AsByte()] = _blackQueenSquareTable.Factor(10);

            _table = new Dictionary<ulong, int>(20000213);
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
        public int GetValue(int piece, int square)
        {
            return _staticValues[piece][square];
        }

        public int GetFullValue(int piece, int square)
        {
            return _values[piece] + _staticValues[piece][square];
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
        public int Evaluate(IPosition position)
        {
            if (_table.TryGetValue(position.GetKey(), out var value))
            {
                return value;
            }

            if (_table.Count > 20000000)
            {
                _table.Clear();
            }

            value = position.GetValue();
            _table.Add(position.GetKey(), value);
            return value;
        }

        #endregion
    }
}
