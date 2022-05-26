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
        //private bool _isWhiteCastled;
        //private bool _isBlackCastled;
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
        private int[] _pieceCount;
        private readonly bool[] _overBoard;
        private readonly Piece[] _pieces;
        private readonly BitBoard _whiteQueenOpening;
        private readonly BitBoard _blackQueenOpening;
        private readonly IMoveProvider _moveProvider;
        private readonly IMoveHistoryService _moveHistory;
        private readonly IEvaluationService _evaluationService;
        private readonly IAttackEvaluationService _attackEvaluationService;

        public Board()
        {
            _pieces = new Piece[64];
            _overBoard = new bool[64];

            SetPieces();

            SetBoards();

            SetFilesAndRanks();

            SetCastles();

            _moveProvider = ServiceLocator.Current.GetInstance<IMoveProvider>();
            _moveHistory = ServiceLocator.Current.GetInstance<IMoveHistoryService>();
            _evaluationService = ServiceLocator.Current.GetInstance<IEvaluationService>();
            _attackEvaluationService = ServiceLocator.Current.GetInstance<IAttackEvaluationService>();

            _hash = new ZobristHash();
            _hash.Initialize(_boards);

            _moveProvider.SetBoard(this);

            _whiteQueenOpening = Squares.D1.AsBitBoard() | Squares.E1.AsBitBoard() | Squares.C1.AsBitBoard() |
                                 Squares.D2.AsBitBoard() | Squares.E2.AsBitBoard() | Squares.C2.AsBitBoard();

            _blackQueenOpening = Squares.D8.AsBitBoard() | Squares.E8.AsBitBoard() | Squares.C8.AsBitBoard() |
                                 Squares.D7.AsBitBoard() | Squares.E7.AsBitBoard() | Squares.C7.AsBitBoard();
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
        private int GetStaticValue(byte piece, byte[] positions)
        {
            int value = 0;
            for (var i = 0; i < positions.Length; i++)
            {
                value += _evaluationService.GetFullValue(piece, positions[i], _phase);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetStaticValue()
        {
            return GetWhiteStaticValue() - GetBlackStaticValue();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetBlackStaticValue()
        {
            int value = 0;
            for (byte i = 6; i < 11; i++)
            {
                value += _evaluationService.GetValue(i, _phase) * _pieceCount[i];
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetWhiteStaticValue()
        {
            int value = 0;
            for (byte i = 0; i < 5; i++)
            {
                value += _evaluationService.GetValue(i, _phase) * _pieceCount[i];
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short GetValue()
        {
            return (short) (GetWhiteValue() - GetBlackValue());
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
            return _evaluationService.GetValue(Piece.BlackKing.AsByte(), _boards[Piece.BlackKing.AsByte()].BitScanForward(), _phase);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetBlackQueenValue()
        {
            var piece = Piece.BlackQueen.AsByte();
            byte[] queens = GetPositionInternal(piece);
            if (queens.Length <= 0) return 0;

            var value = GetStaticValue(piece, queens);

            for (var i = 0; i < queens.Length; i++)
            {
                var attackPattern = _moveProvider.GetAttackPattern(piece, queens[i]);
                if (attackPattern.IsSet(_boards[Piece.WhiteKing.AsByte()]))
                {
                    value += _evaluationService.GetRentgenValue(_phase);
                }

                if (_phase != Phase.Middle) continue;
                var countOfBlockingPawns = (_moveProvider.GetAttackPattern(Piece.BlackPawn.AsByte(), queens[i]) &
                                            _boards[Piece.BlackPawn.AsByte()]).Count();

                value -= countOfBlockingPawns * _evaluationService.GetBishopBlockedByPawnValue(_phase);
            }

            if (_phase != Phase.Opening) return value;

            if ((_blackQueenOpening&_boards[Piece.BlackQueen.AsByte()]).IsZero())
            {
                value -= _evaluationService.GetEarlyQueenValue(_phase);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetBlackRookValue()
        {
            byte[] rooks = GetPositionInternal(Piece.BlackRook.AsByte());
            if (rooks.Length == 0) return 0;
            var value = GetStaticValue(Piece.BlackRook.AsByte(), rooks);

            if (_phase == Phase.Opening) return value;

            for (var i = 0; i < rooks.Length; i++)
            {
                BitBoard file = GetFile(rooks[i]);
                if ((_empty ^ rooks[i].AsBitBoard()).IsSet(file))
                {
                    value += _evaluationService.GetRookOnOpenFileValue(_phase);
                }
                else if ((_boards[Piece.WhitePawn.AsByte()] & file).IsZero() ||
                         (_boards[Piece.BlackPawn.AsByte()] & file).IsZero())
                {
                    value += _evaluationService.GetRookOnHalfOpenFileValue(_phase);
                }

                if (_boards[Piece.WhiteQueen.AsByte()].Any() && file.IsSet(_boards[Piece.WhiteQueen.AsByte()]))
                {
                    value += _evaluationService.GetRentgenValue(_phase);
                }

                if (file.IsSet(_boards[Piece.WhiteKing.AsByte()]))
                {
                    value += _evaluationService.GetRentgenValue(_phase);
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
            byte[] bishops = GetPositionInternal(Piece.BlackBishop.AsByte());
            if (bishops.Length <= 0) return 0;

            var value = GetStaticValue(Piece.BlackBishop.AsByte(), bishops);
            if (bishops.Length == 2)
            {
                value += _evaluationService.GetDoubleBishopValue(_phase);
            }

            for (var i = 0; i < bishops.Length; i++)
            {
                if ((_moveProvider.GetAttackPattern(Piece.WhitePawn.AsByte(), bishops[i]) &
                     _boards[Piece.BlackPawn.AsByte()]).Any())
                {
                    value += _evaluationService.GetMinorDefendedByPawnValue(_phase);
                }
                var attackPattern = _moveProvider.GetAttackPattern(Piece.BlackBishop.AsByte(), bishops[i]);
                if (_boards[Piece.WhiteQueen.AsByte()].Any() && attackPattern.IsSet(_boards[Piece.WhiteQueen.AsByte()]))
                {
                    value += _evaluationService.GetRentgenValue(_phase);
                }
                if (attackPattern.IsSet(_boards[Piece.WhiteKing.AsByte()]))
                {
                    value += _evaluationService.GetRentgenValue(_phase);
                }

                if (_phase != Phase.Middle) continue;
                var countOfBlockingPawns = (_moveProvider.GetAttackPattern(Piece.BlackPawn.AsByte(), bishops[i]) &
                                            _boards[Piece.BlackPawn.AsByte()]).Count();

                value -= countOfBlockingPawns * _evaluationService.GetBishopBlockedByPawnValue(_phase);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetBlackKnightValue()
        {
            byte[] knights = GetPositionInternal(Piece.BlackKnight.AsByte());
            if (knights.Length <= 0) return 0;

            var value = GetStaticValue(Piece.BlackKnight.AsByte(), knights);

            for (var i = 0; i < knights.Length; i++)
            {
                if ((_moveProvider.GetAttackPattern(Piece.WhitePawn.AsByte(), knights[i]) &
                     _boards[Piece.BlackPawn.AsByte()]).Any())
                {
                    value += _evaluationService.GetMinorDefendedByPawnValue(_phase);
                }

                var emptyCells = _empty & _moveProvider.GetAttackPattern(Piece.BlackKnight.AsByte(), knights[i]);
                while (!emptyCells.IsZero())
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
            var pawns = new int[8];
            var piece = Piece.BlackPawn.AsByte();
            byte[] positions = GetPositionInternal(piece);
            for (var i = 0; i < positions.Length; i++)
            {
                byte coordinate = positions[i];
                value += _evaluationService.GetFullValue(piece, coordinate, _phase);
                if (_whites.IsSet((coordinate - 8).AsBitBoard()))
                {
                    value -= _evaluationService.GetBlockedPawnValue(_phase);
                }

                var file = coordinate % 8;
                if (file == 0)
                {
                    value = BlackBackwardPawn(_files[1], coordinate, value, file);
                }
                else if (file == 7)
                {
                    value = BlackBackwardPawn(_files[6], coordinate, value, file);
                }
                else
                {
                    value = BlackBackwardPawn(_files[file - 1] | _files[file + 1], coordinate, value, file);
                }

                if ((_files[file] & _boards[Piece.WhitePawn.AsByte()]).IsZero())
                {
                    if (coordinate / 8 < 4)
                    {
                        value += _evaluationService.GetPassedPawnValue(_phase);
                    }
                }

                pawns[file]++;
            }

            return GetPawnValue(value, pawns);
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
            return _evaluationService.GetValue(Piece.WhiteKing.AsByte(), _boards[Piece.WhiteKing.AsByte()].BitScanForward(), _phase);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetWhiteQueenValue()
        {
            var piece = Piece.WhiteQueen.AsByte();
            byte[] queens = GetPositionInternal(piece);
            if (queens.Length <= 0) return 0;

            var value = GetStaticValue(piece, queens);

            for (var i = 0; i < queens.Length; i++)
            {
                var attackPattern = _moveProvider.GetAttackPattern(piece, queens[i]);
                if (attackPattern.IsSet(_boards[Piece.BlackKing.AsByte()]))
                {
                    value += _evaluationService.GetRentgenValue(_phase);
                }

                if (_phase != Phase.Middle) continue;
                var countOfBlockingPawns = (_moveProvider.GetAttackPattern(Piece.WhitePawn.AsByte(), queens[i]) &
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
            byte[] rooks = GetPositionInternal(piece);
            if (rooks.Length == 0) return 0;
            var value = GetStaticValue(piece, rooks);

            if (_phase == Phase.Opening) return value;

            for (var i = 0; i < rooks.Length; i++)
            {
                BitBoard file = GetFile(rooks[i]);
                if ((_empty ^ rooks[i].AsBitBoard()).IsSet(file))
                {
                    value += _evaluationService.GetRookOnOpenFileValue(_phase);
                }

                else if ((_boards[Piece.WhitePawn.AsByte()] & file).IsZero()|| (_boards[Piece.BlackPawn.AsByte()] & file).IsZero())
                {
                    value += _evaluationService.GetRookOnHalfOpenFileValue(_phase);
                }

                if (_boards[Piece.BlackQueen.AsByte()].Any() && file.IsSet(_boards[Piece.BlackQueen.AsByte()]))
                {
                    value += _evaluationService.GetRentgenValue(_phase);
                }

                if (file.IsSet(_boards[Piece.BlackKing.AsByte()]))
                {
                    value += _evaluationService.GetRentgenValue(_phase);
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
            byte[] bishops = GetPositionInternal(piece);
            if (bishops.Length <= 0) return 0;

            var value = GetStaticValue(piece, bishops);
            if (bishops.Length == 2)
            {
                value += _evaluationService.GetDoubleBishopValue(_phase);
            }

            for (var i = 0; i < bishops.Length; i++)
            {
                if ((_moveProvider.GetAttackPattern(Piece.BlackPawn.AsByte(), bishops[i]) &
                     _boards[Piece.WhitePawn.AsByte()]).Any())
                {
                    value += _evaluationService.GetMinorDefendedByPawnValue(_phase);
                }
                var attackPattern = _moveProvider.GetAttackPattern(piece, bishops[i]);
                if (_boards[Piece.BlackQueen.AsByte()].Any() && attackPattern.IsSet(_boards[Piece.BlackQueen.AsByte()]))
                {
                    value += _evaluationService.GetRentgenValue(_phase);
                }
                if (attackPattern.IsSet(_boards[Piece.BlackKing.AsByte()]))
                {
                    value += _evaluationService.GetRentgenValue(_phase);
                }

                if (_phase != Phase.Middle) continue;
                var countOfBlockingPawns = (_moveProvider.GetAttackPattern(Piece.WhitePawn.AsByte(), bishops[i]) &
                                            _boards[Piece.WhitePawn.AsByte()]).Count();

                value -= countOfBlockingPawns * _evaluationService.GetBishopBlockedByPawnValue(_phase);
            }
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetWhiteKnightValue()
        {
            byte[] knights = GetPositionInternal(Piece.WhiteKnight.AsByte());
            if (knights.Length <= 0) return 0;

            var value = GetStaticValue(Piece.WhiteKnight.AsByte(), knights);

            for (var i = 0; i < knights.Length; i++)
            {
                if ((_moveProvider.GetAttackPattern(Piece.BlackPawn.AsByte(), knights[i]) &
                     _boards[Piece.WhitePawn.AsByte()]).Any())
                {
                    value += _evaluationService.GetMinorDefendedByPawnValue(_phase);
                }

                var emptyCells = _empty & _moveProvider.GetAttackPattern(Piece.WhiteKnight.AsByte(), knights[i]);
                while (!emptyCells.IsZero())
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
            var pawns = new int[8];
            var piece = Piece.WhitePawn.AsByte();
            byte[] positions = GetPositionInternal(piece);
            for (var i = 0; i < positions.Length; i++)
            {
                byte coordinate = positions[i];
                value += _evaluationService.GetFullValue(piece, coordinate, _phase);
                if (_blacks.IsSet((coordinate + 8).AsBitBoard()))
                {
                    value -= _evaluationService.GetBlockedPawnValue(_phase);
                }

                var file = coordinate % 8;
                if (file == 0)
                {
                    value = WhiteBackwardPawn(_files[1], coordinate, value, file);
                }
                else if (file == 7)
                {
                    value = WhiteBackwardPawn(_files[6], coordinate, value, file);
                }
                else
                {
                    value = WhiteBackwardPawn(_files[file - 1] | _files[file + 1], coordinate, value, file);
                }

                if ((_files[file] & _boards[Piece.BlackPawn.AsByte()]).IsZero())
                {
                    if (coordinate / 8 > 3)
                    {
                        value += _evaluationService.GetPassedPawnValue(_phase);
                    }
                }

                pawns[file]++;
            }

            return GetPawnValue(value, pawns);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int BlackBackwardPawn(BitBoard fBit, byte coordinate, int value, int file)
        {
            var b = fBit & _boards[Piece.BlackPawn.AsByte()];
            if (!b.Any()) return value;

            byte lsb = b.BitScanReverse();
            if (!lsb.IsLessRank(coordinate)) return value;

            byte weakPosition = (byte) (lsb / 8 * 8 + file);
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

            byte weakPosition = (byte) (lsb / 8 * 8 + file);
            var attackPattern = _moveProvider.GetAttackPattern(Piece.WhitePawn.AsByte(), weakPosition);
            var b = attackPattern & _boards[Piece.BlackPawn.AsByte()];
            if (b.Any())
            {
                value -= _evaluationService.GetBackwardPawnValue(_phase);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte[] GetPositionInternal(byte piece)
        {
            var b = _boards[piece];
            byte[] positions = new byte[_pieceCount[piece]];
            for (var i = 0; i < positions.Length; i++)
            {
                var position = b.BitScanForward();
                positions[i] = position;
                b = b.Remove(position);
            }

            return positions;
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

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public Piece GetPiece(int cell)
        //{
        //    return _pieces[cell];
        //}

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
            _pieceCount[piece.AsByte()]--;

            Remove(piece, square.AsBitBoard());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(Piece piece, Square square)
        {
            _hash.Update(square.AsByte(), piece.AsByte());
            _pieceCount[piece.AsByte()]++;
            _pieces[square.AsByte()] = piece;

            Add(piece, square.AsBitBoard());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Move(Piece piece, Square from, Square to)
        {
            _hash.Update(from.AsByte(), to.AsByte(), piece.AsByte());
            _pieces[to.AsByte()] = piece;

            Move(piece, from.AsBitBoard() | to.AsBitBoard());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Square GetWhiteKingPosition()
        {
            return new Square(_boards[Piece.WhiteKing.AsByte()].BitScanForward());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Square GetBlackKingPosition()
        {
            return new Square(_boards[Piece.BlackKing.AsByte()].BitScanForward());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DynamicArray<byte> GetPositions(int index)
        {
            return _boards[index].Coordinates(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong GetKey()
        {
            return _hash.Key;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Square[] GetPiecePositions(byte piece)
        {
            return _boards[piece].GetCoordinates(piece, _pieceCount[piece]);
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
            int count = _pieceCount[Piece.BlackRook.AsByte()] + _pieceCount[Piece.BlackQueen.AsByte()];
            if (count > 2)
            {
                return false;
            }

            count += _pieceCount[Piece.BlackBishop.AsByte()];
            if (count > 2)
            {
                return false;
            }

            count += _pieceCount[Piece.BlackKnight.AsByte()];
            return count <= 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsEndGameForWhite()
        {
            int count = _pieceCount[Piece.WhiteRook.AsByte()] + _pieceCount[Piece.WhiteQueen.AsByte()];
            if (count > 2)
            {
                return false;
            }

            count += _pieceCount[Piece.WhiteBishop.AsByte()];
            if (count > 2)
            {
                return false;
            }

            count += _pieceCount[Piece.WhiteKnight.AsByte()];
            return count <= 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetBlackMaxValue()
        {
            int value = 0;
            for (byte i = 10; i > 5; i--)
            {
                if (_pieceCount[i] > 0)
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
                if (_pieceCount[i] > 0)
                {
                    return _evaluationService.GetValue(i, _phase);
                }
            }
            if (_pieceCount[0] > 0)
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

        #endregion

        #region SEE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int StaticExchange(AttackBase attack)
        {
            _attackEvaluationService.Initialize(_boards,_whites|_blacks,_phase);
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

            //_isWhiteCastled = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DoBlackSmallCastle()
        {
            _pieces[Squares.G8.AsByte()] = Piece.BlackKing;
            _pieces[Squares.F8.AsByte()] = Piece.BlackRook;

            _hash.Update(Squares.H8.AsByte(), Squares.F8.AsByte(), Piece.BlackRook.AsByte());
            _hash.Update(Squares.E8.AsByte(), Squares.G8.AsByte(), Piece.BlackKing.AsByte());

            BlackSmallCastle();

            //_isBlackCastled = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DoBlackBigCastle()
        {
            _pieces[Squares.C8.AsByte()] = Piece.BlackKing;
            _pieces[Squares.D8.AsByte()] = Piece.BlackRook;

            _hash.Update(Squares.A8.AsByte(), Squares.D8.AsByte(), Piece.BlackRook.AsByte());
            _hash.Update(Squares.E8.AsByte(), Squares.C8.AsByte(), Piece.BlackKing.AsByte());

            BlackBigCastle();

            //_isBlackCastled = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DoWhiteBigCastle()
        {
            _pieces[Squares.C1.AsByte()] = Piece.WhiteKing;
            _pieces[Squares.D1.AsByte()] = Piece.WhiteRook;

            _hash.Update(Squares.A1.AsByte(), Squares.D1.AsByte(), Piece.WhiteRook.AsByte());
            _hash.Update(Squares.E1.AsByte(), Squares.C1.AsByte(), Piece.WhiteKing.AsByte());

            WhiteBigCastle();

            //_isWhiteCastled = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UndoWhiteSmallCastle()
        {
            _pieces[Squares.E1.AsByte()] = Piece.WhiteKing;
            _pieces[Squares.H1.AsByte()] = Piece.WhiteRook;

            _hash.Update(Squares.F1.AsByte(), Squares.H1.AsByte(), Piece.WhiteRook.AsByte());
            _hash.Update(Squares.G1.AsByte(), Squares.E1.AsByte(), Piece.WhiteKing.AsByte());

            WhiteSmallCastle();

            //_isWhiteCastled = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UndoBlackSmallCastle()
        {
            _pieces[Squares.E8.AsByte()] = Piece.BlackKing;
            _pieces[Squares.H8.AsByte()] = Piece.BlackRook;

            _hash.Update(Squares.F8.AsByte(), Squares.H8.AsByte(), Piece.BlackRook.AsByte());
            _hash.Update(Squares.G8.AsByte(), Squares.E8.AsByte(), Piece.BlackKing.AsByte());

            BlackSmallCastle();

           // _isBlackCastled = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UndoWhiteBigCastle()
        {
            _pieces[Squares.E1.AsByte()] = Piece.WhiteKing;
            _pieces[Squares.A1.AsByte()] = Piece.WhiteRook;

            _hash.Update(Squares.D1.AsByte(), Squares.A1.AsByte(), Piece.WhiteRook.AsByte());
            _hash.Update(Squares.C1.AsByte(), Squares.E1.AsByte(), Piece.WhiteKing.AsByte());

            WhiteBigCastle();

            //_isWhiteCastled = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UndoBlackBigCastle()
        {
            _pieces[Squares.E8.AsByte()] = Piece.BlackKing;
            _pieces[Squares.A8.AsByte()] = Piece.BlackRook;

            _hash.Update(Squares.D8.AsByte(), Squares.A8.AsByte(), Piece.BlackRook.AsByte());
            _hash.Update(Squares.C8.AsByte(), Squares.E8.AsByte(), Piece.BlackKing.AsByte());

            BlackBigCastle();

            //_isBlackCastled = false;
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
                     _moveProvider.IsUnderAttack(Piece.BlackPawn.AsByte(), to));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanDoBlackCastle(byte to)
        {
            return !(_moveProvider.IsUnderAttack(Piece.WhiteBishop.AsByte(), to) ||
                     _moveProvider.IsUnderAttack(Piece.WhiteKnight.AsByte(), to) ||
                     _moveProvider.IsUnderAttack(Piece.WhiteQueen.AsByte(), to) ||
                     _moveProvider.IsUnderAttack(Piece.WhiteRook.AsByte(), to) ||
                     _moveProvider.IsUnderAttack(Piece.WhitePawn.AsByte(), to));
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

            foreach (var piece in Enum.GetValues(typeof(Piece)).OfType<Piece>())
            {
                var coordinates = _boards[piece.AsByte()].Coordinates(piece.AsByte());
                for (var index = 0; index < _pieceCount[piece.AsByte()]; index++)
                {
                    _pieces[coordinates[index]] = piece;
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
        }

        private void SetPieces()
        {
            _pieceCount = new int[12];

            _pieceCount[Piece.WhitePawn.AsByte()] = 8;
            _pieceCount[Piece.WhiteKnight.AsByte()] = 2;
            _pieceCount[Piece.WhiteBishop.AsByte()] = 2;
            _pieceCount[Piece.WhiteRook.AsByte()] = 2;
            _pieceCount[Piece.WhiteQueen.AsByte()] = 1;
            _pieceCount[Piece.WhiteKing.AsByte()] = 1;
            _pieceCount[Piece.BlackPawn.AsByte()] = 8;
            _pieceCount[Piece.BlackRook.AsByte()] = 2;
            _pieceCount[Piece.BlackKnight.AsByte()] = 2;
            _pieceCount[Piece.BlackBishop.AsByte()] = 2;
            _pieceCount[Piece.BlackQueen.AsByte()] = 1;
            _pieceCount[Piece.BlackKing.AsByte()] = 1;
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
                        //v = _pieces[i].AsString();
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
