using System;
using Infrastructure.Interfaces.Moves;
using Infrastructure.Interfaces.Position;
using Infrastructure.Models.Enums;
using Infrastructure.Moves;

namespace Infrastructure.Services
{
    public class MoveFormatter : IMoveFormatter
    {
        private readonly string _x = "abcdefgh";
        private readonly string _y = "12345678";

        private readonly IMoveProvider _moveProvider;
        private readonly ICoordinateProvider _coordinateProvider;

        public MoveFormatter(IMoveProvider moveProvider, ICoordinateProvider coordinateProvider)
        {
            _moveProvider = moveProvider;
            _coordinateProvider = coordinateProvider;
        }

        #region Implementation of IMoveFormatter

        public string Format(IMove move)
        {
            var format = $"{MoveInternal(move)}";
            return move.Result == MoveResult.Check ? $"{format}+" : format;
        }

        public IMove Parse(string move, Turn turn)
        {
            if (move == "0 - 0")
                return turn == Turn.White ? _moveProvider.GetWhiteSmallCastle() : _moveProvider.GetBlackSmallCastle();
            if (move == "0 - 0 - 0")
                return turn == Turn.White ? _moveProvider.GetWhiteBigCastle() : _moveProvider.GetBlackBigCastle();
            var possibleMove = move.Split(new[] { ' ', '-', '+','x' }, StringSplitOptions.RemoveEmptyEntries);
            return turn == Turn.White ? ParseWhite(possibleMove, move.Contains("x")) : ParseBlack(possibleMove, move.Contains("x"));
        }

        private IMove ParseBlack(string[] move, bool contains)
        {
            string f;
            string t = move[1];
            FigureKind figure;
            switch (move[0][0])
            {
                case 'N':
                    figure = FigureKind.BlackKnight;
                    f = move[0].Substring(1);
                    break;
                case 'K':
                    figure = FigureKind.BlackKing;
                    f = move[0].Substring(1);
                    break;
                case 'B':
                    figure = FigureKind.BlackBishop;
                    f = move[0].Substring(1);
                    break;
                case 'R':
                    figure = FigureKind.BlackRook;
                    f = move[0].Substring(1);
                    break;
                case 'Q':
                    figure = FigureKind.BlackQueen;
                    f = move[0].Substring(1);
                    break;
                default:
                    figure = FigureKind.BlackPawn;
                    f = move[0];
                    break;
            }

            Coordinate from = _coordinateProvider.GetCoordinate(f);
            Coordinate to = _coordinateProvider.GetCoordinate(t);
            return contains ? _moveProvider.GetAttack(figure, from, to) : _moveProvider.GetMove(figure, from, to);
        }

        private IMove ParseWhite(string[] move, bool contains)
        {
            string f;
            string t = move[1];
            FigureKind figure;
            switch (move[0][0])
            {
                case 'N': figure = FigureKind.WhiteKnight;
                    f = move[0].Substring(1);
                    break;
                case 'K':
                    figure = FigureKind.WhiteKing;
                    f = move[0].Substring(1);
                    break;
                case 'B':
                    figure = FigureKind.WhiteBishop;
                    f = move[0].Substring(1);
                    break;
                case 'R':
                    figure = FigureKind.WhiteRook;
                    f = move[0].Substring(1);
                    break;
                case 'Q':
                    figure = FigureKind.WhiteQueen;
                    f = move[0].Substring(1);
                    break;
                default: figure = FigureKind.WhitePawn;
                    f = move[0];
                    break;
            }

            Coordinate from = _coordinateProvider.GetCoordinate(f);
            Coordinate to = _coordinateProvider.GetCoordinate(t);
            return contains ? _moveProvider.GetAttack(figure, from, to) : _moveProvider.GetMove(figure, from, to);
        }

        private string MoveInternal(IMove move)
        {
            if (move is PawnOverAttack pawnOverAttack)
            {
                return FormatPawnOverAttack(pawnOverAttack);
            }

            if (move is Attack attack)
            {
                return FormatAttack(attack);
            }

            if (move is SmallCastle)
            {
                return "0 - 0";
            }

            if (move is BigCastle)
            {
                return "0 - 0 - 0";
            }

            return FormatMove(move);
        }

        private string FormatMove(IMove move)
        {
            var figure = GetFigure(move);

            return $"{figure}{_x[move.From.X]}{_y[move.From.Y]} - {_x[move.To.X]}{_y[move.To.Y]}";
        }

        private string FormatPawnOverAttack(PawnOverAttack attack)
        {
            return $"{_x[attack.From.X]}{_y[attack.From.Y]} x {_x[attack.To.X]}{_y[attack.To.Y]}";
        }

        private string FormatAttack(Attack attack)
        {
            var figure = GetFigure(attack);

            return $"{figure}{_x[attack.From.X]}{_y[attack.From.Y]} x {_x[attack.To.X]}{_y[attack.To.Y]}";
        }

        #endregion

        private static string GetFigure(IMove m)
        {
            string figure = string.Empty;

            switch (m.Figure)
            {
                case FigureKind.WhiteKing:
                case FigureKind.BlackKing:
                    figure = "K";
                    break;
                case FigureKind.WhiteKnight:
                case FigureKind.BlackKnight:
                    figure = "N";
                    break;
                case FigureKind.WhiteBishop:
                case FigureKind.BlackBishop:
                    figure = "B";
                    break;
                case FigureKind.WhiteRook:
                case FigureKind.BlackRook:
                    figure = "R";
                    break;
                case FigureKind.WhiteQueen:
                case FigureKind.BlackQueen:
                    figure = "Q";
                    break;
            }

            return figure;
        }
    }
}