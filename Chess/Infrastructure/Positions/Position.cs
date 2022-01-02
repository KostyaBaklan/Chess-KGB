using System.Collections.Generic;
using System.Text;
using CommonServiceLocator;
using Infrastructure.Helpers;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.Moves;
using Infrastructure.Interfaces.Position;
using Infrastructure.Models.Enums;
using Infrastructure.Moves;

namespace Infrastructure.Positions
{
    public class Position : IPosition
    {
        private readonly IMoveProvider _moveProvider;
        private readonly Stack<FigureKind?> _figureHistory;
        private readonly IBoard _board;
        private readonly ZobristHash _hash;
        private readonly IMoveHistoryService _moveHistoryService;
        private readonly ICheckService _checkService;
        private readonly IMoveFormatter _moveFormatter;
        private readonly IOpennigService _opennigService;

        public Position()
        {
            Turn = Turn.White;
            _figureHistory = new Stack<FigureKind?>();

             _board = new Board();
            _moveProvider = ServiceLocator.Current.GetInstance<IMoveProvider>();
            var map = ServiceLocator.Current.GetInstance<IFigureMap>();
            _moveHistoryService = ServiceLocator.Current.GetInstance<IMoveHistoryService>();
            _checkService = ServiceLocator.Current.GetInstance<ICheckService>();
            _moveFormatter = ServiceLocator.Current.GetInstance<IMoveFormatter>();
            _opennigService = ServiceLocator.Current.GetInstance<IOpennigService>();
            _hash = map.Hash;
        }

        public ulong Key => _hash.Key;
        public IBoard Board => _board;

        #region Implementation of IPosition

        public IEnumerable<IMove> GetAllMoves(Coordinate cell)
        {
            FigureKind? figure = _board[cell];

            return _moveProvider.GetMoves(cell, figure.Value, _board);
        }

        public IEnumerable<IMove> GetAllMoves(IMoveSorter sorter, IMove pvMove = null, IMove cutMove = null)
        {
            //if (_moveHistoryService.Ply < 6)
            //{
            //    var moves = _opennigService.GetMoves();
            //    if (moves.Any()) return moves.Where(m => m.IsLegal(_board));

            //}

            return sorter.Order(Turn == Turn.White ? PossibleWhiteMoves() : PossibleBlackMoves(), pvMove, cutMove);
        }

        private IEnumerable<IMove> PossibleWhiteMoves()
        {
            foreach (var attack in GetWhiteAttacks())
            {
                yield return attack;
            }

            var lastMove = _moveHistoryService.GetLastMove();
            var isCheck = lastMove != null && lastMove.Result == MoveResult.Check;

            if (isCheck)
            {
                foreach (var cell in _board.GetWhiteCells())
                {
                    foreach (var move in GetAllMoves(cell))
                    {
                        if (!move.IsCastle())
                        {
                            yield return move;
                        }
                    }
                }
            }
            else
            {
                foreach (var cell in _board.GetWhiteCells())
                {
                    foreach (var move in GetAllMoves(cell))
                    {
                        yield return move;
                    }
                }
            }
        }

        private IEnumerable<IMove> PossibleBlackMoves()
        {
            foreach (var attack in GetBlackAttacks())
            {
                yield return attack;
            }

            var lastMove = _moveHistoryService.GetLastMove();
            bool isCheck = lastMove != null && lastMove.Result == MoveResult.Check;

            if (isCheck)
            {
                foreach (var cell in _board.GetBlackCells())
                {
                    foreach (var move in GetAllMoves(cell))
                    {
                        if (!move.IsCastle())
                        {
                            yield return move;
                        }
                    }
                }
            }
            else
            {
                foreach (var cell in _board.GetBlackCells())
                {
                    foreach (var move in GetAllMoves(cell))
                    {
                        yield return move;
                    }
                }
            }
        }

        public IEnumerable<IMove> GetAllAttacks()
        {
            if (Turn == Turn.White)
                return GetWhiteAttacks();
            return GetBlackAttacks();
        }

        public string GetHistory()
        {
            StringBuilder builder = new StringBuilder();
            IEnumerable<IMove> moves = _moveHistoryService.GetHistory();
            foreach (var move in moves)
            {
                builder.Append($"[{_moveFormatter.Format(move)}]");
            }
            return builder.ToString();
        }

        public byte GetFigureValue(byte key)
        {
            return _board.GetValue(key);
        }

