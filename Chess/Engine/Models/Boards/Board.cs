using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using CommonServiceLocator;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Enums;
using Engine.Models.Helpers;

namespace Engine.Models.Boards
{
    public class Board : IBoard
    {
        private bool _isWhiteCastled;
        private bool _isBlackCastled;
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
        private readonly IMoveProvider _moveProvider;
        private readonly IMoveHistoryService _moveHistory;
        private readonly IEvaluationService _evaluationService;

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

            _hash = new ZobristHash();
            _hash.Initialize(_boards);
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
        public bool IsOpposite(BitBoard bitBoard, Piece piece)
        {
            return piece.IsWhite() ? _blacks.IsSet(bitBoard) : _whites.IsSet(bitBoard);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetStaticValue(byte piece, int[] positions)
        {
            int value = 0;
            for (var i = 0; i < positions.Length; i++)
            {
                value += _evaluationService.GetFullValue(piece, positions[i]);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetValue()
        {
            return GetWhiteValue() - GetBlackValue();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetBlackValue()
        {
            var pawns = GetPositionInternal(Piece.BlackPawn.AsByte());
            var knights = GetPositionInternal(Piece.BlackKnight.AsByte());
            var bishops = GetPositionInternal(Piece.BlackBishop.AsByte());
            var rooks = GetPositionInternal(Piece.BlackRook.AsByte());
            var queens = GetPositionInternal(Piece.BlackQueen.AsByte());
            return GetBlackPawnValue(pawns) + GetBlackKnightValue(knights, pawns) + GetBlackBishopValue(bishops) +
                   GetBlackRookValue(rooks,pawns) + GetBlackQueenValue(queens) + GetBlackKingValue();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetBlackKingValue()
        {
            var p = _boards[Piece.BlackKing.AsByte()].BitScanForward();
            var value = _evaluationService.GetValue(Piece.BlackKing.AsByte(), p);

            if (_isBlackCastled)
            {
                value += _evaluationService.GetPawnValue(2);
            }
            else if (!_moveHistory.CanDoBlackCastle())
            {
                value -= _evaluationService.GetPawnValue(2);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetBlackQueenValue(int[] queens)
        {
            var value = GetStaticValue(Piece.BlackQueen.AsByte(), queens);
            if (_phase != Phase.Opening) return value;

            if (queens.Length <= 0) return value;

            if (queens[0] != Squares.D8.AsInt())
            {
                value -= _evaluationService.GetPawnValue(3);
            }
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetBlackRookValue(int[] rooks, int[] pawns)
        {
            var value = GetStaticValue(Piece.BlackRook.AsByte(), rooks);
            value -= _evaluationService.GetUnitValue(pawns.Length);
            if (_phase == Phase.Opening) return value;

            return UpdateRookValue(rooks, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetBlackBishopValue(int[] bishops)
        {
            var value = GetStaticValue(Piece.BlackBishop.AsByte(), bishops);
            if (_pieceCount[Piece.BlackBishop.AsByte()] < 2)
            {
                value -= _evaluationService.GetPawnValue(2);
            }
            for (var i = 0; i < bishops.Length; i++)
            {
                value = value + _evaluationService.GetUnitValue(bishops[i].BishopAttacks(~_empty).Count());
            }
            value = CheckUndeveloped(Piece.BlackBishop.AsByte(), _ranks[7], value);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetBlackKnightValue(int[] knights, int[] pawns)
        {
            var value = GetStaticValue(Piece.BlackKnight.AsByte(), knights);
            value += _evaluationService.GetUnitValue(pawns.Length);
            if (knights.Length <= 0) return value;
            for (var i = 0; i < knights.Length; i++)
            {
                var bit = knights[i].AsBitBoard();
                for (var j = 0; j < pawns.Length; j++)
                {
                    var pattern = _moveProvider.GetAttackPattern(Piece.BlackPawn.AsByte(), pawns[j]);
                    if (!pattern.IsSet(bit)) continue;

                    value += _evaluationService.GetPenaltyValue();
                    break;
                }
            }
            value = CheckUndeveloped(Piece.BlackKnight.AsByte(), _ranks[7], value);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetBlackPawnValue(int[] positions)
        {
            int value = 0;
            var pawns = new int[8];
            var piece = Piece.BlackPawn.AsByte();
            for (var i = 0; i < positions.Length; i++)
            {
                int coordinate = positions[i];
                value += _evaluationService.GetFullValue(piece, coordinate);
                if (_whites.IsSet((coordinate - 8).AsBitBoard()))
                {
                    value = value - _evaluationService.GetPawnValue(2);
                }

                pawns[coordinate % 8]++;
            }

            return GetPawnValue(value, _evaluationService.GetPawnValue(2),_evaluationService.GetPawnValue(4), pawns);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetWhiteValue()
        {
            var pawns = GetPositionInternal(Piece.WhitePawn.AsByte());
            var knights = GetPositionInternal(Piece.WhiteKnight.AsByte());
            var bishops = GetPositionInternal(Piece.WhiteBishop.AsByte());
            var rooks = GetPositionInternal(Piece.WhiteRook.AsByte());
            var queens = GetPositionInternal(Piece.WhiteQueen.AsByte());
            return GetWhitePawnValue(pawns) + GetWhiteKnightValue(knights, pawns) + GetWhiteBishopValue(bishops) +
                   GetWhiteRookValue(rooks, pawns) + GetWhiteQueenValue(queens) + GetWhiteKingValue();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetWhiteKingValue()
        {
            var p = _boards[Piece.WhiteKing.AsByte()].BitScanForward();
            var value = _evaluationService.GetValue(Piece.WhiteKing.AsByte(), p);

            if (_isWhiteCastled)
            {
                value += _evaluationService.GetPawnValue(2);
            }
            else if (!_moveHistory.CanDoWhiteCastle())
            {
                value -= _evaluationService.GetPawnValue(2);
            }
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetWhiteQueenValue(int[] queens)
        {
            var value = GetStaticValue(Piece.WhiteQueen.AsByte(),queens);
            if (_phase != Phase.Opening) return value;

            if (queens.Length < 1 || queens[0] != Squares.D1.AsInt())
            {
                value -= _evaluationService.GetPawnValue(3);
            }
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetWhiteRookValue(int[] rooks, int[] pawns)
        {
            var value = GetStaticValue(Piece.WhiteRook.AsByte(),rooks);
            value -= _evaluationService.GetUnitValue(pawns.Length);

            if (_phase == Phase.Opening) return value;

            return UpdateRookValue(rooks, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetWhiteBishopValue(int[] bishops)
        {
            var value = GetStaticValue(Piece.WhiteBishop.AsByte(),bishops);
            if (_pieceCount[Piece.WhiteBishop.AsByte()] < 2)
            {
                value -= _evaluationService.GetPawnValue(2);
            }
            for (var i = 0; i < bishops.Length; i++)
            {
                value = value + _evaluationService.GetUnitValue(bishops[i].BishopAttacks(~_empty).Count());
            }
            value = CheckUndeveloped(Piece.WhiteBishop.AsByte(), _ranks[0], value);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetWhiteKnightValue(int[] knights, int[] pawns)
        {
            var piece = Piece.WhiteKnight.AsByte();
            var value = GetStaticValue(piece, knights);

            value += _evaluationService.GetUnitValue(pawns.Length);
            if (knights.Length <= 0) return value;
            for (var i = 0; i < knights.Length; i++)
            {
                var bit = knights[i].AsBitBoard();
                for (var j = 0; j < pawns.Length; j++)
                {
                    var pattern = _moveProvider.GetAttackPattern(Piece.WhitePawn.AsByte(), pawns[j]);
                    if (!pattern.IsSet(bit)) continue;

                    value += _evaluationService.GetPenaltyValue();
                    break;
                }
            }

            value = CheckUndeveloped(piece, _ranks[0], value);
            return value;
        }

        private int CheckUndeveloped(byte piece, BitBoard rank, int value)
        {
            if (_phase == Phase.Middle && (_boards[piece] & rank).Any())
            {
                value -= _evaluationService.GetPenaltyValue();
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetWhitePawnValue(int[] positions)
        {
            int value = 0;
            var pawns = new int[8];
            var piece = Piece.WhitePawn.AsByte();
            for (var i = 0; i < positions.Length; i++)
            {
                int coordinate = positions[i];
                value += _evaluationService.GetFullValue(piece, coordinate);
                if (_blacks.IsSet((coordinate + 8).AsBitBoard()))
                {
                    value = value - _evaluationService.GetPawnValue(2);
                }

                pawns[coordinate % 8]++;
            }

            return GetPawnValue(value, _evaluationService.GetPawnValue(2), _evaluationService.GetPawnValue(4), pawns);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int UpdateRookValue(int[] rooks, int value)
        {
            for (var i = 0; i < rooks.Length; i++)
            {
                BitBoard file = GetFile(rooks[i]);
                if (_empty.IsSet(file))
                {
                    value += _evaluationService.GetPawnValue(3);
                }
            }

            if (rooks.Length == 2)
            {
                if (rooks[0].RookAttacks(~_empty).IsSet(rooks[1].AsBitBoard()))
                {
                    value += _evaluationService.GetPawnValue(2);
                }
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int[] GetPositionInternal(int piece)
        {
            var b = _boards[piece];
            int[] positions = new int[_pieceCount[piece]];
            for (var i = 0; i < positions.Length; i++)
            {
                var position = b.BitScanForward();
                positions[i] = position;
                b = b.Remove(position);
            }

            return positions;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateCastles(int value)
        {
            if (_isWhiteCastled)
            {
                value += _evaluationService.GetPawnValue(2);
            }
            else
            {
                if (!_moveHistory.CanDoWhiteSmallCastle())
                {
                    value -= _evaluationService.GetPawnValue();
                }

                if (!_moveHistory.CanDoWhiteBigCastle())
                {
                    value -= _evaluationService.GetPawnValue();
                }
            }

            if (_isBlackCastled)
            {
                value -= _evaluationService.GetPawnValue(2);
            }
            else
            {
                if (!_moveHistory.CanDoBlackSmallCastle())
                {
                    value += _evaluationService.GetPawnValue();
                }

                if (!_moveHistory.CanDoBlackBigCastle())
                {
                    value += _evaluationService.GetPawnValue();
                }
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetPawnValue(int value, int doubledPawn, int isolatedPawn, int[] pawns)
        {
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
                    value = value - doubledPawn;
                }

                if (pawns[i - 1] == 0 && pawns[i + 1] == 0)
                {
                    value = value - isolatedPawn;
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
        public Piece GetPiece(int cell)
        {
            return _pieces[cell];
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
        public void SetOver(Square to, bool b)
        {
            _overBoard[to.AsByte()] = b;
        }

        public bool IsOver(int square)
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
        public DynamicArray<int> GetPositions(int index)
        {
            return _boards[index].Coordinates(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong GetKey()
        {
            return _hash.Key();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Square[] GetPiecePositions(int piece)
        {
            int i = 0;
            Square[] squares = new Square[_pieceCount[piece]];
            var ints = _boards[piece].Coordinates(piece);
            for (var index = 0; index < _pieceCount[piece]; index++)
            {
                squares[i++] = new Square(ints[index]);
            }

            return squares;
        }

        public BitBoard GetOccupied()
        {
            return ~_empty;
        }

        public void UpdatePhase()
        {
            var ply = _moveHistory.GetPly();
            _phase = ply < 16 ? Phase.Opening : Phase.Middle;
        }

        public int StaticExchange(IAttack attack)
        {
            var boards = new BitBoard[_boards.Length];
            Array.Copy(_boards,boards, _boards.Length);
            var whites = _whites;
            var blacks = _blacks;
            BitBoard occupied = whites | blacks;

            BitBoard mayXRay = boards[Piece.BlackPawn.AsByte()] |
                               boards[Piece.BlackRook.AsByte()] |
                               boards[Piece.BlackBishop.AsByte()] |
                               boards[Piece.BlackQueen.AsByte()] |
                               boards[Piece.WhitePawn.AsByte()] |
                               boards[Piece.WhiteBishop.AsByte()] |
                               boards[Piece.WhiteRook.AsByte()] |
                               boards[Piece.WhiteQueen.AsByte()];

            BitBoard fromSet = attack.From.AsBitBoard();

            var to = attack.To.AsBitBoard();
            BitBoard attackers = GetAttackers(to, occupied);

            var piece = attack.Piece;
            var target = attack.Captured;
            var v = 0;
            var factor = 1;
            while (fromSet.Any())
            {
                v= v+ factor *_evaluationService.GetValue(target.AsByte());
                factor = -factor;

                attackers ^= fromSet; // reset bit in set to traverse
                occupied ^= fromSet; // reset bit in temporary occupancy (for x-Rays)
                boards[piece.AsByte()] ^= fromSet|to;

                if ((fromSet & mayXRay).Any())
                {
                    attackers |= ConsiderXrays(occupied, to, piece, boards);
                }

                target = piece;
                (fromSet, piece) = GetNextAttacker(attackers, piece, boards);
            }
            return v;
        }

        private BitBoard ConsiderXrays(BitBoard occupied, BitBoard to, Piece piece, BitBoard[] boards)
        {
            BitBoard bit = new BitBoard(0);
            if (piece.IsWhite())
            {
                foreach (var i in boards[Piece.WhiteBishop.AsByte()].BitScan())
                {
                    if((i.BishopAttacks(occupied)&to).Any())
                        bit = bit.Add(i);
                }
                foreach (var i in boards[Piece.WhiteRook.AsByte()].BitScan())
                {
                    if ((i.RookAttacks(occupied) & to).Any())
                        bit = bit.Add(i);
                }
                foreach (var i in boards[Piece.WhiteQueen.AsByte()].BitScan())
                {
                    if ((i.QueenAttacks(occupied) & to).Any())
                        bit = bit.Add(i);
                }
            }
            else
            {
                foreach (var i in boards[Piece.BlackBishop.AsByte()].BitScan())
                {
                    if ((i.BishopAttacks(occupied) & to).Any())
                        bit = bit.Add(i);
                }
                foreach (var i in boards[Piece.BlackRook.AsByte()].BitScan())
                {
                    if ((i.RookAttacks(occupied) & to).Any())
                        bit = bit.Add(i);
                }
                foreach (var i in boards[Piece.BlackQueen.AsByte()].BitScan())
                {
                    if ((i.QueenAttacks(occupied) & to).Any())
                        bit = bit.Add(i);
                }
            }

            return bit;
        }

        private Tuple<BitBoard,Piece> GetNextAttacker(BitBoard attackers, Piece piece, BitBoard[] boards)
        {
            int first = 0;
            int last = 6;
            if (piece.IsWhite())
            {
                first = 6;
                last = 12;
            }
            for (int i = first; i < last; i++)
            {
                var bit = attackers & boards[i];
                if (bit.Any())
                {
                    return new Tuple<BitBoard, Piece>(new BitBoard(bit.Lsb()),(Piece) i);
                }
            }
            return new Tuple<BitBoard, Piece>(new BitBoard(0),Piece.WhitePawn );
        }

        private BitBoard GetAttackers(BitBoard to, BitBoard occupied)
        {
            return GetWhiteAttackers(to, occupied)|GetBlackAttackers(to, occupied);
        }

        private BitBoard GetBlackAttackers(BitBoard to, BitBoard occupied)
        {
            BitBoard attackers = new BitBoard(0);
            var positions = GetPositionInternal(Piece.BlackPawn.AsByte());
            for (var p = 0; p < positions.Length; p++)
            {
                var pattern = _moveProvider.GetBlackPawnAttackPattern(positions[p]);
                if (pattern.IsSet(to))
                {
                    attackers = attackers.Add(positions[p]);
                }
            }

            positions = GetPositionInternal(Piece.BlackKnight.AsByte());
            for (var p = 0; p < positions.Length; p++)
            {
                var pattern = _moveProvider.GetBlackKnightAttackPattern(positions[p]);
                if (pattern.IsSet(to))
                {
                    attackers = attackers.Add(positions[p]);
                }
            }

            positions = GetPositionInternal(Piece.BlackBishop.AsByte());
            for (var p = 0; p < positions.Length; p++)
            {
                var pattern = positions[p].GetBlackBishopAttackPattern(occupied);
                if (pattern.IsSet(to))
                {
                    attackers = attackers.Add(positions[p]);
                }
            }

            positions = GetPositionInternal(Piece.BlackRook.AsByte());
            for (var p = 0; p < positions.Length; p++)
            {
                var pattern = positions[p].GetBlackRookAttackPattern(occupied);
                if (pattern.IsSet(to))
                {
                    attackers = attackers.Add(positions[p]);
                }
            }

            positions = GetPositionInternal(Piece.BlackQueen.AsByte());
            for (var p = 0; p < positions.Length; p++)
            {
                var pattern = positions[p].GetBlackQueenAttackPattern(occupied);
                if (pattern.IsSet(to))
                {
                    attackers = attackers.Add(positions[p]);
                }
            }

            positions = GetPositionInternal(Piece.BlackKing.AsByte());
            for (var p = 0; p < positions.Length; p++)
            {
                var pattern = _moveProvider.GetBlackKingAttackPattern(positions[p]);
                if (pattern.IsSet(to))
                {
                    attackers = attackers.Add(positions[p]);
                }
            }

            return attackers;
        }

        private BitBoard GetWhiteAttackers(BitBoard to, BitBoard occupied)
        {
            BitBoard attackers = new BitBoard(0);
            var positions = GetPositionInternal(Piece.WhitePawn.AsByte());
            for (var p = 0; p < positions.Length; p++)
            {
                var pattern = _moveProvider.GetWhitePawnAttackPattern(positions[p]);
                if (pattern.IsSet(to))
                {
                    attackers = attackers.Add(positions[p]);
                }
            }

            positions = GetPositionInternal(Piece.WhiteKnight.AsByte());
            for (var p = 0; p < positions.Length; p++)
            {
                var pattern = _moveProvider.GetWhiteKnightAttackPattern(positions[p]);
                if (pattern.IsSet(to))
                {
                    attackers = attackers.Add(positions[p]);
                }
            }

            positions = GetPositionInternal(Piece.WhiteBishop.AsByte());
            for (var p = 0; p < positions.Length; p++)
            {
                var pattern = positions[p].GetWhiteBishopAttackPattern(occupied);
                if (pattern.IsSet(to))
                {
                    attackers = attackers.Add(positions[p]);
                }
            }

            positions = GetPositionInternal(Piece.WhiteRook.AsByte());
            for (var p = 0; p < positions.Length; p++)
            {
                var pattern = positions[p].GetWhiteRookAttackPattern(occupied);
                if (pattern.IsSet(to))
                {
                    attackers = attackers.Add(positions[p]);
                }
            }

            positions = GetPositionInternal(Piece.WhiteQueen.AsByte());
            for (var p = 0; p < positions.Length; p++)
            {
                var pattern = positions[p].GetWhiteQueenAttackPattern(occupied);
                if (pattern.IsSet(to))
                {
                    attackers = attackers.Add(positions[p]);
                }
            }

            positions = GetPositionInternal(Piece.WhiteKing.AsByte());
            for (var p = 0; p < positions.Length; p++)
            {
                var pattern = _moveProvider.GetWhiteKingAttackPattern(positions[p]);
                if (pattern.IsSet(to))
                {
                    attackers = attackers.Add(positions[p]);
                }
            }

            return attackers;
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

            _isWhiteCastled = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DoBlackSmallCastle()
        {
            _pieces[Squares.G8.AsByte()] = Piece.BlackKing;
            _pieces[Squares.F8.AsByte()] = Piece.BlackRook;

            _hash.Update(Squares.H8.AsByte(), Squares.F8.AsByte(), Piece.BlackRook.AsByte());
            _hash.Update(Squares.E8.AsByte(), Squares.G8.AsByte(), Piece.BlackKing.AsByte());

            BlackSmallCastle();

            _isBlackCastled = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DoBlackBigCastle()
        {
            _pieces[Squares.C8.AsByte()] = Piece.BlackKing;
            _pieces[Squares.D8.AsByte()] = Piece.BlackRook;

            _hash.Update(Squares.A8.AsByte(), Squares.D8.AsByte(), Piece.BlackRook.AsByte());
            _hash.Update(Squares.E8.AsByte(), Squares.C8.AsByte(), Piece.BlackKing.AsByte());

            BlackBigCastle();

            _isBlackCastled = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DoWhiteBigCastle()
        {
            _pieces[Squares.C1.AsByte()] = Piece.WhiteKing;
            _pieces[Squares.D1.AsByte()] = Piece.WhiteRook;

            _hash.Update(Squares.A1.AsByte(), Squares.D1.AsByte(), Piece.WhiteRook.AsByte());
            _hash.Update(Squares.E1.AsByte(), Squares.C1.AsByte(), Piece.WhiteKing.AsByte());

            WhiteBigCastle();

            _isWhiteCastled = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UndoWhiteSmallCastle()
        {
            _pieces[Squares.E1.AsByte()] = Piece.WhiteKing;
            _pieces[Squares.H1.AsByte()] = Piece.WhiteRook;

            _hash.Update(Squares.F1.AsByte(), Squares.H1.AsByte(), Piece.WhiteRook.AsByte());
            _hash.Update(Squares.G1.AsByte(), Squares.E1.AsByte(), Piece.WhiteKing.AsByte());

            WhiteSmallCastle();

            _isWhiteCastled = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UndoBlackSmallCastle()
        {
            _pieces[Squares.E8.AsByte()] = Piece.BlackKing;
            _pieces[Squares.H8.AsByte()] = Piece.BlackRook;

            _hash.Update(Squares.F8.AsByte(), Squares.H8.AsByte(), Piece.BlackRook.AsByte());
            _hash.Update(Squares.G8.AsByte(), Squares.E8.AsByte(), Piece.BlackKing.AsByte());

            BlackSmallCastle();

            _isBlackCastled = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UndoWhiteBigCastle()
        {
            _pieces[Squares.E1.AsByte()] = Piece.WhiteKing;
            _pieces[Squares.A1.AsByte()] = Piece.WhiteRook;

            _hash.Update(Squares.D1.AsByte(), Squares.A1.AsByte(), Piece.WhiteRook.AsByte());
            _hash.Update(Squares.C1.AsByte(), Squares.E1.AsByte(), Piece.WhiteKing.AsByte());

            WhiteBigCastle();

            _isWhiteCastled = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UndoBlackBigCastle()
        {
            _pieces[Squares.E8.AsByte()] = Piece.BlackKing;
            _pieces[Squares.A8.AsByte()] = Piece.BlackRook;

            _hash.Update(Squares.D8.AsByte(), Squares.A8.AsByte(), Piece.BlackRook.AsByte());
            _hash.Update(Squares.C8.AsByte(), Squares.E8.AsByte(), Piece.BlackKing.AsByte());

            BlackBigCastle();

            _isBlackCastled = false;
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

            var to = Squares.E8.AsByte();

            return CanDoBlackCastle(to);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanDoWhiteSmallCastle()
        {
            if (!_moveHistory.CanDoWhiteSmallCastle() || !_boards[Piece.WhiteRook.AsByte()].IsSet(BitBoards.H1) ||
                !_empty.IsSet(_whiteSmallCastleCondition)) return false;

            var to = Squares.E1.AsByte();

            return CanDoWhiteCastle(to);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanDoBlackBigCastle()
        {
            if (!_moveHistory.CanDoBlackBigCastle() || !_boards[Piece.BlackRook.AsByte()].IsSet(BitBoards.A8) ||
                !_empty.IsSet(_blackBigCastleCondition)) return false;

            var to = Squares.E8.AsByte();

            return CanDoBlackCastle(to);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanDoWhiteBigCastle()
        {
            if (!_moveHistory.CanDoWhiteBigCastle() || !_boards[Piece.WhiteRook.AsByte()].IsSet(BitBoards.A1) ||
                !_empty.IsSet(_whiteBigCastleCondition)) return false;

            var to = Squares.E1.AsByte();

            return CanDoWhiteCastle(to);

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanDoWhiteCastle(byte to)
        {
            return !(_moveProvider.IsUnderAttack(this, Piece.BlackBishop.AsByte(), to) ||
                     _moveProvider.IsUnderAttack(this, Piece.BlackKnight.AsByte(), to) ||
                     _moveProvider.IsUnderAttack(this, Piece.BlackQueen.AsByte(), to) ||
                     _moveProvider.IsUnderAttack(this, Piece.BlackRook.AsByte(), to) ||
                     _moveProvider.IsUnderAttack(this, Piece.BlackPawn.AsByte(), to));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanDoBlackCastle(byte to)
        {
            return !(_moveProvider.IsUnderAttack(this, Piece.WhiteBishop.AsByte(), to) ||
                     _moveProvider.IsUnderAttack(this, Piece.WhiteKnight.AsByte(), to) ||
                     _moveProvider.IsUnderAttack(this, Piece.WhiteQueen.AsByte(), to) ||
                     _moveProvider.IsUnderAttack(this, Piece.WhiteRook.AsByte(), to) ||
                     _moveProvider.IsUnderAttack(this, Piece.WhitePawn.AsByte(), to));
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
                '\u265F', '\u265E', '\u265D','\u265C', '\u265B', '\u265A',' '
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
