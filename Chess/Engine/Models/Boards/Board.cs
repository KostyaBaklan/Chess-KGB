using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using CommonServiceLocator;
using Engine.DataStructures;
using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;

namespace Engine.Models.Boards
{
    public class Board : IBoard
    {
        private Phase _phase = Phase.Opening;

        private BitBoard _empty;
        private BitBoard _whites;
        private BitBoard _blacks;

        private BitBoard _whiteSmallCastleCondition;
        private BitBoard _whiteSmallCastleKing;
        private BitBoard _whiteSmallCastleRook;

        private BitBoard _whiteBigCastleCondition;
        private BitBoard _whiteBigCastleKing;
        private BitBoard _whiteBigCastleRook;

        private BitBoard _blackSmallCastleCondition;
        private BitBoard _blackSmallCastleKing;
        private BitBoard _blackSmallCastleRook;

        private BitBoard _blackBigCastleCondition;
        private BitBoard _blackBigCastleKing;
        private BitBoard _blackBigCastleRook;

        private readonly ZobristHash _hash;
        private BitBoard[] _ranks;
        private BitBoard[] _files;
        private BitBoard[] _boards;
        private BitBoard[] _whiteKingShield;
        private BitBoard[] _blackKingShield;
        private BitBoard[] _whiteKingFaceShield;
        private BitBoard[] _blackKingFaceShield;
        private BitBoard[] _whiteKingFace;
        private BitBoard[] _blackKingFace;
        private BitBoard[][] _whiteKingOpenFile;
        private BitBoard[][] _blackKingOpenFile;
        private BitBoard[] _rookFiles;

        private BitBoard[] _whiteMinorDefense;
        private BitBoard[] _blackMinorDefense;
        private BitBoard[] _whiteFacing;
        private BitBoard[] _blackFacing;

        private BitBoard[] _whiteBlockedPawns;
        private BitBoard[] _whiteDoublePawns;
        private BitBoard[] _whitePassedPawns;
        private BitBoard[] _whiteIsolatedPawns;
        private List<KeyValuePair<BitBoard, BitBoard>>[] _whiteBackwardPawns;

        private BitBoard[] _blackBlockedPawns;
        private BitBoard[] _blackDoublePawns;
        private BitBoard[] _blackPassedPawns;
        private BitBoard[] _blackIsolatedPawns;
        private List<KeyValuePair<BitBoard, BitBoard>>[] _blackBackwardPawns;

        private readonly bool[] _overBoard;
        private readonly Piece[] _pieces;
        private readonly BitBoard _whiteQueenOpening;
        private readonly BitBoard _blackQueenOpening;
        private BitBoard _notFileA;
        private BitBoard _notFileH;

        private readonly PositionCollection[] _positionCollections;

        private readonly IMoveProvider _moveProvider;
        private readonly IMoveHistoryService _moveHistory;
        private readonly IEvaluationService _evaluationService;
        private readonly IAttackEvaluationService _attackEvaluationService;

        public Board()
        {
            _pieces = new Piece[64];
            _overBoard = new bool[64];

            _positionCollections = new PositionCollection[12];
            for (var i = 0; i < _positionCollections.Length; i++)
            {
                _positionCollections[i] = new PositionCollection();
            }

            SetBoards();

            SetFilesAndRanks();

            SetCastles();

            _moveProvider = ServiceLocator.Current.GetInstance<IMoveProvider>();
            _moveHistory = ServiceLocator.Current.GetInstance<IMoveHistoryService>();
            _evaluationService = ServiceLocator.Current.GetInstance<IEvaluationService>();
            _attackEvaluationService = ServiceLocator.Current.GetInstance<IAttackEvaluationService>();
            _attackEvaluationService.SetBoard(this);

            _hash = new ZobristHash();
            _hash.Initialize(_boards);

            _moveProvider.SetBoard(this);

            _whiteQueenOpening = Squares.D1.AsBitBoard() | Squares.E1.AsBitBoard() | Squares.C1.AsBitBoard() |
                                 Squares.D2.AsBitBoard() | Squares.E2.AsBitBoard() | Squares.C2.AsBitBoard();

            _blackQueenOpening = Squares.D8.AsBitBoard() | Squares.E8.AsBitBoard() | Squares.C8.AsBitBoard() |
                                 Squares.D7.AsBitBoard() | Squares.E7.AsBitBoard() | Squares.C7.AsBitBoard();

            SetKingSafety();

            SetPawnProperties();
        }

        private void SetPawnProperties()
        {
            _whiteBlockedPawns = new BitBoard[64];
            _whiteDoublePawns = new BitBoard[64];
            _whitePassedPawns = new BitBoard[64];
            _whiteIsolatedPawns = new BitBoard[64];
            _whiteBackwardPawns = new List<KeyValuePair<BitBoard, BitBoard>>[64];

            _blackBlockedPawns = new BitBoard[64];
            _blackDoublePawns = new BitBoard[64];
            _blackPassedPawns = new BitBoard[64];
            _blackIsolatedPawns = new BitBoard[64];
            _blackBackwardPawns = new List<KeyValuePair<BitBoard, BitBoard>>[64];

            _whiteMinorDefense = new BitBoard[64];
            _blackMinorDefense = new BitBoard[64];

            _whiteFacing = new BitBoard[64];
            _blackFacing = new BitBoard[64];

            for (byte i = 8; i < 48; i++)
            {
                BitBoard b = new BitBoard();
                for (int j = i+8; j < 56; j+=8)
                {
                    b |= j.AsBitBoard();
                }
                _whiteFacing[i] = b;
            }
            for (byte i = 16; i < 56; i++)
            {
                BitBoard b = new BitBoard();
                for (int j = i - 8; j >= 8; j -= 8)
                {
                    b |= j.AsBitBoard();
                }
                _blackFacing[i] = b;
            }

            for (byte i = 16; i < 64; i++)
            {
                _whiteMinorDefense[i] = _moveProvider.GetAttackPattern(Piece.BlackPawn.AsByte(), i);
            }
            for (byte i = 0; i < 48; i++)
            {
                _blackMinorDefense[i] = _moveProvider.GetAttackPattern(Piece.WhitePawn.AsByte(), i);
            }

            BitBoard ones = new BitBoard();
            ones = ~ones;

            for (int i = 8; i < 56; i++)
            {
                var f = i % 8;
                var r = i / 8;

                _whiteBlockedPawns[i] = (i + 8).AsBitBoard();
                _blackBlockedPawns[i] = (i - 8).AsBitBoard();

                _whiteDoublePawns[i] = _files[f] ^ i.AsBitBoard();
                _blackDoublePawns[i] = _files[f] ^ i.AsBitBoard();

                if (f == 0)
                {
                    _whitePassedPawns[i] = (ones << i) & (_files[0] | _files[1]) & ~_ranks[r];
                    _blackPassedPawns[i] = ~(ones << i) & (_files[0] | _files[1]) & ~_ranks[r];

                    _whiteIsolatedPawns[i] = _files[1];
                    _blackIsolatedPawns[i] = _files[1];
                }
                else if (f == 7)
                {
                    _whitePassedPawns[i] = (ones << i) & (_files[6] | _files[7]) & ~_ranks[r];
                    _blackPassedPawns[i] = ~(ones << i) & (_files[6] | _files[7]) & ~_ranks[r];

                    _whiteIsolatedPawns[i] = _files[7];
                    _blackIsolatedPawns[i] = _files[7];
                }
                else
                {
                    _whitePassedPawns[i] = (ones << i) & (_files[f - 1] | _files[f] | _files[f + 1]) & ~_ranks[r];
                    _blackPassedPawns[i] = ~(ones << i) & (_files[f - 1] | _files[f] | _files[f + 1]) & ~_ranks[r];

                    _whiteIsolatedPawns[i] = _files[f - 1] | _files[f + 1];
                    _blackIsolatedPawns[i] = _files[f - 1] | _files[f + 1];
                }
            }

            for (int i = 0; i < 64; i++)
            {
                _whiteBackwardPawns[i] = new List<KeyValuePair<BitBoard, BitBoard>>();
                _blackBackwardPawns[i] = new List<KeyValuePair<BitBoard, BitBoard>>();
            }

            for (byte i = 8; i < 16; i++)
            {
                byte w = (byte)(i + 8);
                var bb = _moveProvider.GetAttackPattern(Piece.WhitePawn.AsByte(), w);
                var wb = _blackPassedPawns[w];
                for (int j = i; j >= 0; j -= 8)
                {
                    wb ^= j.AsBitBoard();
                }

                _whiteBackwardPawns[i].Add(new KeyValuePair<BitBoard, BitBoard>(wb, bb));

                w = (byte)(i + 16);
                bb = _moveProvider.GetAttackPattern(Piece.WhitePawn.AsByte(), w);
                wb = _blackPassedPawns[w];
                for (int j = i; j >= 0; j -= 8)
                {
                    wb ^= j.AsBitBoard();
                }
                _whiteBackwardPawns[i].Add(new KeyValuePair<BitBoard, BitBoard>(wb, bb));
            }
            for (byte i = 16; i < 48; i++)
            {
                byte w = (byte)(i + 8);
                var bb = _moveProvider.GetAttackPattern(Piece.WhitePawn.AsByte(), w);
                var wb = _blackPassedPawns[w];
                for (int j = i; j >= 0; j -= 8)
                {
                    wb ^= j.AsBitBoard();
                }

                _whiteBackwardPawns[i].Add(new KeyValuePair<BitBoard, BitBoard>(wb, bb));
            }
            for (byte i = 48; i < 56; i++)
            {
                byte w = (byte)(i - 8);
                var bb = _moveProvider.GetAttackPattern(Piece.BlackPawn.AsByte(), w);
                var wb = _whitePassedPawns[w];
                for (int j = i; j < 56; j += 8)
                {
                    wb ^= j.AsBitBoard();
                }

                _blackBackwardPawns[i].Add(new KeyValuePair<BitBoard, BitBoard>(wb, bb));

                w = (byte)(i - 16);
                bb = _moveProvider.GetAttackPattern(Piece.BlackPawn.AsByte(), w);
                wb = _whitePassedPawns[w];
                for (int j = i; j < 56; j += 8)
                {
                    wb ^= j.AsBitBoard();
                }

                _blackBackwardPawns[i].Add(new KeyValuePair<BitBoard, BitBoard>(wb, bb));
            }
            for (byte i = 16; i < 48; i++)
            {
                byte w = (byte)(i - 8);
                var bb = _moveProvider.GetAttackPattern(Piece.BlackPawn.AsByte(), w);
                var wb = _whitePassedPawns[w];
                for (int j = i; j < 56; j += 8)
                {
                    wb ^= j.AsBitBoard();
                }

                _blackBackwardPawns[i].Add(new KeyValuePair<BitBoard, BitBoard>(wb, bb));
            }
        }

