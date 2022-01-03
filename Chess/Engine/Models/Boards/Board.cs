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
        private BitBoard[] _boards;
        private int[] _pieceCount;
        private int[] _values;
        private readonly bool[] _overBoard;
        private readonly Piece[] _pieces;
        private readonly IMoveProvider _moveProvider;
        private readonly IMoveHistoryService _moveHistory;

        public Board()
        {
            _pieces = new Piece[64];
            _overBoard = new bool[64];

            SetPieces();

            SetBoards();

            SetValues();

            SetCastles();

            _moveProvider = ServiceLocator.Current.GetInstance<IMoveProvider>();
            _moveHistory = ServiceLocator.Current.GetInstance<IMoveHistoryService>();

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
        public int GetValue()
        {
            var value = 0;
            for (int i = 0; i < 6; i++)
            {
                value = value + _pieceCount[i] * _values[i];
            }

            for (int i = 6; i < 12; i++)
            {
                value = value - _pieceCount[i] * _values[i];
            }

            value = GetValue(Piece.WhitePawn, value,1,  3);
            value = GetValue(Piece.BlackPawn, value,-1,  -3);

            //var blockedPawns = GetWhiteBlockedPawns() - GetBlackBlockedPawns();
            //value -= blockedPawns;

            if (_pieceCount[Piece.WhiteBishop.AsByte()] < 2)
            {
                value -= 2;
            }
            if (_pieceCount[Piece.BlackBishop.AsByte()] < 2)
            {
                value += 2;
            }

            value = EvaluateCastles(value);

            return value;
        }

        private int EvaluateCastles(int value)
        {
            if (_isWhiteCastled)
            {
                value += 2;
            }
            else
            {
                if (!_moveHistory.CanDoWhiteSmallCastle())
                {
                    value--;
                }

                if (!_moveHistory.CanDoWhiteBigCastle())
                {
                    value--;
                }
            }

            if (_isBlackCastled)
            {
                value -= 2;
            }
            else
            {
                if (!_moveHistory.CanDoBlackSmallCastle())
                {
                    value++;
                }

                if (!_moveHistory.CanDoBlackBigCastle())
                {
                    value++;
                }
            }

            return value;
        }

        private int GetBlackBlockedPawns()
        {
            int count = 0;
            var pawns = new[] { -1, -1, -1, -1, -1, -1, -1, -1 };
            var coordinates = _boards[Piece.BlackPawn.AsByte()].Coordinates();
            for (var index = 0; index < _pieceCount[Piece.BlackPawn.AsByte()]; index++)
            {
                var coordinate = coordinates[index];
                var x = coordinate % 8;
                if (pawns[x] < 0)
                {
                    pawns[x] = coordinate;
                    if (_whites.IsSet((coordinate - 8).AsBitBoard()))
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

                    if (_whites.IsSet((coordinate - 8).AsBitBoard()))
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        private int GetWhiteBlockedPawns()
        {
            int count = 0;
            var pawns = new[] { -1, -1, -1, -1, -1, -1, -1, -1 };
            var coordinates = _boards[Piece.WhitePawn.AsByte()].Coordinates();
            for (var index = 0; index < _pieceCount[Piece.WhitePawn.AsByte()]; index++)
            {
                var coordinate = coordinates[index];
                var x = coordinate % 8;
                if (pawns[x] < 0)
                {
                    pawns[x] = coordinate;
                    if (_blacks.IsSet((coordinate + 8).AsBitBoard()))
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

                    if (_blacks.IsSet((coordinate + 8).AsBitBoard()))
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        private int GetValue(Piece piece, int value,int doubledPawn, int isolatedPawn)
        {
            var pawns = new int[8];
            var coordinates = _boards[piece.AsByte()].Coordinates();
            for (var index = 0; index < _pieceCount[piece.AsByte()]; index++)
            {
                pawns[coordinates[index] % 8]++;
            }

            if (pawns[0] > 0)
            {
                if (pawns[0] > 1)
                {
                    value = value - doubledPawn;
                }

                if (pawns[1] == 0)
                {
                    value = value - doubledPawn;
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
                    value = value - doubledPawn;
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
            //if (piece == Piece.BlackKing || piece == Piece.WhiteKing)
            //{

            //}
            _hash.Update(square.AsByte(), piece.AsByte());
           // _boards[piece.AsByte()]=_boards[piece.AsByte()].Off(square.AsByte());
            _pieceCount[piece.AsByte()]--;

            Remove(piece,square.AsBitBoard());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(Piece piece, Square square)
        {
            _hash.Update(square.AsByte(), piece.AsByte());
            //_boards[piece.AsByte()] = _boards[piece.AsByte()].Set(square.AsByte());
            _pieceCount[piece.AsByte()]++;
            _pieces[square.AsByte()] = piece;

            Add(piece,square.AsBitBoard());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Move(Piece piece, Square from, Square to)
        {
            _hash.Update(from.AsByte(), to.AsByte(), piece.AsByte());
           // _boards[piece.AsByte()] = _boards[piece.AsByte()].Off(from.AsByte()).Set(to.AsByte());
            _pieces[to.AsByte()] = piece;

            Move(piece, from.AsBitBoard()|to.AsBitBoard());
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
            return _boards[index].Coordinates();
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
            var ints = _boards[piece].Coordinates();
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

        #endregion

        #region Castle

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DoWhiteSmallCastle()
        {
            _pieces[Squares.G1.AsByte()] = Piece.WhiteKing;
            _pieces[Squares.F1.AsByte()] = Piece.WhiteRook;
           //_boards[Piece.WhiteKing.AsByte()]=_boards[Piece.WhiteKing.AsByte()].Replace(Squares.E1.AsByte(), Squares.G1.AsByte());
           // _boards[Piece.WhiteRook.AsByte()]=_boards[Piece.WhiteRook.AsByte()].Replace(Squares.H1.AsByte(), Squares.F1.AsByte());

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

            //_boards[Piece.BlackKing.AsByte()]=_boards[Piece.BlackKing.AsByte()].Replace(Squares.E8.AsByte(), Squares.G8.AsByte());
            //_boards[Piece.BlackRook.AsByte()]=_boards[Piece.BlackRook.AsByte()].Replace(Squares.H8.AsByte(), Squares.F8.AsByte());

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

            //_boards[Piece.BlackKing.AsByte()] = _boards[Piece.BlackKing.AsByte()].Replace(Squares.E8.AsByte(), Squares.C8.AsByte());
           //_boards[Piece.BlackRook.AsByte()]=_boards[Piece.BlackRook.AsByte()].Replace(Squares.A8.AsByte(), Squares.D8.AsByte());

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

           // _boards[Piece.WhiteKing.AsByte()]=_boards[Piece.WhiteKing.AsByte()].Replace(Squares.E1.AsByte(), Squares.C1.AsByte());
           // _boards[Piece.WhiteRook.AsByte()]=_boards[Piece.WhiteRook.AsByte()].Replace(Squares.A1.AsByte(), Squares.D1.AsByte());

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

           // _boards[Piece.WhiteKing.AsByte()] = _boards[Piece.WhiteKing.AsByte()].Replace(Squares.G1.AsByte(), Squares.E1.AsByte());
          //  _boards[Piece.WhiteRook.AsByte()] = _boards[Piece.WhiteRook.AsByte()].Replace(Squares.F1.AsByte(), Squares.H1.AsByte());

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

          //  _boards[Piece.BlackKing.AsByte()] =  _boards[Piece.BlackKing.AsByte()].Replace(Squares.G8.AsByte(), Squares.E8.AsByte());
          //  _boards[Piece.BlackRook.AsByte()] = _boards[Piece.BlackRook.AsByte()].Replace(Squares.F8.AsByte(), Squares.H8.AsByte());

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

           // _boards[Piece.WhiteKing.AsByte()]=_boards[Piece.WhiteKing.AsByte()].Replace(Squares.C1.AsByte(), Squares.E1.AsByte());
          //  _boards[Piece.WhiteRook.AsByte()]=_boards[Piece.WhiteRook.AsByte()].Replace(Squares.D1.AsByte(), Squares.A1.AsByte());

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

         //   _boards[Piece.BlackKing.AsByte()]=_boards[Piece.BlackKing.AsByte()].Replace(Squares.C8.AsByte(), Squares.E8.AsByte());
          //  _boards[Piece.BlackRook.AsByte()]=_boards[Piece.BlackRook.AsByte()].Replace(Squares.D8.AsByte(), Squares.A8.AsByte());

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
            if (!_moveHistory.CanDoBlackSmallCastle() || !_boards[Piece.BlackRook.AsByte()].IsSet(BitBoards.H8) || !_empty.IsSet(_blackSmallCastleCondition)) return false;

            var to = Squares.E8.AsByte();

            return CanDoBlackCastle(to);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanDoWhiteSmallCastle()
        {
            if (!_moveHistory.CanDoWhiteSmallCastle() || !_boards[Piece.WhiteRook.AsByte()].IsSet(BitBoards.H1) || !_empty.IsSet(_whiteSmallCastleCondition)) return false;

            var to = Squares.E1.AsByte();

            return CanDoWhiteCastle(to);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanDoBlackBigCastle()
        {
            if (!_moveHistory.CanDoBlackBigCastle() || !_boards[Piece.BlackRook.AsByte()].IsSet(BitBoards.A8) || !_empty.IsSet(_blackBigCastleCondition)) return false;

            var to = Squares.E8.AsByte();

            return CanDoBlackCastle(to);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanDoWhiteBigCastle()
        {
            if (!_moveHistory.CanDoWhiteBigCastle() || !_boards[Piece.WhiteRook.AsByte()].IsSet(BitBoards.A1) || !_empty.IsSet(_whiteBigCastleCondition)) return false;

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

            _boards[Piece.BlackPawn.AsByte()] = _boards[Piece.BlackPawn.AsByte()].Set(Enumerable.Range(48, 8).ToArray());
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
                var coordinates = _boards[piece.AsByte()].Coordinates();
                for (var index = 0; index < _pieceCount[piece.AsByte()]; index++)
                {
                    _pieces[coordinates[index]] = piece;
                }
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

        private void SetValues()
        {
            _values = new int[12];

            _values[Piece.WhitePawn.AsByte()] = 8;
            _values[Piece.BlackPawn.AsByte()] = 8;
            _values[Piece.WhiteKnight.AsByte()] = 25;
            _values[Piece.BlackKnight.AsByte()] = 25;
            _values[Piece.WhiteBishop.AsByte()] = 25;
            _values[Piece.BlackBishop.AsByte()] = 25;
            _values[Piece.WhiteKing.AsByte()] = 0;
            _values[Piece.BlackKing.AsByte()] = 0;
            _values[Piece.WhiteRook.AsByte()] = 39;
            _values[Piece.BlackRook.AsByte()] = 39;
            _values[Piece.WhiteQueen.AsByte()] = 79;
            _values[Piece.BlackQueen.AsByte()] = 79;
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
            StringBuilder builder = new StringBuilder();
            for (int y = 7; y >= 0; y--)
            {
                for (int x = 0; x< 8; x++)
                {
                    var i = y * 8 + x;
                    string v = " ";
                    if (!_empty.IsSet(i.AsBitBoard()))
                    {
                        v = _pieces[i].AsString();
                    }
                    builder.Append($"[ {v} ]");
                }

                builder.AppendLine();
            }
            return builder.ToString();
        }
    }
}
