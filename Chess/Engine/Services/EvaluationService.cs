using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Enums;
using Engine.Models.Helpers;

namespace Engine.Services
{
    public class EvaluationService : IEvaluationService
    {
        private bool _useCache;
        private int _nextDepth;
        private int _threshold;

        private readonly int _penaltyValue;
        private readonly int _unitValue;
        private readonly int _mateValue;
        private readonly int _notAbleCastleValue;
        private readonly int _earlyQueenValue;
        private readonly int _doubleBishopValue;
        private readonly int _minorDefendedByPawnValue;
        private readonly int _blockedPawnValue;
        private readonly int _passedPawnValue;
        private readonly int _doubledPawnValue;
        private readonly int _isolatedPawnValue;
        private readonly int _backwardPawnValue;
        private readonly int _rookOnOpenFileValue;

        private readonly int[] _values;
        private readonly int[][][] _staticValues;
        private Dictionary<ulong, short> _table;
        private DynamicCollection<ulong>[] _depthTable;
        private readonly IMoveHistoryService _moveHistory;

        public EvaluationService(IMoveHistoryService moveHistory, IConfigurationProvider configuration, IStaticValueProvider staticValueProvider)
        {
            _moveHistory = moveHistory;

            _unitValue = configuration.Evaluation.Static.Unit;
            _mateValue = configuration.Evaluation.Static.Mate;
            _penaltyValue = configuration.Evaluation.Static.Penalty;

            _notAbleCastleValue = configuration.Evaluation.Static.NotAbleCastleValue*_penaltyValue;
            _earlyQueenValue = configuration.Evaluation.Static.EarlyQueenValue * _penaltyValue;
            _doubleBishopValue = configuration.Evaluation.Static.DoubleBishopValue * _penaltyValue;
            _minorDefendedByPawnValue = configuration.Evaluation.Static.MinorDefendedByPawnValue * _unitValue;
            _blockedPawnValue = configuration.Evaluation.Static.BlockedPawnValue * _penaltyValue;
            _passedPawnValue = configuration.Evaluation.Static.PassedPawnValue * _penaltyValue;
            _doubledPawnValue = configuration.Evaluation.Static.DoubledPawnValue * _penaltyValue;
            _isolatedPawnValue = configuration.Evaluation.Static.IsolatedPawnValue * _penaltyValue;
            _backwardPawnValue = configuration.Evaluation.Static.BackwardPawnValue * _penaltyValue;
            _rookOnOpenFileValue = configuration.Evaluation.Static.RookOnOpenFileValue * _penaltyValue;

            _values = new int[12];
            _values[Piece.WhitePawn.AsByte()] = configuration.Evaluation.Piece.Pawn;
            _values[Piece.BlackPawn.AsByte()] = configuration.Evaluation.Piece.Pawn;
            _values[Piece.WhiteKnight.AsByte()] = configuration.Evaluation.Piece.Knight;
            _values[Piece.BlackKnight.AsByte()] = configuration.Evaluation.Piece.Knight;
            _values[Piece.WhiteBishop.AsByte()] = configuration.Evaluation.Piece.Bishop;
            _values[Piece.BlackBishop.AsByte()] = configuration.Evaluation.Piece.Bishop;
            _values[Piece.WhiteKing.AsByte()] = configuration.Evaluation.Piece.King;
            _values[Piece.BlackKing.AsByte()] = configuration.Evaluation.Piece.King;
            _values[Piece.WhiteRook.AsByte()] = configuration.Evaluation.Piece.Rook;
            _values[Piece.BlackRook.AsByte()] = configuration.Evaluation.Piece.Rook;
            _values[Piece.WhiteQueen.AsByte()] = configuration.Evaluation.Piece.Queen;
            _values[Piece.BlackQueen.AsByte()] = configuration.Evaluation.Piece.Queen;

            _staticValues = new int[12][][];
            for (var i = 0; i < _staticValues.Length; i++)
            {
                _staticValues[i] = new int[3][];
            }

            short factor = configuration.Evaluation.Static.Factor;
            for (byte i = 0; i < 12; i++)
            {
                _staticValues[i] = new int[3][];
                for (byte j = 0; j < 3; j++)
                {
                    _staticValues[i][j] = new int[64];
                    for (byte k = 0; k < 64; k++)
                    {
                        _staticValues[i][j][k] = staticValueProvider.GetValue(i, j, k)* factor;
                    }
                }
            }
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

                if (_table.Count > _threshold)
                {
                    ClearOnThreshold();
                }

                value = position.GetValue();
                _table.Add(key, value);
                _depthTable[_moveHistory.GetPly()].Add(key);
            }
            else
            {
                value = position.GetValue();
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ClearOnThreshold()
        {
            var dynamicCollection = _depthTable[_nextDepth % _depthTable.Length];
            foreach (var k in dynamicCollection)
            {
                _table.Remove(k);
            }

            dynamicCollection.Clear();
            _nextDepth++;
        }

        public void Initialize(short level)
        {
            if (level > 6)
            {
                _useCache = true;
                _depthTable = new DynamicCollection<ulong>[256];
                for (var i = 0; i < _depthTable.Length; i++)
                {
                    _depthTable[i] = new DynamicCollection<ulong>();
                }

                int capacity;
                if (level == 7)
                {
                    capacity = 30000781;
                }
                else if (level == 8)
                {
                    capacity = 40000651;
                }
                else
                {
                    capacity = 49979687;
                }

                _threshold = 2 * capacity / 3;
                _table = new Dictionary<ulong, short>(capacity);
            }
            else
            {
                _useCache = false;
                _table = new Dictionary<ulong, short>(0);
            }
        }

        #region Evaluations

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetMateValue()
        {
            return _mateValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetUnitValue()
        {
            return _unitValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetPenaltyValue()
        {
            return _penaltyValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetMinorDefendedByPawnValue()
        {
            return _minorDefendedByPawnValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetBlockedPawnValue()
        {
            return _blockedPawnValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetPassedPawnValue()
        {
            return _passedPawnValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetDoubledPawnValue()
        {
            return _doubledPawnValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetIsolatedPawnValue()
        {
            return _isolatedPawnValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetBackwardPawnValue()
        {
            return _backwardPawnValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetNotAbleCastleValue()
        {
            return _notAbleCastleValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetEarlyQueenValue()
        {
            return _earlyQueenValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetDoubleBishopValue()
        {
            return _doubleBishopValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetRookOnOpenFileValue()
        {
            return _rookOnOpenFileValue;
        }

        #endregion

        #endregion
    }
}
