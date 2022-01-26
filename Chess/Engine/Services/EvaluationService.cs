using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CommonServiceLocator;
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

        private readonly int[] _notAbleCastleValue;
        private readonly int[] _earlyQueenValue;
        private readonly int[] _doubleBishopValue;
        private readonly int[] _minorDefendedByPawnValue;
        private readonly int[] _blockedPawnValue;
        private readonly int[] _passedPawnValue;
        private readonly int[] _doubledPawnValue;
        private readonly int[] _isolatedPawnValue;
        private readonly int[] _backwardPawnValue;
        private readonly int[] _rookOnOpenFileValue;

        private readonly int[][] _values;
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

            _notAbleCastleValue = new int[3];
            _earlyQueenValue = new int[3];
            _doubleBishopValue = new int[3];
            _minorDefendedByPawnValue = new int[3];
            _blockedPawnValue = new int[3];
            _passedPawnValue = new int[3];
            _doubledPawnValue = new int[3];
            _isolatedPawnValue = new int[3];
            _backwardPawnValue = new int[3];
            _rookOnOpenFileValue = new int[3];
            for (byte i = 0; i < 3; i++)
            {
                var evaluationStatic = configuration.Evaluation.Static.GetBoard(i);
                _notAbleCastleValue[i] = evaluationStatic.NotAbleCastleValue * _penaltyValue;
                _earlyQueenValue[i] = evaluationStatic.EarlyQueenValue * _penaltyValue;
                _doubleBishopValue[i] = evaluationStatic.DoubleBishopValue * _penaltyValue;
                _minorDefendedByPawnValue[i] = evaluationStatic.MinorDefendedByPawnValue * _unitValue;
                _blockedPawnValue[i] = evaluationStatic.BlockedPawnValue * _penaltyValue;
                _passedPawnValue[i] = evaluationStatic.PassedPawnValue * _penaltyValue;
                _doubledPawnValue[i] = evaluationStatic.DoubledPawnValue * _penaltyValue;
                _isolatedPawnValue[i] = evaluationStatic.IsolatedPawnValue * _penaltyValue;
                _backwardPawnValue[i] = evaluationStatic.BackwardPawnValue * _penaltyValue;
                _rookOnOpenFileValue[i] = evaluationStatic.RookOnOpenFileValue * _penaltyValue;
            }

            _values = new int[3][];
            for (byte i = 0; i < 3; i++)
            {
                _values[i] = new int[12];
                _values[i][Piece.WhitePawn.AsByte()] = configuration.Evaluation.GetPiece(i).Pawn;
                _values[i][Piece.BlackPawn.AsByte()] = configuration.Evaluation.GetPiece(i).Pawn;
                _values[i][Piece.WhiteKnight.AsByte()] = configuration.Evaluation.GetPiece(i).Knight;
                _values[i][Piece.BlackKnight.AsByte()] = configuration.Evaluation.GetPiece(i).Knight;
                _values[i][Piece.WhiteBishop.AsByte()] = configuration.Evaluation.GetPiece(i).Bishop;
                _values[i][Piece.BlackBishop.AsByte()] = configuration.Evaluation.GetPiece(i).Bishop;
                _values[i][Piece.WhiteKing.AsByte()] = configuration.Evaluation.GetPiece(i).King;
                _values[i][Piece.BlackKing.AsByte()] = configuration.Evaluation.GetPiece(i).King;
                _values[i][Piece.WhiteRook.AsByte()] = configuration.Evaluation.GetPiece(i).Rook;
                _values[i][Piece.BlackRook.AsByte()] = configuration.Evaluation.GetPiece(i).Rook;
                _values[i][Piece.WhiteQueen.AsByte()] = configuration.Evaluation.GetPiece(i).Queen;
                _values[i][Piece.BlackQueen.AsByte()] = configuration.Evaluation.GetPiece(i).Queen;
            }

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
        public int GetValue(int piece, Phase phase)
        {
            return _values[(byte)phase][piece];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetValue(int piece, int square, Phase phase)
        {
            return _staticValues[piece][(int)phase][square];
        }

        public int GetFullValue(int piece, int square, Phase phase)
        {
            return _values[(byte)phase][piece] + _staticValues[piece][(byte)phase][square];
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
            var useEvaluationCache = ServiceLocator.Current.GetInstance<IConfigurationProvider>()
                .GeneralConfiguration.UseEvaluationCache;
            if (useEvaluationCache && level > 6)
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
        public int GetMinorDefendedByPawnValue(Phase phase)
        {
            return _minorDefendedByPawnValue[(byte) phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetBlockedPawnValue(Phase phase)
        {
            return _blockedPawnValue[(byte)phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetPassedPawnValue(Phase phase)
        {
            return _passedPawnValue[(byte)phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetDoubledPawnValue(Phase phase)
        {
            return _doubledPawnValue[(byte)phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetIsolatedPawnValue(Phase phase)
        {
            return _isolatedPawnValue[(byte)phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetBackwardPawnValue(Phase phase)
        {
            return _backwardPawnValue[(byte)phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetNotAbleCastleValue(Phase phase)
        {
            return _notAbleCastleValue[(byte)phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetEarlyQueenValue(Phase phase)
        {
            return _earlyQueenValue[(byte)phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetDoubleBishopValue(Phase phase)
        {
            return _doubleBishopValue[(byte)phase];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetRookOnOpenFileValue(Phase phase)
        {
            return _rookOnOpenFileValue[(byte)phase];
        }

        #endregion

        #endregion
    }
}