        public void MakeTest(IMove move)
        {
            var previousMove = _moveHistoryService.GetLastMove();
            if (previousMove != null && previousMove.Operation == MoveOperation.Over && move.Operation != MoveOperation.EatOver)
            {
                _board.SetOver(previousMove.To.X, previousMove.To.Y, false);
            }

            _moveHistoryService.Add(move);

            _board[move.From] = null;
            var to = move.To;

            switch (move.Type)
            {
                case MoveType.Move:
                    switch (move.Operation)
                    {
                        case MoveOperation.PawnPromotionToQueen:
                            {
                                _board[to] =
                                    move.Figure == FigureKind.WhitePawn ? FigureKind.WhiteQueen : FigureKind.BlackQueen;
                            }
                            break;
                        case MoveOperation.PawnPromotionToRook:
                            {
                                _board[to] =
                                    move.Figure == FigureKind.WhitePawn ? FigureKind.WhiteRook : FigureKind.BlackRook;
                            }
                            break;
                        case MoveOperation.PawnPromotionToKnight:
                            {
                                _board[to] =
                                    move.Figure == FigureKind.WhitePawn ? FigureKind.WhiteKnight : FigureKind.BlackKnight;
                            }
                            break;
                        case MoveOperation.PawnPromotionToBishop:
                            {
                                _board[to] =
                                    move.Figure == FigureKind.WhitePawn ? FigureKind.WhiteBishop : FigureKind.BlackBishop;
                            }
                            break;
                        default:
                            _board[to] = move.Figure;
                            break;
                    }
                    break;
                case MoveType.Attack:
                    _figureHistory.Push(_board[to]);
                    switch (move.Operation)
                    {
                        case MoveOperation.EatByPawnPromotionToQueen:
                            {
                                _board[to] =
                                    move.Figure == FigureKind.WhitePawn ? FigureKind.WhiteQueen : FigureKind.BlackQueen;
                            }
                            break;
                        case MoveOperation.EatByPawnPromotionToRook:
                            {
                                _board[to] =
                                    move.Figure == FigureKind.WhitePawn ? FigureKind.WhiteRook : FigureKind.BlackRook;
                            }
                            break;
                        case MoveOperation.EatByPawnPromotionToKnight:
                            {
                                _board[to] =
                                    move.Figure == FigureKind.WhitePawn ? FigureKind.WhiteKnight : FigureKind.BlackKnight;
                            }
                            break;
                        case MoveOperation.EatByPawnPromotionToBishop:
                            {
                                _board[to] =
                                    move.Figure == FigureKind.WhitePawn ? FigureKind.WhiteBishop : FigureKind.BlackBishop;
                            }
                            break;
                        default:
                            _board[to] = move.Figure;
                            break;
                    }
                    break;
                case MoveType.SmallCastle:
                    if (move.Figure == FigureKind.WhiteKing)
                    {
                        _board.SmallWhiteCastle(true);
                    }
                    else
                    {
                        _board.SmallBlackCastle(true);
                    }
                    _board[to] = move.Figure;
                    break;
                case MoveType.BigCastle:
                    if (move.Figure == FigureKind.WhiteKing)
                    {
                        _board.BigWhiteCastle(true);
                    }
                    else
                    {
                        _board.BigBlackCastle(true);
                    }
                    _board[to] = move.Figure;
                    break;
                case MoveType.PawnOverMove:
                    if (move.Operation == MoveOperation.Over)
                    {
                        _board.SetOver(to.X, to.Y, true);
                    }
                    _board[to] = move.Figure;
                    break;
                case MoveType.PawnOverAttack:
                    _board[(byte)(to.X * 8 + move.From.Y)] = null;
                    _board.SetOver(to.X, move.From.Y, false);
                    _board[to] = move.Figure;
                    break;
            }

            move.Result = IsCheckTest() ? MoveResult.Check : MoveResult.Normal;

            Turn = Turn == Turn.White ? Turn.Black : Turn.White;
        }

        public bool IsCheckTest()
        {
            return Turn != Turn.White ? _moveProvider.AnyBlackCheck(_board) : _moveProvider.AnyWhiteCheck(_board);
        }

        public bool IsCheck()
        {
            return Turn != Turn.White ? _checkService.IsBlackCheck(_hash.Key,_board) : _checkService.IsWhiteCheck(_hash.Key, _board);
        }

        public int GetValue()
        {
            if (Turn == Turn.White)
                return _board.GetValue();
            return -_board.GetValue();
        }