        private void SetKingSafety()
        {
            _whiteKingShield = new BitBoard[64];
            for (byte i = 0; i < 16; i++)
            {
                _whiteKingShield[i] = _moveProvider.GetAttackPattern(Piece.WhiteKing.AsByte(), i) |
                                      _moveProvider.GetAttackPattern(Piece.WhiteKing.AsByte(), (byte)(i + 8));
            }

            for (byte i = 16; i < 64; i++)
            {
                _whiteKingShield[i] = _moveProvider.GetAttackPattern(Piece.WhiteKing.AsByte(), i);
            }

            _blackKingShield = new BitBoard[64];

            for (byte i = (byte)(_blackKingShield.Length - 1); i >= 48; i--)
            {
                _blackKingShield[i] = _moveProvider.GetAttackPattern(Piece.BlackKing.AsByte(), i) |
                                      _moveProvider.GetAttackPattern(Piece.BlackKing.AsByte(), (byte)(i - 8));
            }

            for (byte i = 0; i < 48; i++)
            {
                _blackKingShield[i] = _moveProvider.GetAttackPattern(Piece.BlackKing.AsByte(), i);
            }

            _whiteKingFace = new BitBoard[64];
            for (byte i = 0; i < 32; i++)
            {
                _whiteKingFace[i] = _moveProvider.GetAttackPattern(Piece.WhiteKing.AsByte(), i) &
                                    _ranks[i / 8 + 1];
            }

            _blackKingFace = new BitBoard[64];
            for (byte i = 32; i < 64; i++)
            {
                _blackKingFace[i] = _moveProvider.GetAttackPattern(Piece.BlackKing.AsByte(), i) &
                                    _ranks[i / 8 - 1];
            }

            _whiteKingFaceShield = new BitBoard[64];
            for (byte i = 0; i < 32; i++)
            {
                _whiteKingFaceShield[i] = _moveProvider.GetAttackPattern(Piece.WhiteKing.AsByte(), (byte)(i + 8)) &
                                          _ranks[i / 8 + 2];
            }

            _blackKingFaceShield = new BitBoard[64];
            for (byte i = 32; i < 64; i++)
            {
                _blackKingFaceShield[i] = _moveProvider.GetAttackPattern(Piece.BlackKing.AsByte(), (byte)(i - 8)) &
                                          _ranks[i / 8 - 2];
            }

            _whiteKingOpenFile = new BitBoard[64][];
            for (var i = 0; i < _whiteKingOpenFile.Length; i++)
            {
                var rank = i % 8;
                if (rank == 0)
                {
                    _whiteKingOpenFile[i] = new[] { _files[0] ^ i.AsBitBoard(), _files[1] };
                }
                else if (rank == 7)
                {
                    _whiteKingOpenFile[i] = new[] { _files[7] ^ i.AsBitBoard(), _files[6] };
                }
                else
                {
                    _whiteKingOpenFile[i] = new[]
                        {_files[rank - 1], _files[rank] ^ i.AsBitBoard(), _files[rank + 1]};
                }
            }

            _blackKingOpenFile = new BitBoard[64][];
            for (var i = 0; i < _blackKingOpenFile.Length; i++)
            {
                var rank = i % 8;
                if (rank == 0)
                {
                    _blackKingOpenFile[i] = new[] { _files[0] ^ i.AsBitBoard(), _files[1] };
                }
                else if (rank == 7)
                {
                    _blackKingOpenFile[i] = new[] { _files[7] ^ i.AsBitBoard(), _files[6] };
                }
                else
                {
                    _blackKingOpenFile[i] = new[]
                        {_files[rank - 1], _files[rank] ^ i.AsBitBoard(), _files[rank + 1]};
                }
            }
        }

