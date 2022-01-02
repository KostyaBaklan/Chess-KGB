using System.Collections.Generic;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.Moves;
using Infrastructure.Models.Enums;

namespace Infrastructure.Services
{
    public class MoveHistoryService: IMoveHistoryService
    {
        private readonly bool[] _whiteSmallCastleHistory;
        private readonly bool[] _whiteBigCastleHistory;
        private readonly bool[] _blackSmallCastleHistory;
        private readonly bool[] _blackBigCastleHistory;
        private readonly HashSet<byte>[] _moveMap;
        private readonly IMove[] _moveHistory;
        private readonly IOpennigService _opennigService;

        public MoveHistoryService(IOpennigService opennigService)
        {
            _opennigService = opennigService;
            var historyDepth = 256;
            _whiteSmallCastleHistory = new bool[historyDepth];
            _whiteBigCastleHistory = new bool[historyDepth];
            _blackSmallCastleHistory = new bool[historyDepth];
            _blackBigCastleHistory = new bool[historyDepth];
            _moveHistory = new IMove[historyDepth];
            _moveMap = new HashSet<byte>[12];
            for (var i = 0; i < _moveMap.Length; i++)
            {
                _moveMap[i] = new HashSet<byte>();
            }
        }

        #region Implementation of IMoveHistoryService

        public int Ply { get; private set; } = -1;
        public bool CanDoWhiteSmallCastle => Ply<0 || _whiteSmallCastleHistory[Ply];
        public bool CanDoWhiteBigCastle => Ply < 0 || _whiteBigCastleHistory[Ply];
        public bool CanDoBlackSmallCastle => Ply < 0 || _blackSmallCastleHistory[Ply];
        public bool CanDoBlackBigCastle => Ply < 0 || _blackBigCastleHistory[Ply];

        public bool IsEmpty()
        {
            return Ply <0;
        }

        public IMove GetLastMove()
        {
            return Ply < 0 ?null: _moveHistory[Ply];
        }

        public void Add(IMove move)
        {
            var ply = Ply;
            _moveHistory[++Ply] = move;
            //if (Ply<17)
            //{
            //    _moveMap[(int) move.Figure].Add(move.To.Key);
            //}
            if (Ply > 0)
            {
                if (Ply % 2 == 0)
                {
                    _blackSmallCastleHistory[Ply] = _blackSmallCastleHistory[ply];
                    _blackBigCastleHistory[Ply] = _blackBigCastleHistory[ply];

                    var figure = move.Figure;
                    if (figure == FigureKind.WhiteKing)
                    {
                        _whiteSmallCastleHistory[Ply] = false;
                        _whiteBigCastleHistory[Ply] = false;
                        return;
                    }

                    if (!_whiteSmallCastleHistory[ply])
                    {
                        _whiteSmallCastleHistory[Ply] = false;
                    }
                    else
                    {
                        _whiteSmallCastleHistory[Ply] = figure != FigureKind.WhiteRook || move.From.Key != 56;
                    }
                    if (!_whiteBigCastleHistory[ply])
                    {
                        _whiteBigCastleHistory[Ply] = false;
                    }
                    else
                    {
                        _whiteBigCastleHistory[Ply] = figure != FigureKind.WhiteRook || move.From.Key != 0;
                    }
                }
                else
                {
                    _whiteSmallCastleHistory[Ply] = _whiteSmallCastleHistory[ply];
                    _whiteBigCastleHistory[Ply] = _whiteBigCastleHistory[ply];

                    var figure = move.Figure;
                    if (figure == FigureKind.BlackKing)
                    {
                        _blackSmallCastleHistory[Ply] = false;
                        _blackBigCastleHistory[Ply] = false;
                        return;
                    }

                    if (!_blackSmallCastleHistory[ply])
                    {
                        _blackSmallCastleHistory[Ply] = false;
                    }
                    else
                    {
                        _blackSmallCastleHistory[Ply] = figure != FigureKind.BlackRook || move.From.Key != 63;
                    }
                    if (!_blackBigCastleHistory[ply])
                    {
                        _blackBigCastleHistory[Ply] = false;
                    }
                    else
                    {
                        _blackBigCastleHistory[Ply] = figure != FigureKind.BlackRook || move.From.Key != 7;
                    }
                }
            }
            else
            {
                _whiteSmallCastleHistory[Ply] = true;
                _whiteBigCastleHistory[Ply] = true;
                _blackSmallCastleHistory[Ply] = true;
                _blackBigCastleHistory[Ply] = true;
            }

            //if (Ply < 6)
            //{
            //    _opennigService.Add(move);
            //}
        }

        public IMove Remove()
        {
            //if (Ply < 6)
            //{
            //    _opennigService.Remove();
            //}
            //var move = _moveHistory[Ply--];
            //if (Ply < 17)
            //{
            //    _moveMap[(int)move.Figure].Remove(move.To.Key);
            //}
            return _moveHistory[Ply--];
        }

        public IEnumerable<IMove> GetHistory()
        {
            for (var i = 0; i < Ply; i++)
            {
                yield return _moveHistory[i];
            }
        }

        public bool IsAdditionalDebutMove(IMove move)
        {
            if (Ply >= 17) return false;

            int x = 1;
            if (move.Figure.IsWhite())
            {
                x = 0;
            }

            for (int i = x; i < Ply; i+=2)
            {
                if (_moveHistory[i].Figure == move.Figure && _moveHistory[i].To.Key == move.From.Key)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}
