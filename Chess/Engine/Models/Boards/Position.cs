using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using CommonServiceLocator;
using Engine.DataStructures;
using Engine.DataStructures.Moves;
using Engine.Interfaces;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Sorting.Sorters;

namespace Engine.Models.Boards
{
    public class Position : IPosition
    {
        private Turn _turn;
        private Phase _phase;
        private readonly ArrayStack<Piece?> _figureHistory;

        private readonly byte[][] _white;
        private readonly byte[][] _black;

        private readonly AttackList _attacks;
        private readonly AttackList _attacksTemp;

        private readonly MoveList _moves;
        private readonly MoveList _movesTemp;

        private readonly IBoard _board;
        private readonly IMoveProvider _moveProvider;
        private readonly IMoveHistoryService _moveHistoryService;
        private readonly ICheckService _checkService;

        public Position()
        {
            _turn = Turn.White;
            _white = new byte[3][];
            _black = new byte[3][];
            _white[0] = new[]
            {
                Piece.WhitePawn.AsByte(), Piece.WhiteKnight.AsByte(), Piece.WhiteBishop.AsByte(),
                Piece.WhiteRook.AsByte(), Piece.WhiteQueen.AsByte(),
                Piece.WhiteKing.AsByte()
            };
            _white[1] = new[]
            {
                Piece.WhitePawn.AsByte(), Piece.WhiteKnight.AsByte(), Piece.WhiteBishop.AsByte(),
                Piece.WhiteRook.AsByte(), Piece.WhiteQueen.AsByte(),
                Piece.WhiteKing.AsByte()
            };
            _white[2] = new[]
            {
                Piece.WhitePawn.AsByte(), Piece.WhiteKnight.AsByte(), Piece.WhiteBishop.AsByte(),
                Piece.WhiteRook.AsByte(), Piece.WhiteQueen.AsByte(),
                Piece.WhiteKing.AsByte()
            };
            _black[0] = new[]
            {
                Piece.BlackPawn.AsByte(), Piece.BlackKnight.AsByte(), Piece.BlackBishop.AsByte(),
                Piece.BlackRook.AsByte(), Piece.BlackQueen.AsByte(),
                Piece.BlackKing.AsByte()
            };
            _black[1] = new[]
                {
                    Piece.BlackPawn.AsByte(), Piece.BlackKnight.AsByte(), Piece.BlackBishop.AsByte(),
                    Piece.BlackRook.AsByte(), Piece.BlackQueen.AsByte(),
                    Piece.BlackKing.AsByte()};
            _black[2] = new[]
                {
                    Piece.BlackPawn.AsByte(), Piece.BlackKnight.AsByte(), Piece.BlackBishop.AsByte(),
                    Piece.BlackRook.AsByte(), Piece.BlackQueen.AsByte(),
                    Piece.BlackKing.AsByte()};

            //_white[0] = new[]
            //{
            //    Piece.WhitePawn, Piece.WhiteKnight, Piece.WhiteBishop, Piece.WhiteRook, Piece.WhiteQueen,
            //    Piece.WhiteKing
            //};
            //_white[1] = new[]
            //{
            //    Piece.WhiteKnight, Piece.WhiteBishop,Piece.WhitePawn,Piece.WhiteQueen, Piece.WhiteRook,
            //    Piece.WhiteKing
            //};
            //_white[2] = new[]
            //{
            //    Piece.WhitePawn,
            //    Piece.WhiteKing, Piece.WhiteRook, Piece.WhiteQueen, Piece.WhiteKnight, Piece.WhiteBishop
            //};
            //_black[0] = new[]
            //    {Piece.BlackPawn, Piece.BlackKnight, Piece.BlackBishop, Piece.BlackRook, Piece.BlackQueen, Piece.BlackKing};
            //_black[1] = new[]
            //    {Piece.BlackKnight, Piece.BlackBishop,Piece.BlackPawn,Piece.BlackQueen,  Piece.BlackRook,  Piece.BlackKing};
            //_black[2] = new[]
            //    {Piece.BlackPawn, Piece.BlackKing, Piece.BlackRook, Piece.BlackQueen, Piece.BlackKnight, Piece.BlackBishop};

            _attacks = new AttackList();
            _attacksTemp = new AttackList();
            _moves = new MoveList();
            _movesTemp = new MoveList();

            _board = new Board();
            _figureHistory = new ArrayStack<Piece?>();
            _moveProvider = ServiceLocator.Current.GetInstance<IMoveProvider>();
            _moveHistoryService = ServiceLocator.Current.GetInstance<IMoveHistoryService>();
            _checkService = ServiceLocator.Current.GetInstance<ICheckService>();
        }
        #region Implementation of IPosition

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetPiece(Square cell, out Piece? piece)
        {
            return _board.GetPiece(cell, out piece);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong GetKey()
        {
            return _board.GetKey();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short GetValue()
        {
            if (_turn == Turn.White)
                return _board.GetValue();
            return (short) -_board.GetValue();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetStaticValue()
        {
            if (_turn == Turn.White)
                return _board.GetStaticValue();
            return (short)-_board.GetStaticValue();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetOpponentMaxValue()
        {
            if (_turn == Turn.White)
                return _board.GetBlackMaxValue();
            return _board.GetWhiteMaxValue();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Turn GetTurn()
        {
            return _turn;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<IAttack> GetAllAttacks(Square cell, Piece piece)
        {
            var attacks = _moveProvider.GetAttacks(piece, cell);
            foreach (var attack in attacks.Where(IsLigal))
            {
                yield return attack;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<IMove> GetAllMoves(Square cell, Piece piece)
        {
            var moves = _moveProvider.GetMoves(piece, cell);
            foreach (var move in moves.Where(IsLigal))
            {
                yield return move;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IMove[] GetAllAttacks(IMoveSorter sorter)
        {
            var pieces = _turn == Turn.White ? _white[(byte)_phase] : _black[(byte)_phase];
            var squares = GetSquares(pieces);
            return sorter.Order(PossibleAttacks(squares, pieces));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IMove[] GetAllMoves(IMoveSorter sorter, IMove pvMove = null)
        {
            var pieces = _turn == Turn.White ? _white[(byte)_phase] : _black[(byte)_phase];
            var squares = GetSquares(pieces);
            return sorter.Order(PossibleAttacks(squares, pieces), PossibleMoves(squares, pieces), pvMove);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private MoveList PossibleMoves(Square[][] squares, byte[] pieces)
        {
            _moves.Clear();
            _movesTemp.Clear();
            for (var index = 0; index < pieces.Length; index++)
            {
                var p = pieces[index];
                Square[] from = squares[p % 6];

                for (var f = 0; f < from.Length; f++)
                {
                    _moveProvider.GetMoves(p, @from[f], _movesTemp);
                    for (var i = 0; i < _movesTemp.Count; i++)
                    {
                        if (IsLigal(_movesTemp[i]))
                        {
                            _moves.Add(_movesTemp[i]);
                        }
                    }
                    _movesTemp.Clear();
                }
            }

            return _moves;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private AttackList PossibleAttacks(Square[][] squares, byte[] pieces)
        {
            _attacks.Clear();
            _attacksTemp.Clear();
            for (var index = 0; index < pieces.Length; index++)
            {
                var p = pieces[index];

                var square = squares[p % 6];
                for (var f = 0; f < square.Length; f++)
                {
                    _moveProvider.GetAttacks(p, square[f],_attacksTemp);
                    for (var i = 0; i < _attacksTemp.Count; i++)
                    {
                        if (IsLigal(_attacksTemp[i]))
                        {
                            _attacks.Add(_attacksTemp[i]);
                        }
                    }
                    _attacksTemp.Clear();
                }
            }

            return _attacks;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetPieceValue(Square square)
        {
            return _board.GetPiece(square).AsValue();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IBoard GetBoard()
        {
            return _board;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<IMove> GetHistory()
        {
            return _moveHistoryService.GetHistory();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsNotLegal(IMove move)
        {
            return _turn != Turn.White
                ? _moveProvider.AnyBlackCheck() || move.IsCastle() &&
                  _moveProvider.IsWhiteUnderAttack(move.To == Squares.C1 ? Squares.D1 : Squares.F1)
                : _moveProvider.AnyWhiteCheck() || move.IsCastle() &&
                  _moveProvider.IsBlackUnderAttack(move.To == Squares.C8 ? Squares.D8 : Squares.F8);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Phase GetPhase()
        {
            return _phase;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanWhitePromote()
        {
            return _board.CanWhitePromote();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanBlackPromote()
        {
            return _board.CanBlackPromote();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Square[][] GetSquares(byte[] pieces)
        {
            var squares = new Square[pieces.Length][];
            for (var i = 0; i < squares.Length; i++)
            {
                var p = pieces[i];
                var from = _board.GetPiecePositions(p);
                squares[p % squares.Length] = from;
            }
            return squares;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsCheck()
        {
            //return _turn != Turn.White ? _checkService.IsBlackCheck(_board.GetKey(), _board) : _checkService.IsWhiteCheck(_board.GetKey(), _board);
            //return _turn != Turn.White ? _moveProvider.IsCheckToWhite(_board) : _moveProvider.IsCheckToBlack(_board);

            return _turn != Turn.White ? _moveProvider.AnyBlackCheck() : _moveProvider.AnyWhiteCheck();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Make(IMove move)
        {
            IMove previousMove = _moveHistoryService.GetLastMove();
            if (previousMove != null && previousMove.Type == MoveType.Over && move.Type != MoveType.EatOver)
            {
                _board.SetOver(previousMove.To, false);
            }

            _moveHistoryService.Add(move);

            move.Make(_board, _figureHistory);

            move.SetMoveResult(IsCheck());

            //var set = _board.GetBoardSet();

            //_moveHistoryService.Add(set);

            _phase = _board.UpdatePhase();

            _moveHistoryService.Add(_board.GetKey());

            SwapTurn();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnMake()
        {
            _moveHistoryService.Remove(_board.GetKey());
            IMove move = _moveHistoryService.Remove();

            IMove previousMove = _moveHistoryService.GetLastMove();
            if (previousMove != null && previousMove.Type == MoveType.Over)
            {
                _board.SetOver(previousMove.To, true);
            }

            move.UnMake(_board, _figureHistory);

            move.SetMoveResult(false);

            _phase = _board.UpdatePhase();

            SwapTurn();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Do(IMove move)
        {
            move.Make(_board, _figureHistory);

            SwapTurn();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnDo(IMove move)
        {
            move.UnMake(_board, _figureHistory);

            SwapTurn();
        }

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsLigal(IMove move)
        {
            move.Make(_board, _figureHistory);

            SwapTurn();

            bool isLegal = !IsNotLegal(move);

            move.UnMake(_board, _figureHistory);

            SwapTurn();

            return isLegal;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SwapTurn()
        {
            _turn = _turn == Turn.White ? Turn.Black : Turn.White;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"Turn = {_turn}, Key = {GetKey()}, Value = {GetValue()}");
            builder.AppendLine(_board.ToString());
            return builder.ToString();
        }
    }
}