        #region Implementation of IBoard

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEmpty(BitBoard bitBoard)
        {
            return _empty.IsSet(bitBoard);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBlackPawn(BitBoard bitBoard)
        {
            return _boards[Piece.BlackPawn.AsByte()].IsSet(bitBoard);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsWhitePawn(BitBoard bitBoard)
        {
            return _boards[Piece.WhitePawn.AsByte()].IsSet(bitBoard);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsWhiteOpposite(Square square)
        {
            return _blacks.IsSet(square.AsBitBoard());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBlackOpposite(Square square)
        {
            return _whites.IsSet(square.AsBitBoard());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetStaticValue(byte piece)
        {
            int value = 0;
            for (var i = 0; i < _positionCollections[piece].Count; i++)
            {
                value += _evaluationService.GetFullValue(piece, _positionCollections[piece][i], _phase);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetStaticValue()
        {
            return GetWhiteStaticValue() - GetBlackStaticValue();
        }

        public int GetKingSafetyValue()
        {
            return WhiteKingSafety(_positionCollections[5][0]) - BlackKingSafety(_positionCollections[11][0]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetBlackStaticValue()
        {
            int value = 0;
            for (byte i = 6; i < 11; i++)
            {
                value += _evaluationService.GetValue(i, _phase) * _positionCollections[i].Count;
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetWhiteStaticValue()
        {
            int value = 0;
            for (byte i = 0; i < 5; i++)
            {
                value += _evaluationService.GetValue(i, _phase) * _positionCollections[i].Count;
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short GetValue()
        {
            return (short)(GetWhiteValue() - GetBlackValue());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetBlackValue()
        {
            return GetBlackPawnValue() + GetBlackKnightValue() + GetBlackBishopValue() +
                   GetBlackRookValue() + GetBlackQueenValue() + GetBlackKingValue();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetBlackKingValue()
        {
            var kingPosition = _positionCollections[Piece.BlackKing.AsByte()][0];
            var value = _evaluationService.GetValue(Piece.BlackKing.AsByte(), kingPosition, _phase);
            return _phase != Phase.End ? value + BlackKingSafety(kingPosition) : value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int BlackKingSafety(byte kingPosition)
        {
            return BlackKingShieldValue(kingPosition) - BlackKingAttackValue(kingPosition) - BlackKingOpenValue(kingPosition);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int BlackKingOpenValue(byte kingPosition)
        {
            int value = 0;
            var boards = _blackKingOpenFile[kingPosition];
            for (var i = 0; i < boards.Length; i++)
            {
                if ((boards[i] & _blacks).IsZero())
                {
                    value += _evaluationService.GetKingZoneOpenFileValue();
                }
            }
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int BlackKingAttackValue(byte kingPosition)
        {
            int attackingPiecesCount = 0;
            int valueOfAttacks = 0;
            var shield = _blackKingShield[kingPosition];

            int pieceAttacks = 0;
            var pawnAttacks = GetWhitePawnAttacks() & shield;
            if (pawnAttacks.Any())
            {
                var attacks = pawnAttacks.Count();
                pieceAttacks += attacks;
                valueOfAttacks += attacks * _evaluationService.GetPawnAttackValue();
            }

            if (pieceAttacks > 0)
            {
                attackingPiecesCount++;
            }

            var positions = _positionCollections[Piece.WhiteKnight.AsByte()];
            pieceAttacks = 0;
            for (var i = 0; i < positions.Count; i++)
            {
                var attackPattern = _moveProvider.GetAttackPattern(Piece.WhiteKnight.AsByte(), positions[i]) & shield;
                if (!attackPattern.Any()) continue;

                var attacks = attackPattern.Count();
                pieceAttacks += attacks;
                valueOfAttacks += attacks * _evaluationService.GetKnightAttackValue();
            }

            if (pieceAttacks > 0)
            {
                attackingPiecesCount++;
            }

            positions = _positionCollections[Piece.WhiteKing.AsByte()];
            pieceAttacks = 0;
            for (var i = 0; i < positions.Count; i++)
            {
                var attackPattern = _moveProvider.GetAttackPattern(Piece.WhiteKing.AsByte(), positions[i]) & shield;
                if (!attackPattern.Any()) continue;

                var attacks = attackPattern.Count();
                pieceAttacks += attacks;
                valueOfAttacks += attacks * _evaluationService.GetKingAttackValue();
            }

            if (pieceAttacks > 0)
            {
                attackingPiecesCount++;
            }

            positions = _positionCollections[Piece.WhiteBishop.AsByte()];
            pieceAttacks = 0;
            for (var i = 0; i < positions.Count; i++)
            {
                var bishopAttacks = positions[i].BishopAttacks(~_empty) & shield;
                if (bishopAttacks.Any())
                {
                    var attacks = bishopAttacks.Count();
                    pieceAttacks += attacks;
                    valueOfAttacks += attacks * _evaluationService.GetBishopAttackValue();
                }
                //else if (_boards[Piece.WhiteQueen.AsByte()].Any())
                //{
                //    bishopAttacks = positions[i].BishopAttacks(~_empty^ _boards[Piece.WhiteQueen.AsByte()]) & shield;
                //    if (bishopAttacks.Any())
                //    {
                //        var attacks = bishopAttacks.Count();
                //        pieceAttacks += attacks;
                //        valueOfAttacks += attacks * _evaluationService.GetBishopAttackValue();
                //    }
                //}
            }

            if (pieceAttacks > 0)
            {
                attackingPiecesCount++;
            }

            positions = _positionCollections[Piece.WhiteRook.AsByte()];
            pieceAttacks = 0;
            for (var i = 0; i < positions.Count; i++)
            {
                var rookAttacks = positions[i].RookAttacks(~_empty) & shield;
                if (rookAttacks.Any())
                {
                    var attacks = rookAttacks.Count();
                    pieceAttacks += attacks;
                    valueOfAttacks += attacks * _evaluationService.GetRookAttackValue();
                }
                //else
                //{
                //    var rooks = (_boards[Piece.WhiteRook.AsByte()] ^ positions[i].AsBitBoard()) |
                //                _boards[Piece.WhiteQueen.AsByte()];
                //    if (rooks.Any())
                //    {
                //        rookAttacks = positions[i].RookAttacks(~_empty ^ rooks) & shield;
                //        if (rookAttacks.Any())
                //        {
                //            var attacks = rookAttacks.Count();
                //            pieceAttacks += attacks;
                //            valueOfAttacks += attacks * _evaluationService.GetRookAttackValue();
                //        }
                //    }
                //}
            }

            if (pieceAttacks > 0)
            {
                attackingPiecesCount++;
            }

            positions = _positionCollections[Piece.WhiteQueen.AsByte()];
            pieceAttacks = 0;
            for (var i = 0; i < positions.Count; i++)
            {
                var queenAttacks = positions[i].QueenAttacks(~_empty) & shield;
                if (queenAttacks.Any())
                {
                    var attacks = queenAttacks.Count();
                    pieceAttacks += attacks;
                    valueOfAttacks += attacks * _evaluationService.GetQueenAttackValue();
                }
                //else if (_boards[Piece.WhiteBishop.AsByte()].Any())
                //{
                //    queenAttacks = positions[i].BishopAttacks(~_empty^ _boards[Piece.WhiteBishop.AsByte()]) & shield;
                //    if (queenAttacks.Any())
                //    {
                //        var attacks = queenAttacks.Count();
                //        pieceAttacks += attacks;
                //        valueOfAttacks += attacks * _evaluationService.GetQueenAttackValue();
                //    }
                //}
                //else if (_boards[Piece.WhiteRook.AsByte()].Any())
                //{
                //    queenAttacks = positions[i].RookAttacks(~_empty^ _boards[Piece.WhiteRook.AsByte()]) & shield;
                //    if (queenAttacks.Any())
                //    {
                //        var attacks = queenAttacks.Count();
                //        pieceAttacks += attacks;
                //        valueOfAttacks += attacks * _evaluationService.GetQueenAttackValue();
                //    }
                //}
            }

            if (pieceAttacks > 0)
            {
                attackingPiecesCount++;
            }

            double value = valueOfAttacks * _evaluationService.GetAttackWeight(attackingPiecesCount);

            return _evaluationService.GetUnitValue() * (int)Math.Ceiling(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int BlackKingShieldValue(byte kingPosition)
        {
            var face = _blackKingFace[kingPosition] & _blacks;
            var preFace = _blackKingFaceShield[kingPosition] & _blacks;
            return _evaluationService.GetKingShieldFaceValue() * face.Count() + _evaluationService.GetKingShieldPreFaceValue() * preFace.Count();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetBlackQueenValue()
        {
            var piece = Piece.BlackQueen.AsByte();
            var positions = _positionCollections[piece];
            if (positions.Count <= 0) return 0;

            var value = GetStaticValue(piece);

            for (var i = 0; i < positions.Count; i++)
            {
                var attackPattern = _moveProvider.GetAttackPattern(piece, positions[i]);
                if (attackPattern.IsSet(_boards[Piece.WhiteKing.AsByte()]))
                {
                    value += _evaluationService.GetRentgenValue(_phase);
                }

                if (_phase != Phase.Middle) continue;
                var countOfBlockingPawns = (_moveProvider.GetAttackPattern(Piece.BlackPawn.AsByte(), positions[i]) &
                                            _boards[Piece.BlackPawn.AsByte()]).Count();

                value -= countOfBlockingPawns * _evaluationService.GetBishopBlockedByPawnValue(_phase);
            }

            if (_phase != Phase.Opening) return value;

            if ((_blackQueenOpening & _boards[Piece.BlackQueen.AsByte()]).IsZero())
            {
                value -= _evaluationService.GetEarlyQueenValue(_phase);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetBlackRookValue()
        {
            var positions = _positionCollections[Piece.BlackRook.AsByte()];
            if (positions.Count == 0) return 0;
            var value = GetStaticValue(Piece.BlackRook.AsByte());

            if (_phase == Phase.Opening) return value;

            for (var i = 0; i < positions.Count; i++)
            {
                if ((_rookFiles[positions[i]] & (_boards[Piece.WhitePawn.AsByte()] | _boards[Piece.BlackPawn.AsByte()]))
                    .IsZero())
                {
                    value += _evaluationService.GetRookOnOpenFileValue(_phase);
                }
                else if ((_rookFiles[positions[i]] &  _boards[Piece.BlackPawn.AsByte()]).IsZero())
                {
                    value += _evaluationService.GetRookOnHalfOpenFileValue(_phase);
                }

                if (_boards[Piece.WhiteQueen.AsByte()].Any() && _rookFiles[positions[i]].IsSet(_boards[Piece.WhiteQueen.AsByte()]))
                {
                    value += _evaluationService.GetRentgenValue(_phase);
                }

                if (_rookFiles[positions[i]].IsSet(_boards[Piece.WhiteKing.AsByte()]))
                {
                    value += _evaluationService.GetRentgenValue(_phase);
                }

                if ((_rookFiles[positions[i]] & (_boards[Piece.BlackRook.AsByte()] | _boards[Piece.BlackQueen.AsByte()])).Any())
                {
                    value += _evaluationService.GetDoubleRookValue(_phase);
                }

                //if (_phase == Phase.End) continue;
                //if (rooks[i] == Squares.A8.AsByte())
                //{
                //    if ((_boards[Piece.BlackKing.AsByte()] &
                //         (Squares.B8.AsBitBoard() | Squares.C8.AsBitBoard() | Squares.D8.AsBitBoard())).Any())
                //    {
                //        value -= _evaluationService.GetRookBlockedByKingValue(_phase);
                //    }
                //}
                //else if (rooks[i] == Squares.B8.AsByte())
                //{
                //    if ((_boards[Piece.BlackKing.AsByte()] & (Squares.C8.AsBitBoard() | Squares.D8.AsBitBoard()))
                //        .Any())
                //    {
                //        value -= _evaluationService.GetRookBlockedByKingValue(_phase);
                //    }
                //}
                //else if (rooks[i] == Squares.C8.AsByte())
                //{
                //    if ((_boards[Piece.BlackKing.AsByte()] & Squares.D8.AsBitBoard()).Any())
                //    {
                //        value -= _evaluationService.GetRookBlockedByKingValue(_phase);
                //    }
                //}
                //else if (rooks[i] == Squares.H8.AsByte())
                //{
                //    if ((_boards[Piece.BlackKing.AsByte()] & (Squares.G8.AsBitBoard() | Squares.F8.AsBitBoard()))
                //        .Any())
                //    {
                //        value -= _evaluationService.GetRookBlockedByKingValue(_phase);
                //    }
                //}
                //else if (rooks[i] == Squares.G8.AsByte())
                //{
                //    if ((_boards[Piece.BlackKing.AsByte()] & Squares.F8.AsBitBoard()).Any())
                //    {
                //        value -= _evaluationService.GetRookBlockedByKingValue(_phase);
                //    }
                //}
            }

            //if (_pieceCount[Piece.BlackRook.AsByte()] <= 1) return value;

            //if ((rooks[0].RookAttacks(~_empty) & _boards[Piece.BlackRook.AsByte()]).Any())
            //{
            //    value += _evaluationService.GetRookConnectionValue(_phase);
            //}

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetBlackBishopValue()
        {
            var positions = _positionCollections[Piece.BlackBishop.AsByte()];
            if (positions.Count <= 0) return 0;

            var value = GetStaticValue(Piece.BlackBishop.AsByte());
            if (positions.Count == 2)
            {
                value += _evaluationService.GetDoubleBishopValue(_phase);
            }

            for (var i = 0; i < positions.Count; i++)
            {
                if ((_blackMinorDefense[positions[i]] & _boards[Piece.BlackPawn.AsByte()]).Any())
                {
                    value += _evaluationService.GetMinorDefendedByPawnValue(_phase);
                }
                var attackPattern = _moveProvider.GetAttackPattern(Piece.BlackBishop.AsByte(), positions[i]);
                if (_boards[Piece.WhiteQueen.AsByte()].Any() && attackPattern.IsSet(_boards[Piece.WhiteQueen.AsByte()]))
                {
                    value += _evaluationService.GetRentgenValue(_phase);
                }
                if (attackPattern.IsSet(_boards[Piece.WhiteKing.AsByte()]))
                {
                    value += _evaluationService.GetRentgenValue(_phase);
                }

                if (_phase != Phase.Middle) continue;
                var countOfBlockingPawns = (_moveProvider.GetAttackPattern(Piece.BlackPawn.AsByte(), positions[i]) &
                                            _boards[Piece.BlackPawn.AsByte()]).Count();

                value -= countOfBlockingPawns * _evaluationService.GetBishopBlockedByPawnValue(_phase);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetBlackKnightValue()
        {
            var positions = _positionCollections[Piece.BlackKnight.AsByte()];
            if (positions.Count <= 0) return 0;

            var value = GetStaticValue(Piece.BlackKnight.AsByte());

            for (var i = 0; i < positions.Count; i++)
            {
                if ((_blackMinorDefense[positions[i]] & _boards[Piece.BlackPawn.AsByte()]).Any())
                {
                    value += _evaluationService.GetMinorDefendedByPawnValue(_phase);
                }

                var emptyCells = _empty & _moveProvider.GetAttackPattern(Piece.BlackKnight.AsByte(), positions[i]);
                while (emptyCells.Any())
                {
                    byte position = emptyCells.BitScanForward();
                    if ((_moveProvider.GetAttackPattern(Piece.BlackPawn.AsByte(), position) &
                         _boards[Piece.WhitePawn.AsByte()]).Any())
                    {
                        value -= _evaluationService.GetKnightAttackedByPawnValue(_phase);
                    }
                    emptyCells = emptyCells.Remove(position);
                }
            }
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetBlackPawnValue()
        {
            int value = 0;
            var piece = Piece.BlackPawn.AsByte();
            var positions = _positionCollections[piece];
            for (var i = 0; i < positions.Count; i++)
            {
                byte coordinate = positions[i];
                value += _evaluationService.GetFullValue(piece, coordinate, _phase);
                if ((_blackBlockedPawns[coordinate] & _whites).Any())
                {
                    value -= _evaluationService.GetBlockedPawnValue(_phase);
                }

                if ((_blackIsolatedPawns[coordinate] & _boards[piece]).IsZero())
                {
                    value -= _evaluationService.GetIsolatedPawnValue(_phase);
                }

                if ((_blackDoublePawns[coordinate] & _boards[piece]).Any())
                {
                    value -= _evaluationService.GetDoubledPawnValue(_phase);
                }

                if (coordinate < 32 && (_blackFacing[coordinate] & _boards[Piece.WhitePawn.AsByte()]).IsZero())
                {
                    if ((_blackPassedPawns[coordinate] & _boards[Piece.WhitePawn.AsByte()]).IsZero())
                    {
                        value += _evaluationService.GetPassedPawnValue(_phase);
                    }
                    else
                    {
                        value += _evaluationService.GetOpenPawnValue(_phase);
                    }
                }

                for (var c = 0; c < _blackBackwardPawns[coordinate].Count; c++)
                {
                    if ((_blackBackwardPawns[coordinate][c].Key & _boards[Piece.BlackPawn.AsByte()]).IsZero() &&
                        (_blackBackwardPawns[coordinate][c].Value & _boards[Piece.WhitePawn.AsByte()]).Any())
                    {
                        value -= _evaluationService.GetBackwardPawnValue(_phase);
                        break;
                    }
                }
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetWhiteValue()
        {
            return GetWhitePawnValue() + GetWhiteKnightValue() + GetWhiteBishopValue() +
                   GetWhiteRookValue() + GetWhiteQueenValue() + GetWhiteKingValue();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetWhiteKingValue()
        {
            var kingPosition = _positionCollections[Piece.WhiteKing.AsByte()][0];
            var value = _evaluationService.GetValue(Piece.WhiteKing.AsByte(), kingPosition, _phase);
            return _phase != Phase.End ? value + WhiteKingSafety(kingPosition) : value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int WhiteKingSafety(byte kingPosition)
        {
            return WhiteKingShieldValue(kingPosition) - WhiteKingAttackValue(kingPosition) - WhiteKingOpenValue(kingPosition);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int WhiteKingOpenValue(byte kingPosition)
        {
            int value = 0;
            var boards = _whiteKingOpenFile[kingPosition];
            for (var i = 0; i < boards.Length; i++)
            {
                if ((boards[i] & _whites).IsZero())
                {
                    value += _evaluationService.GetKingZoneOpenFileValue();
                }
            }
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int WhiteKingAttackValue(byte kingPosition)
        {
            int attackingPiecesCount = 0;
            int valueOfAttacks = 0;
            var shield = _whiteKingShield[kingPosition];

            int pieceAttacks = 0;
            var pawnAttacks = GetBlackPawnAttacks() & shield;
            if (pawnAttacks.Any())
            {
                var attacks = pawnAttacks.Count();
                pieceAttacks += attacks;
                valueOfAttacks += attacks * _evaluationService.GetPawnAttackValue();
            }
            if (pieceAttacks > 0)
            {
                attackingPiecesCount++;
            }

            var positions = _positionCollections[Piece.BlackKnight.AsByte()];
            pieceAttacks = 0;
            for (var i = 0; i < positions.Count; i++)
            {
                var attackPattern = _moveProvider.GetAttackPattern(Piece.BlackKnight.AsByte(), positions[i]) & shield;
                if (!attackPattern.Any()) continue;

                var attacks = attackPattern.Count();
                pieceAttacks += attacks;
                valueOfAttacks += attacks * _evaluationService.GetKnightAttackValue();
            }

            if (pieceAttacks > 0)
            {
                attackingPiecesCount++;
            }

            positions = _positionCollections[Piece.BlackKing.AsByte()];
            pieceAttacks = 0;
            for (var i = 0; i < positions.Count; i++)
            {
                var attackPattern = _moveProvider.GetAttackPattern(Piece.BlackKing.AsByte(), positions[i]) & shield;
                if (!attackPattern.Any()) continue;

                var attacks = attackPattern.Count();
                pieceAttacks += attacks;
                valueOfAttacks += attacks * _evaluationService.GetKingAttackValue();
            }

            if (pieceAttacks > 0)
            {
                attackingPiecesCount++;
            }

            positions = _positionCollections[Piece.BlackBishop.AsByte()];
            pieceAttacks = 0;
            for (var i = 0; i < positions.Count; i++)
            {
                var bishopAttacks = positions[i].BishopAttacks(~_empty) & shield;
                if (bishopAttacks.Any())
                {
                    var attacks = bishopAttacks.Count();
                    pieceAttacks += attacks;
                    valueOfAttacks += attacks * _evaluationService.GetBishopAttackValue();
                }
                //else if(_boards[Piece.BlackQueen.AsByte()].Any())
                //{
                //    bishopAttacks = positions[i].BishopAttacks(~_empty^ _boards[Piece.BlackQueen.AsByte()]) & shield;
                //    if (bishopAttacks.Any())
                //    {
                //        var attacks = bishopAttacks.Count();
                //        pieceAttacks += attacks;
                //        valueOfAttacks += attacks * _evaluationService.GetBishopAttackValue();
                //    }
                //}
            }

            if (pieceAttacks > 0)
            {
                attackingPiecesCount++;
            }

            positions = _positionCollections[Piece.BlackRook.AsByte()];
            pieceAttacks = 0;
            for (var i = 0; i < positions.Count; i++)
            {
                var rookAttacks = positions[i].RookAttacks(~_empty) & shield;
                if (rookAttacks.Any())
                {
                    var attacks = rookAttacks.Count();
                    pieceAttacks += attacks;
                    valueOfAttacks += attacks * _evaluationService.GetRookAttackValue();
                }
                //else
                //{
                //    var rooks = (_boards[Piece.BlackRook.AsByte()] ^ positions[i].AsBitBoard()) |
                //                _boards[Piece.BlackQueen.AsByte()];
                //    if (rooks.Any())
                //    {
                //        rookAttacks = positions[i].RookAttacks(~_empty ^ rooks) & shield;
                //        if (rookAttacks.Any())
                //        {
                //            var attacks = rookAttacks.Count();
                //            pieceAttacks += attacks;
                //            valueOfAttacks += attacks * _evaluationService.GetRookAttackValue();
                //        }
                //    }
                //}
            }

            if (pieceAttacks > 0)
            {
                attackingPiecesCount++;
            }

            positions = _positionCollections[Piece.BlackQueen.AsByte()];
            pieceAttacks = 0;
            for (var i = 0; i < positions.Count; i++)
            {
                var queenAttacks = positions[i].QueenAttacks(~_empty) & shield;
                if (queenAttacks.Any())
                {
                    var attacks = queenAttacks.Count();
                    pieceAttacks += attacks;
                    valueOfAttacks += attacks * _evaluationService.GetQueenAttackValue();
                }
                //else if (_boards[Piece.BlackRook.AsByte()].Any())
                //{
                //    queenAttacks = positions[i].RookAttacks(~_empty^ _boards[Piece.BlackRook.AsByte()]) & shield;
                //    if (queenAttacks.Any())
                //    {
                //        var attacks = queenAttacks.Count();
                //        pieceAttacks += attacks;
                //        valueOfAttacks += attacks * _evaluationService.GetQueenAttackValue();
                //    }
                //}
                //else if (_boards[Piece.BlackBishop.AsByte()].Any())
                //{
                //    queenAttacks = positions[i].BishopAttacks(~_empty ^ _boards[Piece.BlackBishop.AsByte()]) & shield;
                //    if (queenAttacks.Any())
                //    {
                //        var attacks = queenAttacks.Count();
                //        pieceAttacks += attacks;
                //        valueOfAttacks += attacks * _evaluationService.GetQueenAttackValue();
                //    }
                //}
            }

            if (pieceAttacks > 0)
            {
                attackingPiecesCount++;
            }

            double value = valueOfAttacks * _evaluationService.GetAttackWeight(attackingPiecesCount);

            return _evaluationService.GetUnitValue() * (int)Math.Ceiling(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int WhiteKingShieldValue(byte kingPosition)
        {
            var face = _whiteKingFace[kingPosition] & _whites;
            var preFace = _whiteKingFaceShield[kingPosition] & _whites;
            return _evaluationService.GetKingShieldFaceValue() * face.Count() + _evaluationService.GetKingShieldPreFaceValue() * preFace.Count();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetWhiteQueenValue()
        {
            var piece = Piece.WhiteQueen.AsByte();
            var positions = _positionCollections[piece];
            if (positions.Count <= 0) return 0;

            var value = GetStaticValue(piece);

            for (var i = 0; i < positions.Count; i++)
            {
                var attackPattern = _moveProvider.GetAttackPattern(piece, positions[i]);
                if (attackPattern.IsSet(_boards[Piece.BlackKing.AsByte()]))
                {
                    value += _evaluationService.GetRentgenValue(_phase);
                }

                if (_phase != Phase.Middle) continue;
                var countOfBlockingPawns = (_moveProvider.GetAttackPattern(Piece.WhitePawn.AsByte(), positions[i]) &
                                            _boards[Piece.WhitePawn.AsByte()]).Count();

                value -= countOfBlockingPawns * _evaluationService.GetBishopBlockedByPawnValue(_phase);
            }

            if (_phase != Phase.Opening) return value;

            if ((_whiteQueenOpening & _boards[Piece.WhiteQueen.AsByte()]).IsZero())
            {
                value -= _evaluationService.GetEarlyQueenValue(_phase);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetWhiteRookValue()
        {
            var piece = Piece.WhiteRook.AsByte();
            var positions = _positionCollections[piece];
            if (positions.Count == 0) return 0;
            var value = GetStaticValue(piece);

            if (_phase == Phase.Opening) return value;

            for (var i = 0; i < positions.Count; i++)
            {
                if ((_rookFiles[positions[i]] & (_boards[Piece.WhitePawn.AsByte()] | _boards[Piece.BlackPawn.AsByte()]))
                    .IsZero())
                {
                    value += _evaluationService.GetRookOnOpenFileValue(_phase);
                }
                else if ((_rookFiles[positions[i]] & _boards[Piece.WhitePawn.AsByte()]).IsZero())
                {
                    value += _evaluationService.GetRookOnHalfOpenFileValue(_phase);
                }

                if (_boards[Piece.BlackQueen.AsByte()].Any() && _rookFiles[positions[i]].IsSet(_boards[Piece.BlackQueen.AsByte()]))
                {
                    value += _evaluationService.GetRentgenValue(_phase);
                }

                if (_rookFiles[positions[i]].IsSet(_boards[Piece.BlackKing.AsByte()]))
                {
                    value += _evaluationService.GetRentgenValue(_phase);
                }

                if ((_rookFiles[positions[i]]&(_boards[Piece.WhiteRook.AsByte()]| _boards[Piece.WhiteQueen.AsByte()])).Any())
                {
                    value += _evaluationService.GetDoubleRookValue(_phase);
                }

                //if (_phase == Phase.End) continue;
                //if (rooks[i] == Squares.A1.AsByte())
                //{
                //    if ((_boards[Piece.WhiteKing.AsByte()] &
                //         (Squares.B1.AsBitBoard() | Squares.C1.AsBitBoard() | Squares.D1.AsBitBoard())).Any())
                //    {
                //        value -= _evaluationService.GetRookBlockedByKingValue(_phase);
                //    }
                //}
                //else if (rooks[i] == Squares.B1.AsByte())
                //{
                //    if ((_boards[Piece.WhiteKing.AsByte()] & (Squares.C1.AsBitBoard() | Squares.D1.AsBitBoard()))
                //        .Any())
                //    {
                //        value -= _evaluationService.GetRookBlockedByKingValue(_phase);
                //    }
                //}
                //else if (rooks[i] == Squares.C1.AsByte())
                //{
                //    if ((_boards[Piece.WhiteKing.AsByte()] & Squares.D1.AsBitBoard()).Any())
                //    {
                //        value -= _evaluationService.GetRookBlockedByKingValue(_phase);
                //    }
                //}
                //else if (rooks[i] == Squares.H1.AsByte())
                //{
                //    if ((_boards[Piece.WhiteKing.AsByte()] & (Squares.G1.AsBitBoard() | Squares.F1.AsBitBoard()))
                //        .Any())
                //    {
                //        value -= _evaluationService.GetRookBlockedByKingValue(_phase);
                //    }
                //}
                //else if (rooks[i] == Squares.G1.AsByte())
                //{
                //    if ((_boards[Piece.WhiteKing.AsByte()] & Squares.F1.AsBitBoard()).Any())
                //    {
                //        value -= _evaluationService.GetRookBlockedByKingValue(_phase);
                //    }
                //}
            }

            //if (_pieceCount[Piece.WhiteRook.AsByte()] <= 1) return value;

            //if ((rooks[0].RookAttacks(~_empty) & _boards[Piece.WhiteRook.AsByte()]).Any())
            //{
            //    value += _evaluationService.GetRookConnectionValue(_phase);
            //}

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetWhiteBishopValue()
        {
            var piece = Piece.WhiteBishop.AsByte();
            var positions = _positionCollections[piece];
            if (positions.Count <= 0) return 0;

            var value = GetStaticValue(piece);
            if (positions.Count == 2)
            {
                value += _evaluationService.GetDoubleBishopValue(_phase);
            }

            for (var i = 0; i < positions.Count; i++)
            {
                if ((_whiteMinorDefense[positions[i]] & _boards[Piece.WhitePawn.AsByte()]).Any())
                {
                    value += _evaluationService.GetMinorDefendedByPawnValue(_phase);
                }
                var attackPattern = _moveProvider.GetAttackPattern(piece, positions[i]);
                if (_boards[Piece.BlackQueen.AsByte()].Any() && attackPattern.IsSet(_boards[Piece.BlackQueen.AsByte()]))
                {
                    value += _evaluationService.GetRentgenValue(_phase);
                }
                if (attackPattern.IsSet(_boards[Piece.BlackKing.AsByte()]))
                {
                    value += _evaluationService.GetRentgenValue(_phase);
                }

                if (_phase != Phase.Middle) continue;
                var countOfBlockingPawns = (_moveProvider.GetAttackPattern(Piece.WhitePawn.AsByte(), positions[i]) &
                                            _boards[Piece.WhitePawn.AsByte()]).Count();

                value -= countOfBlockingPawns * _evaluationService.GetBishopBlockedByPawnValue(_phase);
            }
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetWhiteKnightValue()
        {
            var positions = _positionCollections[Piece.WhiteKnight.AsByte()];
            if (positions.Count <= 0) return 0;

            var value = GetStaticValue(Piece.WhiteKnight.AsByte());

            for (var i = 0; i < positions.Count; i++)
            {
                if ((_whiteMinorDefense[positions[i]] & _boards[Piece.WhitePawn.AsByte()]).Any())
                {
                    value += _evaluationService.GetMinorDefendedByPawnValue(_phase);
                }

                var emptyCells = _empty & _moveProvider.GetAttackPattern(Piece.WhiteKnight.AsByte(), positions[i]);
                while (emptyCells.Any())
                {
                    byte position = emptyCells.BitScanForward();
                    if ((_moveProvider.GetAttackPattern(Piece.WhitePawn.AsByte(), position) &
                         _boards[Piece.BlackPawn.AsByte()]).Any())
                    {
                        value -= _evaluationService.GetKnightAttackedByPawnValue(_phase);
                    }
                    emptyCells = emptyCells.Remove(position);
                }
            }
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetWhitePawnValue()
        {
            int value = 0;
            var piece = Piece.WhitePawn.AsByte();
            var positions = _positionCollections[piece];
            for (var i = 0; i < positions.Count; i++)
            {
                byte coordinate = positions[i];
                value += _evaluationService.GetFullValue(piece, coordinate, _phase);

                if ((_whiteBlockedPawns[coordinate] & _blacks).Any())
                {
                    value -= _evaluationService.GetBlockedPawnValue(_phase);
                }

                if ((_whiteIsolatedPawns[coordinate] & _boards[piece]).IsZero())
                {
                    value -= _evaluationService.GetIsolatedPawnValue(_phase);
                }

                if ((_whiteDoublePawns[coordinate] & _boards[piece]).Any())
                {
                    value -= _evaluationService.GetDoubledPawnValue(_phase);
                }

                if (coordinate > 31 && (_whiteFacing[coordinate] & _boards[Piece.BlackPawn.AsByte()]).IsZero())
                {
                    if ((_whitePassedPawns[coordinate] & _boards[Piece.BlackPawn.AsByte()]).IsZero())
                    {
                        value += _evaluationService.GetPassedPawnValue(_phase);
                    }
                    else
                    {
                        value += _evaluationService.GetOpenPawnValue(_phase);
                    }
                }

                for (var c = 0; c < _whiteBackwardPawns[coordinate].Count; c++)
                {
                    if ((_whiteBackwardPawns[coordinate][c].Key & _boards[Piece.WhitePawn.AsByte()]).IsZero() &&
                        (_whiteBackwardPawns[coordinate][c].Value & _boards[Piece.BlackPawn.AsByte()]).Any())
                    {
                        value -= _evaluationService.GetBackwardPawnValue(_phase);
                        break;
                    }
                }
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int BlackBackwardPawn(BitBoard fBit, byte coordinate, int value, int file)
        {
            var b = fBit & _boards[Piece.BlackPawn.AsByte()];
            if (!b.Any()) return value;

            byte lsb = b.BitScanReverse();
            if (!lsb.IsLessRank(coordinate)) return value;

            byte weakPosition = (byte)(lsb / 8 * 8 + file);
            var attackPattern = _moveProvider.GetAttackPattern(Piece.BlackPawn.AsByte(), weakPosition);
            var w = attackPattern & _boards[Piece.WhitePawn.AsByte()];
            if (w.Any())
            {
                value -= _evaluationService.GetBackwardPawnValue(_phase);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int WhiteBackwardPawn(BitBoard fBit, int coordinate, int value, int file)
        {
            var w = fBit & _boards[Piece.WhitePawn.AsByte()];
            if (!w.Any()) return value;

            int lsb = w.BitScanForward();
            if (!lsb.IsGreaterRank(coordinate)) return value;

            byte weakPosition = (byte)(lsb / 8 * 8 + file);
            var attackPattern = _moveProvider.GetAttackPattern(Piece.WhitePawn.AsByte(), weakPosition);
            var b = attackPattern & _boards[Piece.BlackPawn.AsByte()];
            if (b.Any())
            {
                value -= _evaluationService.GetBackwardPawnValue(_phase);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetPawnValue(int value, int[] pawns)
        {
            int pawnPenalty = _evaluationService.GetDoubledPawnValue(_phase);
            int isolated = _evaluationService.GetIsolatedPawnValue(_phase);
            if (pawns[0] > 0)
            {
                if (pawns[0] > 1)
                {
                    value -= pawnPenalty;
                }

                if (pawns[1] == 0)
                {
                    value -= isolated;
                }
            }

            if (pawns[7] > 0)
            {
                if (pawns[7] > 1)
                {
                    value -= pawnPenalty;
                }

                if (pawns[6] == 0)
                {
                    value -= isolated;
                }
            }

            for (var i = 1; i < pawns.Length - 1; i++)
            {
                if (pawns[i] <= 0) continue;

                if (pawns[i] > 1)
                {
                    value -= pawnPenalty;
                }

                if (pawns[i - 1] == 0 && pawns[i + 1] == 0)
                {
                    value -= isolated;
                }
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private BitBoard GetFile(int position)
        {
            return _files[position % 8];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Piece GetPiece(Square cell)
        {
            return _pieces[cell.AsByte()];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetPiece(Square cell, out Piece? piece)
        {
            piece = null;

            var bit = cell.AsBitBoard();
            foreach (var p in Enum.GetValues(typeof(Piece)).OfType<Piece>())
            {
                if (!_boards[p.AsByte()].IsSet(bit)) continue;

                piece = p;
                break;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetOver(byte to, bool b)
        {
            _overBoard[to] = b;
        }

        public bool IsOver(byte square)
        {
            return _overBoard[square];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(Piece piece, Square square)
        {
            _hash.Update(square.AsByte(), piece.AsByte());
            _positionCollections[piece.AsByte()].Remove(square.AsByte());

            Remove(piece, square.AsBitBoard());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(Piece piece, Square square)
        {
            _hash.Update(square.AsByte(), piece.AsByte());
            _pieces[square.AsByte()] = piece;
            _positionCollections[piece.AsByte()].Add(square.AsByte());

            Add(piece, square.AsBitBoard());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Move(Piece piece, Square from, Square to)
        {
            _hash.Update(from.AsByte(), to.AsByte(), piece.AsByte());
            _pieces[to.AsByte()] = piece;
            _positionCollections[piece.AsByte()].Replace(from.AsByte(), to.AsByte());

            Move(piece, from.AsBitBoard() | to.AsBitBoard());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetWhiteKingPosition()
        {
            return _positionCollections[Piece.WhiteKing.AsByte()][0];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetBlackKingPosition()
        {
            return _positionCollections[Piece.BlackKing.AsByte()][0];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PositionCollection GetPiecePositions(byte index)
        {
            return _positionCollections[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong GetKey()
        {
            return _hash.Key;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard GetOccupied()
        {
            return ~_empty;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard GetPieceBits(Piece piece)
        {
            return _boards[piece.AsByte()];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard GetPerimeter()
        {
            return _ranks[0] | _ranks[7] | _files[0] | _files[7];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard GetWhitePawnAttacks()
        {
            return ((_boards[Piece.WhitePawn.AsByte()] & _notFileA) << 7) |
                   ((_boards[Piece.WhitePawn.AsByte()] & _notFileH) << 9);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard GetBlackPawnAttacks()
        {
            return ((_boards[Piece.BlackPawn.AsByte()] & _notFileA) >> 9) |
                   ((_boards[Piece.BlackPawn.AsByte()] & _notFileH) >> 7);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Phase UpdatePhase()
        {
            var ply = _moveHistory.GetPly();
            _phase = ply < 16 ? Phase.Opening : ply > 25 && IsEndGame() ? Phase.End : Phase.Middle;
            return _phase;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsEndGame()
        {
            return IsEndGameForWhite() || IsEndGameForBlack();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsEndGameForBlack()
        {
            int count = _positionCollections[Piece.BlackRook.AsByte()].Count + _positionCollections[Piece.BlackQueen.AsByte()].Count;
            if (count > 2)
            {
                return false;
            }

            count += _positionCollections[Piece.BlackBishop.AsByte()].Count;
            if (count > 2)
            {
                return false;
            }

            count += _positionCollections[Piece.BlackKnight.AsByte()].Count;
            return count <= 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsEndGameForWhite()
        {
            int count = _positionCollections[Piece.WhiteRook.AsByte()].Count + _positionCollections[Piece.WhiteQueen.AsByte()].Count;
            if (count > 2)
            {
                return false;
            }

            count += _positionCollections[Piece.WhiteBishop.AsByte()].Count;
            if (count > 2)
            {
                return false;
            }

            count += _positionCollections[Piece.WhiteKnight.AsByte()].Count;
            return count <= 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetBlackMaxValue()
        {
            int value = 0;
            for (byte i = 10; i > 5; i--)
            {
                if (_positionCollections[i].Count > 0)
                {
                    return _evaluationService.GetValue(i, _phase);
                }
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetWhiteMaxValue()
        {
            int value = 0;
            for (byte i = 4; i > 0; i--)
            {
                if (_positionCollections[i].Count > 0)
                {
                    return _evaluationService.GetValue(i, _phase);
                }
            }
            if (_positionCollections[0].Count > 0)
            {
                return _evaluationService.GetValue(0, _phase);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanWhitePromote()
        {
            return (_ranks[6] & _boards[Piece.WhitePawn.AsByte()]).Any();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanBlackPromote()
        {
            return (_ranks[1] & _boards[Piece.BlackPawn.AsByte()]).Any();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard GetRank(int rank)
        {
            return _ranks[rank];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard GetWhitePieceBits()
        {
            return _whites ^ _boards[Piece.WhitePawn.AsByte()] ^ _boards[Piece.WhiteKing.AsByte()];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard GetBlackPieceBits()
        {
            return _blacks ^ _boards[Piece.BlackPawn.AsByte()] ^ _boards[Piece.BlackKing.AsByte()];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard GetWhitePieceForKnightBits()
        {
            return _boards[Piece.WhiteBishop.AsByte()] | _boards[Piece.WhiteRook.AsByte()] | _boards[Piece.WhiteQueen.AsByte()];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard GetWhitePieceForBishopBits()
        {
            return _boards[Piece.WhiteKnight.AsByte()] | _boards[Piece.WhiteRook.AsByte()];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard GetBlackPieceForKnightBits()
        {
            return _boards[Piece.BlackBishop.AsByte()] | _boards[Piece.BlackRook.AsByte()] | _boards[Piece.BlackQueen.AsByte()];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard GetBlackPieceForBishopBits()
        {
            return _boards[Piece.BlackKnight.AsByte()] | _boards[Piece.BlackRook.AsByte()];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard GetBlackBits()
        {
            return _blacks;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard GetWhiteBits()
        {
            return _whites;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Phase GetPhase()
        {
            return _phase;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBlackPass(byte position)
        {
            return _phase != Phase.Opening && position<32&&
                   (_blackPassedPawns[position] & _boards[Piece.WhitePawn.AsByte()]).IsZero();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsWhitePass(byte position)
        {
            return _phase != Phase.Opening &&position > 31&&
                   (_whitePassedPawns[position] & _boards[Piece.BlackPawn.AsByte()]).IsZero();
        }

        #endregion

        #region SEE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int StaticExchange(AttackBase attack)
        {
            _attackEvaluationService.Initialize(_boards);
            return _attackEvaluationService.StaticExchange(attack);
        }

        #endregion

        #region Castle

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DoWhiteSmallCastle()
        {
            _pieces[Squares.G1.AsByte()] = Piece.WhiteKing;
            _pieces[Squares.F1.AsByte()] = Piece.WhiteRook;

            _hash.Update(Squares.H1.AsByte(), Squares.F1.AsByte(), Piece.WhiteRook.AsByte());
            _hash.Update(Squares.E1.AsByte(), Squares.G1.AsByte(), Piece.WhiteKing.AsByte());

            WhiteSmallCastle();

            _positionCollections[Piece.WhiteRook.AsByte()].Replace(Squares.H1.AsByte(), Squares.F1.AsByte());
            _positionCollections[Piece.WhiteKing.AsByte()].Replace(Squares.E1.AsByte(), Squares.G1.AsByte());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DoBlackSmallCastle()
        {
            _pieces[Squares.G8.AsByte()] = Piece.BlackKing;
            _pieces[Squares.F8.AsByte()] = Piece.BlackRook;

            _hash.Update(Squares.H8.AsByte(), Squares.F8.AsByte(), Piece.BlackRook.AsByte());
            _hash.Update(Squares.E8.AsByte(), Squares.G8.AsByte(), Piece.BlackKing.AsByte());

            BlackSmallCastle();

            _positionCollections[Piece.BlackRook.AsByte()].Replace(Squares.H8.AsByte(), Squares.F8.AsByte());
            _positionCollections[Piece.BlackKing.AsByte()].Replace(Squares.E8.AsByte(), Squares.G8.AsByte());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DoBlackBigCastle()
        {
            _pieces[Squares.C8.AsByte()] = Piece.BlackKing;
            _pieces[Squares.D8.AsByte()] = Piece.BlackRook;

            _hash.Update(Squares.A8.AsByte(), Squares.D8.AsByte(), Piece.BlackRook.AsByte());
            _hash.Update(Squares.E8.AsByte(), Squares.C8.AsByte(), Piece.BlackKing.AsByte());

            BlackBigCastle();

            _positionCollections[Piece.BlackRook.AsByte()].Replace(Squares.A8.AsByte(), Squares.D8.AsByte());
            _positionCollections[Piece.BlackKing.AsByte()].Replace(Squares.E8.AsByte(), Squares.C8.AsByte());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DoWhiteBigCastle()
        {
            _pieces[Squares.C1.AsByte()] = Piece.WhiteKing;
            _pieces[Squares.D1.AsByte()] = Piece.WhiteRook;

            _hash.Update(Squares.A1.AsByte(), Squares.D1.AsByte(), Piece.WhiteRook.AsByte());
            _hash.Update(Squares.E1.AsByte(), Squares.C1.AsByte(), Piece.WhiteKing.AsByte());

            WhiteBigCastle();

            _positionCollections[Piece.WhiteRook.AsByte()].Replace(Squares.A1.AsByte(), Squares.D1.AsByte());
            _positionCollections[Piece.WhiteKing.AsByte()].Replace(Squares.E1.AsByte(), Squares.C1.AsByte());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UndoWhiteSmallCastle()
        {
            _pieces[Squares.E1.AsByte()] = Piece.WhiteKing;
            _pieces[Squares.H1.AsByte()] = Piece.WhiteRook;

            _hash.Update(Squares.F1.AsByte(), Squares.H1.AsByte(), Piece.WhiteRook.AsByte());
            _hash.Update(Squares.G1.AsByte(), Squares.E1.AsByte(), Piece.WhiteKing.AsByte());

            WhiteSmallCastle();

            _positionCollections[Piece.WhiteRook.AsByte()].Replace(Squares.F1.AsByte(), Squares.H1.AsByte());
            _positionCollections[Piece.WhiteKing.AsByte()].Replace(Squares.G1.AsByte(), Squares.E1.AsByte());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UndoBlackSmallCastle()
        {
            _pieces[Squares.E8.AsByte()] = Piece.BlackKing;
            _pieces[Squares.H8.AsByte()] = Piece.BlackRook;

            _hash.Update(Squares.F8.AsByte(), Squares.H8.AsByte(), Piece.BlackRook.AsByte());
            _hash.Update(Squares.G8.AsByte(), Squares.E8.AsByte(), Piece.BlackKing.AsByte());

            BlackSmallCastle();

            _positionCollections[Piece.BlackRook.AsByte()].Replace(Squares.F8.AsByte(), Squares.H8.AsByte());
            _positionCollections[Piece.BlackKing.AsByte()].Replace(Squares.G8.AsByte(), Squares.E8.AsByte());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UndoWhiteBigCastle()
        {
            _pieces[Squares.E1.AsByte()] = Piece.WhiteKing;
            _pieces[Squares.A1.AsByte()] = Piece.WhiteRook;

            _hash.Update(Squares.D1.AsByte(), Squares.A1.AsByte(), Piece.WhiteRook.AsByte());
            _hash.Update(Squares.C1.AsByte(), Squares.E1.AsByte(), Piece.WhiteKing.AsByte());

            WhiteBigCastle();

            _positionCollections[Piece.WhiteRook.AsByte()].Replace(Squares.D1.AsByte(), Squares.A1.AsByte());
            _positionCollections[Piece.WhiteKing.AsByte()].Replace(Squares.C1.AsByte(), Squares.E1.AsByte());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UndoBlackBigCastle()
        {
            _pieces[Squares.E8.AsByte()] = Piece.BlackKing;
            _pieces[Squares.A8.AsByte()] = Piece.BlackRook;

            _hash.Update(Squares.D8.AsByte(), Squares.A8.AsByte(), Piece.BlackRook.AsByte());
            _hash.Update(Squares.C8.AsByte(), Squares.E8.AsByte(), Piece.BlackKing.AsByte());

            BlackBigCastle();

            _positionCollections[Piece.BlackRook.AsByte()].Replace(Squares.D8.AsByte(), Squares.A8.AsByte());
            _positionCollections[Piece.BlackKing.AsByte()].Replace(Squares.C8.AsByte(), Squares.E8.AsByte());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void BlackSmallCastle()
        {
            _boards[Piece.BlackKing.AsByte()] ^= _blackSmallCastleKing;
            _boards[Piece.BlackRook.AsByte()] ^= _blackSmallCastleRook;

            _blacks ^= _blackSmallCastleKing;
            _blacks ^= _blackSmallCastleRook;

            _empty = ~(_whites | _blacks);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void BlackBigCastle()
        {
            _boards[Piece.BlackKing.AsByte()] ^= _blackBigCastleKing;
            _boards[Piece.BlackRook.AsByte()] ^= _blackBigCastleRook;

            _blacks ^= _blackBigCastleKing;
            _blacks ^= _blackBigCastleRook;

            _empty = ~(_whites | _blacks);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WhiteBigCastle()
        {
            _boards[Piece.WhiteKing.AsByte()] ^= _whiteBigCastleKing;
            _boards[Piece.WhiteRook.AsByte()] ^= _whiteBigCastleRook;

            _whites ^= _whiteBigCastleKing;
            _whites ^= _whiteBigCastleRook;

            _empty = ~(_whites | _blacks);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WhiteSmallCastle()
        {
            _boards[Piece.WhiteKing.AsByte()] ^= _whiteSmallCastleKing;
            _boards[Piece.WhiteRook.AsByte()] ^= _whiteSmallCastleRook;

            _whites ^= _whiteSmallCastleKing;
            _whites ^= _whiteSmallCastleRook;

            _empty = ~(_whites | _blacks);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanDoBlackSmallCastle()
        {
            if (!_moveHistory.CanDoBlackSmallCastle() || !_boards[Piece.BlackRook.AsByte()].IsSet(BitBoards.H8) ||
                !_empty.IsSet(_blackSmallCastleCondition)) return false;

            return CanDoBlackCastle(Squares.E8.AsByte());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanDoWhiteSmallCastle()
        {
            if (!_moveHistory.CanDoWhiteSmallCastle() || !_boards[Piece.WhiteRook.AsByte()].IsSet(BitBoards.H1) ||
                !_empty.IsSet(_whiteSmallCastleCondition)) return false;

            return CanDoWhiteCastle(Squares.E1.AsByte());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanDoBlackBigCastle()
        {
            if (!_moveHistory.CanDoBlackBigCastle() || !_boards[Piece.BlackRook.AsByte()].IsSet(BitBoards.A8) ||
                !_empty.IsSet(_blackBigCastleCondition)) return false;

            return CanDoBlackCastle(Squares.E8.AsByte());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanDoWhiteBigCastle()
        {
            if (!_moveHistory.CanDoWhiteBigCastle() || !_boards[Piece.WhiteRook.AsByte()].IsSet(BitBoards.A1) ||
                !_empty.IsSet(_whiteBigCastleCondition)) return false;

            return CanDoWhiteCastle(Squares.E1.AsByte());

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanDoWhiteCastle(byte to)
        {
            return !(_moveProvider.IsUnderAttack(Piece.BlackBishop.AsByte(), to) ||
                     _moveProvider.IsUnderAttack(Piece.BlackKnight.AsByte(), to) ||
                     _moveProvider.IsUnderAttack(Piece.BlackQueen.AsByte(), to) ||
                     _moveProvider.IsUnderAttack(Piece.BlackRook.AsByte(), to) ||
                     (GetBlackPawnAttacks() & to.AsBitBoard()).Any());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanDoBlackCastle(byte to)
        {
            return !(_moveProvider.IsUnderAttack(Piece.WhiteBishop.AsByte(), to) ||
                     _moveProvider.IsUnderAttack(Piece.WhiteKnight.AsByte(), to) ||
                     _moveProvider.IsUnderAttack(Piece.WhiteQueen.AsByte(), to) ||
                     _moveProvider.IsUnderAttack(Piece.WhiteRook.AsByte(), to) ||
                     (GetWhitePawnAttacks() & to.AsBitBoard()).Any());
        }

        #endregion

        #region Private

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Remove(Piece piece, BitBoard bitBoard)
        {
            var bit = ~bitBoard;
            _boards[piece.AsByte()] &= bit;
            if (piece.IsWhite())
            {
                _whites &= bit;
            }
            else
            {
                _blacks &= bit;
            }

            _empty = ~(_whites | _blacks);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Add(Piece piece, BitBoard bitBoard)
        {
            _boards[piece.AsByte()] |= bitBoard;
            if (piece.IsWhite())
            {
                _whites |= bitBoard;
            }
            else
            {
                _blacks |= bitBoard;
            }

            _empty = ~(_whites | _blacks);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Move(Piece piece, BitBoard bitBoard)
        {
            _boards[piece.AsByte()] ^= bitBoard;
            if (piece.IsWhite())
            {
                _whites ^= bitBoard;
            }
            else
            {
                _blacks ^= bitBoard;
            }

            _empty = ~(_whites | _blacks);
        }

        private void SetBoards()
        {
            _boards = new BitBoard[12];
            _boards[Piece.WhitePawn.AsByte()] = _boards[Piece.WhitePawn.AsByte()].Set(Enumerable.Range(8, 8).ToArray());
            _boards[Piece.WhiteKnight.AsByte()] = _boards[Piece.WhiteKnight.AsByte()].Set(1, 6);
            _boards[Piece.WhiteBishop.AsByte()] = _boards[Piece.WhiteBishop.AsByte()].Set(2, 5);
            _boards[Piece.WhiteRook.AsByte()] = _boards[Piece.WhiteRook.AsByte()].Set(0, 7);
            _boards[Piece.WhiteQueen.AsByte()] = _boards[Piece.WhiteQueen.AsByte()].Set(3);
            _boards[Piece.WhiteKing.AsByte()] = _boards[Piece.WhiteKing.AsByte()].Set(4);

            _whites = _boards[Piece.WhitePawn.AsByte()] |
                      _boards[Piece.WhiteKnight.AsByte()] |
                      _boards[Piece.WhiteBishop.AsByte()] |
                      _boards[Piece.WhiteRook.AsByte()] |
                      _boards[Piece.WhiteQueen.AsByte()] |
                      _boards[Piece.WhiteKing.AsByte()];

            _boards[Piece.BlackPawn.AsByte()] =
                _boards[Piece.BlackPawn.AsByte()].Set(Enumerable.Range(48, 8).ToArray());
            _boards[Piece.BlackRook.AsByte()] = _boards[Piece.BlackRook.AsByte()].Set(56, 63);
            _boards[Piece.BlackKnight.AsByte()] = _boards[Piece.BlackKnight.AsByte()].Set(57, 62);
            _boards[Piece.BlackBishop.AsByte()] = _boards[Piece.BlackBishop.AsByte()].Set(58, 61);
            _boards[Piece.BlackQueen.AsByte()] = _boards[Piece.BlackQueen.AsByte()].Set(59);
            _boards[Piece.BlackKing.AsByte()] = _boards[Piece.BlackKing.AsByte()].Set(60);

            _blacks = _boards[Piece.BlackPawn.AsByte()] |
                      _boards[Piece.BlackRook.AsByte()] |
                      _boards[Piece.BlackKnight.AsByte()] |
                      _boards[Piece.BlackBishop.AsByte()] |
                      _boards[Piece.BlackQueen.AsByte()] |
                      _boards[Piece.BlackKing.AsByte()];

            _empty = ~(_whites | _blacks);

            for (var i = 0; i < _boards.Length; i++)
            {
                foreach (var b in _boards[i].BitScan())
                {
                    _positionCollections[i].Add(b);
                }
            }

            foreach (var piece in Enum.GetValues(typeof(Piece)).OfType<Piece>())
            {
                var positionCollection = _positionCollections[piece.AsByte()];
                for (var index = 0; index < positionCollection.Count; index++)
                {
                    _pieces[positionCollection[index]] = piece;
                }
            }
        }

        private void SetFilesAndRanks()
        {
            BitBoard rank = new BitBoard(0);
            rank = rank.Set(Enumerable.Range(0, 8).ToArray());
            _ranks = new BitBoard[8];
            for (var i = 0; i < _ranks.Length; i++)
            {
                _ranks[i] = rank;
                rank = rank << 8;
            }

            _files = new BitBoard[8];
            BitBoard file = new BitBoard(0);
            for (int i = 0; i < 60; i += 8)
            {
                file = file.Set(i);
            }

            for (var i = 0; i < _files.Length; i++)
            {
                _files[i] = file;
                file = file << 1;
            }

            _rookFiles = new BitBoard[64];
            for (var i = 0; i < _rookFiles.Length; i++)
            {
                _rookFiles[i] = _files[i % 8] ^ i.AsBitBoard();
            }

            _notFileA = ~_files[0];
            _notFileH = ~_files[7];
        }

        private void SetCastles()
        {
            _whiteSmallCastleCondition = new BitBoard();
            _whiteSmallCastleCondition = _whiteSmallCastleCondition.Set(5, 6);

            _whiteBigCastleCondition = new BitBoard();
            _whiteBigCastleCondition = _whiteBigCastleCondition.Set(1, 2, 3);

            _blackSmallCastleCondition = new BitBoard();
            _blackSmallCastleCondition = _blackSmallCastleCondition.Set(61, 62);

            _blackBigCastleCondition = new BitBoard();
            _blackBigCastleCondition = _blackBigCastleCondition.Set(57, 58, 59);

            _whiteBigCastleKing = new BitBoard();
            _whiteBigCastleKing = _whiteBigCastleKing.Or(4, 2);

            _whiteBigCastleRook = new BitBoard();
            _whiteBigCastleRook = _whiteBigCastleRook.Or(0, 3);

            _whiteSmallCastleKing = new BitBoard();
            _whiteSmallCastleKing = _whiteSmallCastleKing.Or(4, 6);

            _whiteSmallCastleRook = new BitBoard();
            _whiteSmallCastleRook = _whiteSmallCastleRook.Or(5, 7);

            _blackBigCastleKing = new BitBoard();
            _blackBigCastleKing = _blackBigCastleKing.Or(58, 60);

            _blackBigCastleRook = new BitBoard();
            _blackBigCastleRook = _blackBigCastleRook.Or(56, 59);

            _blackSmallCastleKing = new BitBoard();
            _blackSmallCastleKing = _blackSmallCastleKing.Or(60, 62);

            _blackSmallCastleRook = new BitBoard();
            _blackSmallCastleRook = _blackSmallCastleRook.Or(61, 63);
        }

        #endregion

        public override string ToString()
        {
            char[] pieceUnicodeChar =
            {
                '\u2659', '\u2658', '\u2657', '\u2656', '\u2655', '\u2654',
                '\u265F', '\u265E', '\u265D', '\u265C', '\u265B', '\u265A', ' '
            };
            var piecesNames = pieceUnicodeChar.Select(c => c.ToString()).ToArray();

            StringBuilder builder = new StringBuilder();
            for (int y = 7; y >= 0; y--)
            {
                for (int x = 0; x < 8; x++)
                {
                    var i = y * 8 + x;
                    string v = piecesNames.Last();
                    if (!_empty.IsSet(i.AsBitBoard()))
                    {
                        v = piecesNames[_pieces[i].AsByte()];
                    }

                    builder.Append($"[ {v} ]");
                }

                builder.AppendLine();
            }

            return builder.ToString();
        }
    }
}
