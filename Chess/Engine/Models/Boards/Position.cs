using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using CommonServiceLocator;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Sorting.Sorters;

namespace Engine.Models.Boards
{
    public class Position : IPosition
    {
        private Turn _turn;
        private readonly ArrayStack<Piece?> _figureHistory;

        private readonly Piece[] _white;
        private readonly Piece[] _black;

        private readonly IBoard _board;
        private readonly IMoveProvider _moveProvider;
        private readonly IMoveHistoryService _moveHistoryService;
        private readonly ICheckService _checkService;

        public Position()
        {
            _turn = Turn.White;
            _white = new[]
                {Piece.WhitePawn, Piece.WhiteKnight, Piece.WhiteBishop, Piece.WhiteRook, Piece.WhiteQueen, Piece.WhiteKing};
            _black = new[]
                {Piece.BlackPawn, Piece.BlackKnight, Piece.BlackBishop, Piece.BlackRook, Piece.BlackQueen, Piece.BlackKing};
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
        public int GetValue()
        {
            if (_turn == Turn.White)
                return _board.GetValue();
            return -_board.GetValue();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Turn GetTurn()
        {
            return _turn;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<IAttack> GetAllAttacks(Square cell, Piece piece)
        {
            return _moveProvider.GetAttacks(piece, cell,_board);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<IMove> GetAllMoves(Square cell, Piece piece)
        {
            return _moveProvider.GetMoves(piece, cell, _board);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IMoveCollection GetAllAttacks(IMoveSorter sorter)
        {
            if (_turn == Turn.White)
            {
                var squares = GetSquares(_white);
                return sorter.Order(PossibleAttacks(squares, _white));
            }
            else
            {
                var squares = GetSquares(_black);
                return sorter.Order(PossibleAttacks(squares, _black));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IMoveCollection GetAllMoves(IMoveSorter sorter, IMove pvMove = null, IMove cutMove = null)
        {
            if (_turn == Turn.White)
            {
                var squares = GetSquares(_white);
                return sorter.Order(PossibleAttacks(squares, _white), PossibleMoves(squares, _white), pvMove, cutMove);
            }
            else
            {
                var squares = GetSquares(_black);
                return sorter.Order(PossibleAttacks(squares, _black), PossibleMoves(squares, _black), pvMove, cutMove);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IEnumerable<IMove> PossibleMoves(Square[][] squares, Piece[] pieces)
        {
            var lastMove = _moveHistoryService.GetLastMove();

            if (lastMove?.IsCheck() == true)
            {
                for (var index = 0; index < pieces.Length; index++)
                {
                    var p = pieces[index];
                    Square[] from = squares[p.AsByte()%6];

                    for (var f = 0; f < from.Length; f++)
                    {
                        foreach (var move in _moveProvider.GetMoves(p, from[f], _board))
                        {
                            if (!move.IsCastle())
                            {
                                yield return move;
                            }
                        }
                    }
                }
            }
            else
            {
                for (var index = 0; index < pieces.Length; index++)
                {
                    var p = pieces[index];
                    Square[] from = squares[p.AsByte()%6];

                    for (var f = 0; f < from.Length; f++)
                    {
                        foreach (var move in _moveProvider.GetMoves(p, from[f], _board))
                        {
                            yield return move;
                        }
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IEnumerable<IAttack> PossibleAttacks(Square[][] squares, Piece[] pieces)
        {
            for (var index = 0; index < pieces.Length; index++)
            {
                var p = pieces[index];

                var square = squares[p.AsByte()%6];
                for (var f = 0; f < square.Length; f++)
                {
                    foreach (var attack in _moveProvider.GetAttacks(p, square[f], _board))
                    {
                        yield return attack;
                    }
                }
            }
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
                ? _moveProvider.AnyBlackCheck(_board) || move.IsCastle() &&
                  _moveProvider.IsWhiteUnderAttack(_board, move.To == Squares.C1 ? Squares.D1 : Squares.F1)
                : _moveProvider.AnyWhiteCheck(_board) || move.IsCastle() &&
                  _moveProvider.IsBlackUnderAttack(_board, move.To == Squares.C8 ? Squares.D8 : Squares.F8);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Square[][] GetSquares(Piece[] pieces)
        {
            var squares = new Square[pieces.Length][];
            for (var i = 0; i < squares.Length; i++)
            {
                var p = pieces[i];
                var from = _board.GetPiecePositions(p.AsByte());
                squares[p.AsByte()% squares.Length] = from;
            }
            return squares;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsCheck()
        {
            //return _turn != Turn.White ? _checkService.IsBlackCheck(_board.GetKey(), _board) : _checkService.IsWhiteCheck(_board.GetKey(), _board);
            return _turn != Turn.White ? _moveProvider.AnyBlackCheck(_board) : _moveProvider.AnyWhiteCheck(_board);

            //return _turn != Turn.White ? _moveProvider.IsCheckToWhite(_board) : _moveProvider.IsCheckToBlack(_board);
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

            _board.UpdatePhase();

            //var set = _board.GetBoardSet();

            //_moveHistoryService.Add(set);

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

            _board.UpdatePhase();

            SwapTurn();
        }

        #endregion

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