        public void Make(IMove move)
        {
            var previousMove = _moveHistoryService.GetLastMove();
            if (previousMove != null && previousMove.Operation == MoveOperation.Over && move.Operation != MoveOperation.EatOver)
            {
                _board.SetOver(previousMove.To.X, previousMove.To.Y, false);
            }

            _moveHistoryService.Add(move);

            _board[move.From] = null;
            var to = move.To;

            switch (move.Type)
            {
                case MoveType.Move:
                    switch (move.Operation)
                    {
                        case MoveOperation.PawnPromotionToQueen:
                        {
                            _board[to] =
                                move.Figure == FigureKind.WhitePawn ? FigureKind.WhiteQueen : FigureKind.BlackQueen;
                            }
                            break;
                        case MoveOperation.PawnPromotionToRook:
                        {
                            _board[to] =
                                move.Figure == FigureKind.WhitePawn ? FigureKind.WhiteRook : FigureKind.BlackRook;
                            }
                            break;
                        case MoveOperation.PawnPromotionToKnight:
                        {
                            _board[to] =
                                move.Figure == FigureKind.WhitePawn ? FigureKind.WhiteKnight : FigureKind.BlackKnight;
                            }
                            break;
                        case MoveOperation.PawnPromotionToBishop:
                        {
                            _board[to] =
                                move.Figure == FigureKind.WhitePawn ? FigureKind.WhiteBishop : FigureKind.BlackBishop;
                            }
                            break;
                        default:
                            _board[to] = move.Figure;
                            break;
                    }
                    break;
                case MoveType.Attack:
                    _figureHistory.Push(_board[to]);
                    switch (move.Operation)
                    {
                        case MoveOperation.EatByPawnPromotionToQueen:
                        {
                            _board[to] =
                                move.Figure == FigureKind.WhitePawn ? FigureKind.WhiteQueen : FigureKind.BlackQueen;
                            }
                            break;
                        case MoveOperation.EatByPawnPromotionToRook:
                        {
                            _board[to] =
                                move.Figure == FigureKind.WhitePawn ? FigureKind.WhiteRook : FigureKind.BlackRook;
                            }
                            break;
                        case MoveOperation.EatByPawnPromotionToKnight:
                        {
                            _board[to] =
                                move.Figure == FigureKind.WhitePawn ? FigureKind.WhiteKnight : FigureKind.BlackKnight;
                            }
                            break;
                        case MoveOperation.EatByPawnPromotionToBishop:
                        {
                            _board[to] =
                                move.Figure == FigureKind.WhitePawn ? FigureKind.WhiteBishop : FigureKind.BlackBishop;
                            }
                            break;
                        default:
                            _board[to] = move.Figure;
                            break;
                    }
                    break;
                case MoveType.SmallCastle:
                    if (move.Figure == FigureKind.WhiteKing)
                    {
                        _board.SmallWhiteCastle(true);
                    }
                    else
                    {
                        _board.SmallBlackCastle(true);
                    }
                    _board[to] = move.Figure;
                    break;
                case MoveType.BigCastle:
                    if (move.Figure == FigureKind.WhiteKing)
                    {
                        _board.BigWhiteCastle(true);
                    }
                    else
                    {
                        _board.BigBlackCastle(true);
                    }
                    _board[to] = move.Figure;
                    break;
                case MoveType.PawnOverMove:
                    if (move.Operation == MoveOperation.Over)
                    {
                        _board.SetOver(to.X, to.Y, true);
                    }
                    _board[to] = move.Figure;
                    break;
                case MoveType.PawnOverAttack:
                    _board[(byte) (to.X * 8 + move.From.Y)] = null;
                    _board.SetOver(to.X, move.From.Y, false);
                    _board[to] = move.Figure;
                    break;
            }

            move.Result = IsCheck() ? MoveResult.Check : MoveResult.Normal;

            Turn = Turn == Turn.White ? Turn.Black : Turn.White;
        }

        public void UnMake()
        {
            IMove move = _moveHistoryService.Remove();

            _board[move.From] = move.Figure;
            var to = move.To;
            _board[to] = null;

            switch (move.Type)
            {
                case MoveType.Attack:
                    _board[to] = _figureHistory.Pop();
                    break;
                case MoveType.SmallCastle:
                    if (move.Figure == FigureKind.WhiteKing)
                    {
                        _board.SmallWhiteCastle(false);
                    }
                    else
                    {
                        _board.SmallBlackCastle(false);
                    }

                    break;
                case MoveType.BigCastle:
                    if (move.Figure == FigureKind.WhiteKing)
                    {
                        _board.BigWhiteCastle(false);
                    }
                    else
                    {
                        _board.BigBlackCastle(false);
                    }

                    break;
                case MoveType.PawnOverMove:
                    if (move.Operation == MoveOperation.Over)
                    {
                        _board.SetOver(to.X, to.Y, false);
                    }

                    break;
                case MoveType.PawnOverAttack:
                    _board[(byte) (to.X * 8 + move.From.Y)] = move.Figure == FigureKind.WhitePawn
                        ? FigureKind.BlackPawn
                        : FigureKind.WhitePawn;
                    _board.SetOver(to.X, move.From.Y, true);
                    break;
            }

            Turn = Turn == Turn.White ? Turn.Black : Turn.White;
        }

        public FigureKind? GetFigure(Coordinate cell)
        {
            return _board[cell];
        }

        public Turn Turn { get; private set; }

        #endregion

        private IEnumerable<IMove> GetBlackAttacks()
        {
            foreach (var cell in _board.GetBlackCells())
            {
                foreach (var attack in GetAllAttacks(cell))
                {
                    yield return attack;
                }
            }
        }

        private IEnumerable<IMove> GetWhiteAttacks()
        {
            foreach (var cell in _board.GetWhiteCells())
            {
                foreach (var attack in GetAllAttacks(cell))
                {
                    yield return attack;
                }
            }
        }

        public IEnumerable<IMove> GetAllAttacks(Coordinate cell)
        {
            FigureKind? figure = _board[cell];

            return _moveProvider.GetAttacks(cell, figure.Value, _board);
        }

        #region Overrides of Object

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"Turn = {Turn}, Key = {Key}, Value = {GetValue()}");
            builder.AppendLine(_board.ToString());
            return builder.ToString();
        }

        #endregion
    }
}