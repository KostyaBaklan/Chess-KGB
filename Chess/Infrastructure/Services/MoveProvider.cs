using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Interfaces.Moves;
using Infrastructure.Interfaces.Position;
using Infrastructure.Models.Enums;
using Infrastructure.Moves;
using Infrastructure.Sorters.Comparers;

namespace Infrastructure.Services
{
    public class MoveWrapper
    {
        public byte From;
        public FigureKind Figure;

        public MoveWrapper(IMove move)
        {
            Move = move;
            From = move.From.Key;
            Figure = move.Figure;
        }

        public IMove Move { get; }
    }

    public class MoveProvider: IMoveProvider
    {
        private readonly int[] _whites;
        private readonly int[] _blacks;

        private readonly HashSet<int>[][] _whiteChecks;
        private readonly HashSet<int>[][] _blackChecks;

        private readonly List<MoveBase>[][] _moves;
        private readonly List<MoveBase>[][] _attacks;
        private readonly List<MoveWrapper>[][][] _attacksTo;
        private readonly ICoordinateProvider _coordinateProvider;

        public MoveProvider(ICoordinateProvider coordinateProvider)
        {
            _whites = new[]
            {
                (int) FigureKind.WhiteQueen, (int) FigureKind.WhiteRook, (int) FigureKind.WhiteKnight, (int) FigureKind.WhiteBishop,
                (int) FigureKind.WhitePawn, (int) FigureKind.WhiteKing
            };
            _blacks = new[]
            {
                (int) FigureKind.BlackQueen, (int) FigureKind.BlackRook, (int) FigureKind.BlackKnight, (int) FigureKind.BlackBishop,
                (int) FigureKind.BlackPawn, (int) FigureKind.BlackKing
            };

            _moves = new List<MoveBase>[12][];
            _attacks = new List<MoveBase>[12][];
            _attacksTo = new List<MoveWrapper>[12][][];
            _coordinateProvider = coordinateProvider;

            foreach (var kind in Enum.GetValues(typeof(FigureKind)).OfType<FigureKind>())
            {
                var k = (int) kind;
                _moves[k] = new List<MoveBase>[64];
                _attacks[k] = new List<MoveBase>[64];
                _attacksTo[k] = new List<MoveWrapper>[64][];
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        var key = i * 8 + j;
                        _moves[k][key] = new List<MoveBase>();
                        _attacks[k][key] = new List<MoveBase>();
                    }
                }

                SetMoves(kind);
                SetAttacks(kind);

                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        var x = i * 8 + j;
                        Dictionary<byte, List<MoveBase>> attacksTo = _attacks[k][x]
                            .GroupBy(g => g.To.Key)
                            .ToDictionary(key => key.Key, v => v.ToList());
                        List<MoveWrapper>[] aTo = new List<MoveWrapper>[64];
                        for (byte q = 0; q < aTo.Length; q++)
                        {
                            if (attacksTo.TryGetValue(q, out var list))
                            {
                                aTo[q] = list.Select(l=>new MoveWrapper(l)).ToList();
                            }
                        }
                        _attacksTo[k][x] = aTo;
                    }
                }
            }

            _whiteChecks = new HashSet<int>[64][];
            _blackChecks = new HashSet<int>[64][];

            SetChecks();

            SetStaticValues();
        }

        private void SetStaticValues()
        {
            var moveValueComparer = new StaticComparer();
            for (var figure = 0; figure < _moves.Length; figure++)
            {
                for (var from = 0; from < _moves[figure].Length; from++)
                {
                    foreach (var move in _moves[figure][from])
                    {
                        if (move.Type == MoveType.BigCastle)
                        {
                            move.Value = 0;
                            move.StaticValue = 5;
                        }
                        else if (move.Type == MoveType.SmallCastle)
                        {
                            move.Value = 0;
                            move.StaticValue = 10;
                        }
                        else if (move.Figure == FigureKind.BlackPawn && move.To.Y == 0)
                        {
                            switch (move.Operation)
                            {
                                case MoveOperation.PawnPromotionToQueen:
                                    move.Value = _attacks[(int) FigureKind.BlackQueen][move.To.Key].Count;
                                    move.StaticValue = _attacks[(int)FigureKind.BlackQueen][move.To.Key].Count;
                                    break;
                                case MoveOperation.PawnPromotionToRook:
                                    move.Value = _attacks[(int)FigureKind.BlackRook][move.To.Key].Count;
                                    move.StaticValue = _attacks[(int)FigureKind.BlackRook][move.To.Key].Count;
                                    break;
                                case MoveOperation.PawnPromotionToKnight:
                                    move.Value = _attacks[(int)FigureKind.BlackKnight][move.To.Key].Count;
                                    move.StaticValue = _attacks[(int)FigureKind.BlackKnight][move.To.Key].Count;
                                    break;
                                case MoveOperation.PawnPromotionToBishop:
                                    move.Value = _attacks[(int)FigureKind.BlackBishop][move.To.Key].Count;
                                    move.StaticValue = _attacks[(int)FigureKind.BlackBishop][move.To.Key].Count;
                                    break;
                            }
                        }
                        else if (move.Figure == FigureKind.WhitePawn && move.To.Y == 7)
                        {
                            switch (move.Operation)
                            {
                                case MoveOperation.PawnPromotionToQueen:
                                    move.Value = _attacks[(int)FigureKind.WhiteQueen][move.To.Key].Count;
                                    move.StaticValue = _attacks[(int)FigureKind.WhiteQueen][move.To.Key].Count;
                                    break;
                                case MoveOperation.PawnPromotionToRook:
                                    move.Value = _attacks[(int)FigureKind.WhiteRook][move.To.Key].Count;
                                    move.StaticValue = _attacks[(int)FigureKind.WhiteRook][move.To.Key].Count;
                                    break;
                                case MoveOperation.PawnPromotionToKnight:
                                    move.Value = _attacks[(int)FigureKind.WhiteKnight][move.To.Key].Count;
                                    move.StaticValue = _attacks[(int)FigureKind.WhiteKnight][move.To.Key].Count;
                                    break;
                                case MoveOperation.PawnPromotionToBishop:
                                    move.Value = _attacks[(int)FigureKind.WhiteBishop][move.To.Key].Count;
                                    move.StaticValue = _attacks[(int)FigureKind.WhiteBishop][move.To.Key].Count;
                                    break;
                            }
                        }
                        else
                        {
                            move.Value = _attacks[figure][move.To.Key].Count - _attacks[figure][from].Count;
                            move.StaticValue = _attacks[figure][move.To.Key].Count;
                        }
                    }

                    if (_moves[figure][from].Count > 1)
                    {
                        _moves[figure][from].Sort(moveValueComparer);
                    }
                }
            }
        }

        #region Checks

        private void SetChecks()
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    var key = i * 8 + j;
                    _whiteChecks[key] = new HashSet<int>[12];
                    _blackChecks[key] = new HashSet<int>[12];
                    foreach (var figure in Enum.GetValues(typeof(FigureKind)).OfType<FigureKind>().Select(kind => (int)kind))
                    {
                        _whiteChecks[key][figure] = new HashSet<int>();
                        _blackChecks[key][figure] = new HashSet<int>();
                    }
                }
            }

            SetWhiteChecks();
            SetBlackChecks();
        }

        private void SetBlackChecks()
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    var to = _coordinateProvider.GetCoordinate(i, j);
                    foreach (var w in _blacks)
                    {
                        for (int x = 0; x < 8; x++)
                        {
                            for (int y = 0; y < 8; y++)
                            {
                                var from = _coordinateProvider.GetCoordinate(x, y);
                                foreach (var move in _attacks[w][from.Key])
                                {
                                    if (move.To.Equals(to))
                                    {
                                        _blackChecks[to.Key][w].Add(from.Key);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void SetWhiteChecks()
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    var to = _coordinateProvider.GetCoordinate(i, j);
                    foreach (var w in _whites)
                    {
                        for (int x = 0; x < 8; x++)
                        {
                            for (int y = 0; y < 8; y++)
                            {
                                var from = _coordinateProvider.GetCoordinate(x, y);
                                foreach (var move in _attacks[w][from.Key])
                                {
                                    if (move.To.Equals(to))
                                    {
                                        _whiteChecks[to.Key][w].Add(from.Key);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion

        private void SetAttacks(FigureKind kind)
        {
            switch (kind)
            {
                case FigureKind.WhitePawn:
                    SetWhitePawnAttacks();
                    break;
                case FigureKind.WhiteKnight:
                    SetWhiteKnightAttacks();
                    break;
                case FigureKind.WhiteBishop:
                    SetWhiteBishopAttacks();
                    break;
                case FigureKind.WhiteRook:
                    SetWhiteRookAttacks();
                    break;
                case FigureKind.WhiteKing:
                    SetWhiteKingAttacks();
                    break;
                case FigureKind.WhiteQueen:
                    SetWhiteQueenAttacks();
                    break;
                case FigureKind.BlackPawn:
                    SetBlackPawnAttacks();
                    break;
                case FigureKind.BlackKnight:
                    SetBlackKnightAttacks();
                    break;
                case FigureKind.BlackBishop:
                    SetBlackBishopAttacks();
                    break;
                case FigureKind.BlackRook:
                    SetBlackRookAttacks();
                    break;
                case FigureKind.BlackKing:
                    SetBlackKingAttacks();
                    break;
                case FigureKind.BlackQueen:
                    SetBlackQueenAttacks();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
            }
        }

        private void SetMoves(FigureKind kind)
        {
            switch (kind)
            {
                case FigureKind.WhitePawn:
                    SetWhitePawnMoves();
                    break;
                case FigureKind.WhiteKnight:
                    SetWhiteKnightMoves();
                    break;
                case FigureKind.WhiteBishop:
                    SetMovesWhiteBishop();
                    break;
                case FigureKind.WhiteRook:
                    SetWhiteRookMoves();
                    break;
                case FigureKind.WhiteKing:
                    SetWhiteKingMoves();
                    break;
                case FigureKind.WhiteQueen:
                    SetWhiteQueenMoves();
                    break;
                case FigureKind.BlackPawn:
                    SetBlackPawnMoves();
                    break;
                case FigureKind.BlackKnight:
                    SetBlackKnightMoves();
                    break;
                case FigureKind.BlackBishop:
                    SetBlackBishopMoves();
                    break;
                case FigureKind.BlackRook:
                    SetBlackRookMoves();
                    break;
                case FigureKind.BlackKing:
                    SetBlackKingMoves();
                    break;
                case FigureKind.BlackQueen:
                    SetBlackQueenMoves();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
            }
        }

        #region Queen

        private void SetBlackQueenMoves()
        {
            var figure = FigureKind.BlackQueen;
            var moveOperation = MoveOperation.MoveQueen;
            var moves = _moves[(int) figure];

            SetHorizontalMoves(moveOperation, figure, moves);

            SetDiagonalMoves(moveOperation, figure, moves);
        }

        private void SetWhiteQueenMoves()
        {
            var figure = FigureKind.WhiteQueen;
            var moveOperation = MoveOperation.MoveQueen;
            var moves = _moves[(int) figure];

            SetHorizontalMoves(moveOperation, figure, moves);

            SetDiagonalMoves(moveOperation, figure, moves);
        }

        private void SetBlackQueenAttacks()
        {
            var figure = FigureKind.BlackQueen;
            var moveOperation = MoveOperation.EatByQueen;
            var moves = _attacks[(int) figure];

            SetHorizontalAttacks(moveOperation, figure, moves);

            SetDiagonalAttacks(moveOperation, figure, moves);
        }

        private void SetWhiteQueenAttacks()
        {
            var figure = FigureKind.WhiteQueen;
            var moveOperation = MoveOperation.EatByQueen;
            var moves = _attacks[(int) figure];

            SetHorizontalAttacks(moveOperation, figure, moves);

            SetDiagonalAttacks(moveOperation, figure, moves);
        }

        #endregion

        #region Rook

        private void SetWhiteRookAttacks()
        {
            var figure = FigureKind.WhiteRook;
            var moveOperation = MoveOperation.EatByRook;
            var moves = _attacks[(int) figure];

            SetHorizontalAttacks(moveOperation, figure, moves);
        }

        private void SetWhiteRookMoves()
        {
            var figure = FigureKind.WhiteRook;
            var moveOperation = MoveOperation.MoveRook;
            var moves = _moves[(int) figure];

            SetHorizontalMoves(moveOperation, figure, moves);
        }

        private void SetBlackRookAttacks()
        {
            var figure = FigureKind.BlackRook;
            var moveOperation = MoveOperation.EatByRook;
            var moves = _attacks[(int) figure];

            SetHorizontalAttacks(moveOperation, figure, moves);
        }

        private void SetBlackRookMoves()
        {
            var figure = FigureKind.BlackRook;
            var moveOperation = MoveOperation.MoveRook;
            var moves = _moves[(int) figure];

            SetHorizontalMoves(moveOperation, figure, moves);
        }

        #endregion

        #region Bishop

        private void SetMovesWhiteBishop()
        {
            var figure = FigureKind.WhiteBishop;
            var moveOperation = MoveOperation.MoveBishop;
            var moves = _moves[(int) figure];

            SetDiagonalMoves(moveOperation, figure, moves);
        }

        private void SetWhiteBishopAttacks()
        {
            var figure = FigureKind.WhiteBishop;
            var moveOperation = MoveOperation.EatByBishop;
            var moves = _attacks[(int) figure];

            SetDiagonalAttacks(moveOperation, figure, moves);
        }

        private void SetBlackBishopMoves()
        {
            var figure = FigureKind.BlackBishop;
            var moveOperation = MoveOperation.MoveBishop;
            var moves = _moves[(int) figure];

            SetDiagonalMoves(moveOperation, figure, moves);
        }

        private void SetBlackBishopAttacks()
        {
            var figure = FigureKind.BlackBishop;
            var moveOperation = MoveOperation.EatByBishop;
            var moves = _attacks[(int) figure];

            SetDiagonalAttacks(moveOperation, figure, moves);
        }

        #endregion

        #region Knight

        private void SetWhiteKnightAttacks()
        {
            var figure = FigureKind.WhiteKnight;
            var moveOperation = MoveOperation.EatByKnight;
            var moves = _attacks[(int) figure];

            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    var cF = _coordinateProvider.GetCoordinate(x, y);
                    if (x > 1 && y < 7)
                    {
                        var cT = _coordinateProvider.GetCoordinate(x - 2, y + 1);
                        moves[x * 8 + y].Add(new Attack(cF, cT, moveOperation, figure));
                    }
                    if (x > 0 && y < 6)
                    {
                        var cT = _coordinateProvider.GetCoordinate(x - 1, y + 2);
                        moves[x * 8 + y].Add(new Attack(cF, cT, moveOperation, figure));
                    }
                    if (x < 7 && y < 6)
                    {
                        var cT = _coordinateProvider.GetCoordinate(x + 1, y + 2);
                        moves[x * 8 + y].Add(new Attack(cF, cT, moveOperation, figure));
                    }
                    if (x < 6 && y < 7)
                    {
                        var cT = _coordinateProvider.GetCoordinate(x + 2, y + 1);
                        moves[x * 8 + y].Add(new Attack(cF, cT, moveOperation, figure));
                    }
                    if (x < 6 && y > 0)
                    {
                        var cT = _coordinateProvider.GetCoordinate(x + 2, y - 1);
                        moves[x * 8 + y].Add(new Attack(cF, cT, moveOperation, figure));
                    }
                    if (x < 7 && y > 1)
                    {
                        var cT = _coordinateProvider.GetCoordinate(x + 1, y - 2);
                        moves[x * 8 + y].Add(new Attack(cF, cT, moveOperation, figure));
                    }
                    if (x > 1 && y > 0)
                    {
                        var cT = _coordinateProvider.GetCoordinate(x - 2, y - 1);
                        moves[x * 8 + y].Add(new Attack(cF, cT, moveOperation, figure));
                    }
                    if (x > 0 && y > 1)
                    {
                        var cT = _coordinateProvider.GetCoordinate(x - 1, y - 2);
                        moves[x * 8 + y].Add(new Attack(cF, cT, moveOperation, figure));
                    }
                }
            }
        }

        private void SetWhiteKnightMoves()
        {
            var figure = FigureKind.WhiteKnight;
            var moveOperation = MoveOperation.MoveKnight;
            var moves = _moves[(int) figure];

            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    var cF = _coordinateProvider.GetCoordinate(x, y);
                    if (x > 1 && y < 7)
                    {
                        var cT = _coordinateProvider.GetCoordinate(x-2, y+1);
                        moves[x * 8 + y].Add(new Move(cF,cT,moveOperation, figure));
                    }
                    if (x > 0 && y < 6)
                    {
                        var cT = _coordinateProvider.GetCoordinate(x - 1, y + 2);
                        moves[x * 8 + y].Add(new Move(cF, cT, moveOperation, figure));
                    }
                    if (x < 7 && y < 6)
                    {
                        var cT = _coordinateProvider.GetCoordinate(x + 1, y + 2);
                        moves[x * 8 + y].Add(new Move(cF, cT, moveOperation, figure));
                    }
                    if (x < 6 && y < 7)
                    {
                        var cT = _coordinateProvider.GetCoordinate(x + 2, y + 1);
                        moves[x * 8 + y].Add(new Move(cF, cT, moveOperation, figure));
                    }
                    if (x < 6 && y > 0)
                    {
                        var cT = _coordinateProvider.GetCoordinate(x + 2, y - 1);
                        moves[x * 8 + y].Add(new Move(cF, cT, moveOperation, figure));
                    }
                    if (x < 7 && y > 1)
                    {
                        var cT = _coordinateProvider.GetCoordinate(x + 1, y - 2);
                        moves[x * 8 + y].Add(new Move(cF, cT, moveOperation, figure));
                    }
                    if (x > 1 && y > 0)
                    {
                        var cT = _coordinateProvider.GetCoordinate(x - 2, y - 1);
                        moves[x * 8 + y].Add(new Move(cF, cT, moveOperation, figure));
                    }
                    if (x > 0 && y > 1)
                    {
                        var cT = _coordinateProvider.GetCoordinate(x - 1, y - 2);
                        moves[x * 8 + y].Add(new Move(cF, cT, moveOperation, figure));
                    }
                }
            }
        }

        private void SetBlackKnightAttacks()
        {
            var figure = FigureKind.BlackKnight;
            var moveOperation = MoveOperation.EatByKnight;
            var moves = _attacks[(int) figure];

            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    var cF = _coordinateProvider.GetCoordinate(x, y);
                    if (x > 1 && y < 7)
                    {
                        var cT = _coordinateProvider.GetCoordinate(x - 2, y + 1);
                        moves[x * 8 + y].Add(new Attack(cF, cT, moveOperation, figure));
                    }
                    if (x > 0 && y < 6)
                    {
                        var cT = _coordinateProvider.GetCoordinate(x - 1, y + 2);
                        moves[x * 8 + y].Add(new Attack(cF, cT, moveOperation, figure));
                    }
                    if (x < 7 && y < 6)
                    {
                        var cT = _coordinateProvider.GetCoordinate(x + 1, y + 2);
                        moves[x * 8 + y].Add(new Attack(cF, cT, moveOperation, figure));
                    }
                    if (x < 6 && y < 7)
                    {
                        var cT = _coordinateProvider.GetCoordinate(x + 2, y + 1);
                        moves[x * 8 + y].Add(new Attack(cF, cT, moveOperation, figure));
                    }
                    if (x < 6 && y > 0)
                    {
                        var cT = _coordinateProvider.GetCoordinate(x + 2, y - 1);
                        moves[x * 8 + y].Add(new Attack(cF, cT, moveOperation, figure));
                    }
                    if (x < 7 && y > 1)
                    {
                        var cT = _coordinateProvider.GetCoordinate(x + 1, y - 2);
                        moves[x * 8 + y].Add(new Attack(cF, cT, moveOperation, figure));
                    }
                    if (x > 1 && y > 0)
                    {
                        var cT = _coordinateProvider.GetCoordinate(x - 2, y - 1);
                        moves[x * 8 + y].Add(new Attack(cF, cT, moveOperation, figure));
                    }
                    if (x > 0 && y > 1)
                    {
                        var cT = _coordinateProvider.GetCoordinate(x - 1, y - 2);
                        moves[x * 8 + y].Add(new Attack(cF, cT, moveOperation, figure));
                    }
                }
            }
        }

        private void SetBlackKnightMoves()
        {
            var figure = FigureKind.BlackKnight;
            var moveOperation = MoveOperation.MoveKnight;
            var moves = _moves[(int) figure];

            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    var cF = _coordinateProvider.GetCoordinate(x, y);
                    if (x > 1 && y < 7)
                    {
                        var cT = _coordinateProvider.GetCoordinate(x - 2, y + 1);
                        moves[x * 8 + y].Add(new Move(cF, cT, moveOperation, figure));
                    }
                    if (x > 0 && y < 6)
                    {
                        var cT = _coordinateProvider.GetCoordinate(x - 1, y + 2);
                        moves[x * 8 + y].Add(new Move(cF, cT, moveOperation, figure));
                    }
                    if (x < 7 && y < 6)
                    {
                        var cT = _coordinateProvider.GetCoordinate(x + 1, y + 2);
                        moves[x * 8 + y].Add(new Move(cF, cT, moveOperation, figure));
                    }
                    if (x < 6 && y < 7)
                    {
                        var cT = _coordinateProvider.GetCoordinate(x + 2, y + 1);
                        moves[x * 8 + y].Add(new Move(cF, cT, moveOperation, figure));
                    }
                    if (x < 6 && y > 0)
                    {
                        var cT = _coordinateProvider.GetCoordinate(x + 2, y - 1);
                        moves[x * 8 + y].Add(new Move(cF, cT, moveOperation, figure));
                    }
                    if (x < 7 && y > 1)
                    {
                        var cT = _coordinateProvider.GetCoordinate(x + 1, y - 2);
                        moves[x * 8 + y].Add(new Move(cF, cT, moveOperation, figure));
                    }
                    if (x > 1 && y > 0)
                    {
                        var cT = _coordinateProvider.GetCoordinate(x - 2, y - 1);
                        moves[x * 8 + y].Add(new Move(cF, cT, moveOperation, figure));
                    }
                    if (x > 0 && y > 1)
                    {
                        var cT = _coordinateProvider.GetCoordinate(x - 1, y - 2);
                        moves[x * 8 + y].Add(new Move(cF, cT, moveOperation, figure));
                    }
                }
            }
        }

        #endregion

        #region Pawn

        private void SetBlackPawnMoves()
        {
            var figure = FigureKind.BlackPawn;
            var moves = _moves[(int) figure];
            for (int x = 0; x < 8; x++)
            {
                var coordinateF = _coordinateProvider.GetCoordinate(x, 6);
                var coordinateT = _coordinateProvider.GetCoordinate(x, 4);

                var move = new PawnOverMove(coordinateF, coordinateT, figure);
                move.EmptyCells.Add(_coordinateProvider.GetCoordinate(x, 5).Key);
                moves[x * 8 + 6].Add(move);
            }

            for (int x = 0; x < 8; x++)
            {
                for (int y = 6; y > 1; y--)
                {
                    var coordinateF = _coordinateProvider.GetCoordinate(x, y);
                    var coordinateT = _coordinateProvider.GetCoordinate(x, y - 1);

                    var item = new Move(coordinateF, coordinateT, MoveOperation.MovePawn,
                        figure);

                    moves[x * 8 + y].Add(item);
                }
            }

            for (int x = 0; x < 8; x++)
            {
                var coordinateF = _coordinateProvider.GetCoordinate(x, 1);
                var coordinateT = _coordinateProvider.GetCoordinate(x, 0);

                moves[x * 8 + 1].Add(new Move(coordinateF, coordinateT, MoveOperation.PawnPromotionToQueen,
                    figure));
                moves[x * 8 + 1].Add(new Move(coordinateF, coordinateT, MoveOperation.PawnPromotionToRook,
                    figure));
                moves[x * 8 + 1].Add(new Move(coordinateF, coordinateT, MoveOperation.PawnPromotionToBishop,
                    figure));
                moves[x * 8 + 1].Add(new Move(coordinateF, coordinateT, MoveOperation.PawnPromotionToKnight,
                    figure));
            }
        }

        private void SetWhitePawnMoves()
        {
            var figure = FigureKind.WhitePawn;
            var moves = _moves[(int) figure];
            for (int x = 0; x < 8; x++)
            {
                var coordinateF = _coordinateProvider.GetCoordinate(x, 1);
                var coordinateT = _coordinateProvider.GetCoordinate(x, 3);

                var move = new PawnOverMove(coordinateF, coordinateT, figure);
                move.EmptyCells.Add(_coordinateProvider.GetCoordinate(x, 2).Key);
                moves[x * 8 + 1].Add(move);
            }

            for (int x = 0; x < 8; x++)
            {
                for (int y = 1; y < 6; y++)
                {
                    var coordinateF = _coordinateProvider.GetCoordinate(x, y);
                    var coordinateT = _coordinateProvider.GetCoordinate(x, y + 1);

                    var item = new Move(coordinateF, coordinateT, MoveOperation.MovePawn,
                        figure);

                    moves[x * 8 + y].Add(item);
                }
            }

            for (int x = 0; x < 8; x++)
            {
                var coordinateF = _coordinateProvider.GetCoordinate(x, 6);
                var coordinateT = _coordinateProvider.GetCoordinate(x, 7);

                int y = 6;
                moves[x * 8 + y].Add(new Move(coordinateF, coordinateT, MoveOperation.PawnPromotionToQueen,
                    figure));
                moves[x * 8 + y].Add(new Move(coordinateF, coordinateT, MoveOperation.PawnPromotionToRook,
                    figure));
                moves[x * 8 + y].Add(new Move(coordinateF, coordinateT, MoveOperation.PawnPromotionToBishop,
                    figure));
                moves[x * 8 + y].Add(new Move(coordinateF, coordinateT, MoveOperation.PawnPromotionToKnight,
                    figure));
            }
        }

        private void SetWhitePawnAttacks()
        {
            var figure = FigureKind.WhitePawn;
            var moves = _attacks[(int) figure];
            for (int x = 0; x < 8; x++)
            {
                for (int y = 1; y < 6; y++)
                {
                    var coordinateF = _coordinateProvider.GetCoordinate(x, y);
                    if (x > 0)
                    {
                        var cT = _coordinateProvider.GetCoordinate(x - 1, y + 1);
                        moves[x * 8 + y].Add(new Attack(coordinateF, cT, MoveOperation.EatByPawn, figure));
                        if (y == 4)
                        {
                            moves[x * 8 + y].Add(new PawnOverAttack(coordinateF, cT, figure));
                        }
                    }
                    if (x < 7)
                    {
                        var cT = _coordinateProvider.GetCoordinate(x + 1, y + 1);
                        moves[x * 8 + y].Add(new Attack(coordinateF, cT, MoveOperation.EatByPawn,
                            figure));
                        if (y == 4)
                        {
                            moves[x * 8 + y].Add(new PawnOverAttack(coordinateF, cT, figure));
                        }
                    }
                }
            }

            for (int x = 0; x < 8; x++)
            {
                var coordinateF = _coordinateProvider.GetCoordinate(x, 6);
                int y = 6;
                if (x > 0)
                {
                    var cT = _coordinateProvider.GetCoordinate(x - 1, 7);

                    moves[x * 8 + y].Add(new Attack(coordinateF, cT, MoveOperation.EatByPawnPromotionToQueen,
                        figure));
                    moves[x * 8 + y].Add(new Attack(coordinateF, cT, MoveOperation.EatByPawnPromotionToRook,
                        figure));
                    moves[x * 8 + y].Add(new Attack(coordinateF, cT, MoveOperation.EatByPawnPromotionToBishop,
                        figure));
                    moves[x * 8 + y].Add(new Attack(coordinateF, cT, MoveOperation.EatByPawnPromotionToKnight,
                        figure));
                }
                if (x < 7)
                {
                    var cT = _coordinateProvider.GetCoordinate(x + 1, 7);

                    moves[x * 8 + y].Add(new Attack(coordinateF, cT, MoveOperation.EatByPawnPromotionToQueen,
                        figure));
                    moves[x * 8 + y].Add(new Attack(coordinateF, cT, MoveOperation.EatByPawnPromotionToRook,
                        figure));
                    moves[x * 8 + y].Add(new Attack(coordinateF, cT, MoveOperation.EatByPawnPromotionToBishop,
                        figure));
                    moves[x * 8 + y].Add(new Attack(coordinateF, cT, MoveOperation.EatByPawnPromotionToKnight,
                        figure));
                }
            }
        }

        private void SetBlackPawnAttacks()
        {
            var figure = FigureKind.BlackPawn;
            var moves = _attacks[(int) figure];
            for (int x = 0; x < 8; x++)
            {
                for (int y = 6; y > 1; y--)
                {
                    var coordinateF = _coordinateProvider.GetCoordinate(x, y);
                    if (x > 0)
                    {
                        var cT = _coordinateProvider.GetCoordinate(x - 1, y - 1);
                        moves[x * 8 + y].Add(new Attack(coordinateF, cT, MoveOperation.EatByPawn,
                            figure));
                        if (y == 3)
                        {
                            moves[x * 8 + y].Add(new PawnOverAttack(coordinateF, cT, figure));
                        }
                    }
                    if (x < 7)
                    {
                        var cT = _coordinateProvider.GetCoordinate(x + 1, y - 1);
                        moves[x * 8 + y].Add(new Attack(coordinateF, cT, MoveOperation.EatByPawn,
                            figure));
                        if (y == 3)
                        {
                            moves[x * 8 + y].Add(new PawnOverAttack(coordinateF, cT, figure));
                        }
                    }
                }
            }

            for (int x = 0; x < 8; x++)
            {
                var coordinateF = _coordinateProvider.GetCoordinate(x, 1);
                int y = 1;
                if (x > 0)
                {
                    var cT = _coordinateProvider.GetCoordinate(x - 1, 0);

                    moves[x * 8 + y].Add(new Attack(coordinateF, cT, MoveOperation.EatByPawnPromotionToQueen,
                        figure));
                    moves[x * 8 + y].Add(new Attack(coordinateF, cT, MoveOperation.EatByPawnPromotionToRook,
                        figure));
                    moves[x * 8 + y].Add(new Attack(coordinateF, cT, MoveOperation.EatByPawnPromotionToBishop,
                        figure));
                    moves[x * 8 + y].Add(new Attack(coordinateF, cT, MoveOperation.EatByPawnPromotionToKnight,
                        figure));
                }
                if (x < 7)
                {
                    var cT = _coordinateProvider.GetCoordinate(x + 1, 0);

                    moves[x * 8 + y].Add(new Attack(coordinateF, cT, MoveOperation.EatByPawnPromotionToQueen,
                        figure));
                    moves[x * 8 + y].Add(new Attack(coordinateF, cT, MoveOperation.EatByPawnPromotionToRook,
                        figure));
                    moves[x * 8 + y].Add(new Attack(coordinateF, cT, MoveOperation.EatByPawnPromotionToBishop,
                        figure));
                    moves[x * 8 + y].Add(new Attack(coordinateF, cT, MoveOperation.EatByPawnPromotionToKnight,
                        figure));
                }
            }
        }

        #endregion

        #region King

        private void SetBlackKingAttacks()
        {
            var figure = FigureKind.BlackKing;
            var moveOperation = MoveOperation.EatByKing;
            var moves = _attacks[(int) figure];

            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    var cF = _coordinateProvider.GetCoordinate(x, y);
                    if (y > 0)
                    {
                        var ct = _coordinateProvider.GetCoordinate(x, y - 1);
                        moves[x * 8 + y].Add(new Attack(cF, ct, moveOperation, figure));

                        if (x > 0)
                        {
                            var ct1 = _coordinateProvider.GetCoordinate(x - 1, y - 1);
                            moves[x * 8 + y].Add(new Attack(cF, ct1, moveOperation, figure));
                        }
                        if (x < 7)
                        {
                            var ct1 = _coordinateProvider.GetCoordinate(x + 1, y - 1);
                            moves[x * 8 + y].Add(new Attack(cF, ct1, moveOperation, figure));
                        }
                    }
                    if (y < 7)
                    {
                        var ct = _coordinateProvider.GetCoordinate(x, y + 1);
                        moves[x * 8 + y].Add(new Attack(cF, ct, moveOperation, figure));

                        if (x > 0)
                        {
                            var ct1 = _coordinateProvider.GetCoordinate(x - 1, y + 1);
                            moves[x * 8 + y].Add(new Attack(cF, ct1, moveOperation, figure));
                        }
                        if (x < 7)
                        {
                            var ct1 = _coordinateProvider.GetCoordinate(x + 1, y + 1);
                            moves[x * 8 + y].Add(new Attack(cF, ct1, moveOperation, figure));
                        }
                    }

                    if (x > 0)
                    {
                        var ct1 = _coordinateProvider.GetCoordinate(x - 1, y);
                        moves[x * 8 + y].Add(new Attack(cF, ct1, moveOperation, figure));
                    }
                    if (x < 7)
                    {
                        var ct1 = _coordinateProvider.GetCoordinate(x + 1, y);
                        moves[x * 8 + y].Add(new Attack(cF, ct1, moveOperation, figure));
                    }
                }
            }
        }

        private void SetBlackKingMoves()
        {
            var figure = FigureKind.BlackKing;
            var moveOperation = MoveOperation.MoveKing;
            var moves = _moves[(int) figure];

            var cF1 = _coordinateProvider.GetCoordinate(4, 7);
            var ct11 = _coordinateProvider.GetCoordinate(6, 7);
            var smallCastle = new SmallCastle(cF1, ct11, figure);
            smallCastle.EmptyCells.Add(_coordinateProvider.GetCoordinate(5, 7).Key);
            moves[39].Add(smallCastle);

            var cF2 = _coordinateProvider.GetCoordinate(4, 7);
            var ct12 = _coordinateProvider.GetCoordinate(2, 7);
            var bigCastle = new BigCastle(cF2, ct12, figure);
            bigCastle.EmptyCells.Add(_coordinateProvider.GetCoordinate(3, 7).Key);
            bigCastle.EmptyCells.Add(_coordinateProvider.GetCoordinate(1, 7).Key);
            moves[39].Add(bigCastle);

            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    var cF = _coordinateProvider.GetCoordinate(x, y);
                    if (y > 0)
                    {
                        var ct = _coordinateProvider.GetCoordinate(x, y - 1);
                        moves[x * 8 + y].Add(new Move(cF, ct, moveOperation, figure));

                        if (x > 0)
                        {
                            var ct1 = _coordinateProvider.GetCoordinate(x - 1, y - 1);
                            moves[x * 8 + y].Add(new Move(cF, ct1, moveOperation, figure));
                        }
                        if (x < 7)
                        {
                            var ct1 = _coordinateProvider.GetCoordinate(x + 1, y - 1);
                            moves[x * 8 + y].Add(new Move(cF, ct1, moveOperation, figure));
                        }
                    }
                    if (y < 7)
                    {
                        var ct = _coordinateProvider.GetCoordinate(x, y + 1);
                        moves[x * 8 + y].Add(new Move(cF, ct, moveOperation, figure));

                        if (x > 0)
                        {
                            var ct1 = _coordinateProvider.GetCoordinate(x - 1, y + 1);
                            moves[x * 8 + y].Add(new Move(cF, ct1, moveOperation, figure));
                        }
                        if (x < 7)
                        {
                            var ct1 = _coordinateProvider.GetCoordinate(x + 1, y + 1);
                            moves[x * 8 + y].Add(new Move(cF, ct1, moveOperation, figure));
                        }
                    }

                    if (x > 0)
                    {
                        var ct1 = _coordinateProvider.GetCoordinate(x - 1, y);
                        moves[x * 8 + y].Add(new Move(cF, ct1, moveOperation, figure));
                    }
                    if (x < 7)
                    {
                        var ct1 = _coordinateProvider.GetCoordinate(x + 1, y);
                        moves[x * 8 + y].Add(new Move(cF, ct1, moveOperation, figure));
                    }
                }
            }
        }

        private void SetWhiteKingMoves()
        {
            var figure = FigureKind.WhiteKing;
            var moveOperation = MoveOperation.MoveKing;
            var moves = _moves[(int) figure];

            var cF1 = _coordinateProvider.GetCoordinate(4, 0);
            var ct11 = _coordinateProvider.GetCoordinate(6, 0);
            var smallCastle = new SmallCastle(cF1, ct11, figure);
            smallCastle.EmptyCells.Add(_coordinateProvider.GetCoordinate(5, 0).Key);
            moves[32].Add(smallCastle);

            var cF2 = _coordinateProvider.GetCoordinate(4, 0);
            var ct12 = _coordinateProvider.GetCoordinate(2, 0);
            var bigCastle = new BigCastle(cF2, ct12, figure);
            bigCastle.EmptyCells.Add(_coordinateProvider.GetCoordinate(3, 0).Key);
            bigCastle.EmptyCells.Add(_coordinateProvider.GetCoordinate(1, 0).Key);
            moves[32].Add(bigCastle);

            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    var cF = _coordinateProvider.GetCoordinate(x, y);
                    if (y > 0)
                    {
                        var ct = _coordinateProvider.GetCoordinate(x, y-1);
                        moves[x * 8 + y].Add(new Move(cF, ct, moveOperation,figure));

                        if (x > 0)
                        {
                            var ct1 = _coordinateProvider.GetCoordinate(x - 1, y - 1);
                            moves[x * 8 + y].Add(new Move(cF, ct1, moveOperation, figure));
                        }
                        if (x < 7)
                        {
                            var ct1 = _coordinateProvider.GetCoordinate(x + 1, y - 1);
                            moves[x * 8 + y].Add(new Move(cF, ct1, moveOperation, figure));
                        }
                    }
                    if (y < 7)
                    {
                        var ct = _coordinateProvider.GetCoordinate(x, y + 1);
                        moves[x * 8 + y].Add(new Move(cF, ct, moveOperation, figure));

                        if (x > 0)
                        {
                            var ct1 = _coordinateProvider.GetCoordinate(x - 1, y + 1);
                            moves[x * 8 + y].Add(new Move(cF, ct1, moveOperation, figure));
                        }
                        if (x < 7)
                        {
                            var ct1 = _coordinateProvider.GetCoordinate(x + 1, y + 1);
                            moves[x * 8 + y].Add(new Move(cF, ct1, moveOperation, figure));
                        }
                    }
                    if (x > 0)
                    {
                        var ct1 = _coordinateProvider.GetCoordinate(x - 1, y);
                        moves[x * 8 + y].Add(new Move(cF, ct1, moveOperation, figure));
                    }
                    if (x < 7)
                    {
                        var ct1 = _coordinateProvider.GetCoordinate(x + 1, y);
                        moves[x * 8 + y].Add(new Move(cF, ct1, moveOperation, figure));
                    }
                }
            }
        }

        private void SetWhiteKingAttacks()
        {
            var figure = FigureKind.WhiteKing;
            var moveOperation = MoveOperation.EatByKing;
            var moves = _attacks[(int) figure];

            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    var cF = _coordinateProvider.GetCoordinate(x, y);
                    if (y > 0)
                    {
                        var ct = _coordinateProvider.GetCoordinate(x, y - 1);
                        moves[x * 8 + y].Add(new Attack(cF, ct, moveOperation, figure));

                        if (x > 0)
                        {
                            var ct1 = _coordinateProvider.GetCoordinate(x - 1, y - 1);
                            moves[x * 8 + y].Add(new Attack(cF, ct1, moveOperation, figure));
                        }
                        if (x < 7)
                        {
                            var ct1 = _coordinateProvider.GetCoordinate(x + 1, y - 1);
                            moves[x * 8 + y].Add(new Attack(cF, ct1, moveOperation, figure));
                        }
                    }
                    if (y < 7)
                    {
                        var ct = _coordinateProvider.GetCoordinate(x, y + 1);
                        moves[x * 8 + y].Add(new Attack(cF, ct, moveOperation, figure));

                        if (x > 0)
                        {
                            var ct1 = _coordinateProvider.GetCoordinate(x - 1, y + 1);
                            moves[x * 8 + y].Add(new Attack(cF, ct1, moveOperation, figure));
                        }
                        if (x < 7)
                        {
                            var ct1 = _coordinateProvider.GetCoordinate(x + 1, y + 1);
                            moves[x * 8 + y].Add(new Attack(cF, ct1, moveOperation, figure));
                        }
                    }
                    if (x > 0)
                    {
                        var ct1 = _coordinateProvider.GetCoordinate(x - 1, y);
                        moves[x * 8 + y].Add(new Attack(cF, ct1, moveOperation, figure));
                    }
                    if (x < 7)
                    {
                        var ct1 = _coordinateProvider.GetCoordinate(x + 1, y);
                        moves[x * 8 + y].Add(new Attack(cF, ct1, moveOperation, figure));
                    }
                }
            }
        }

        #endregion

        #region Implementation of IMoveProvider

        public IEnumerable<IMove> GetMoves(Coordinate from, FigureKind figure, IBoard board)
        {
            return _moves[(int) figure][from.Key].Where(m => m.IsLegal(board));
        }

        public IEnumerable<IMove> GetAttacks(Coordinate from, FigureKind figure, IBoard board)
        {
           return _attacks[(int) figure][from.Key].Where(m => m.IsLegal(board));
        }

        public bool AnyBlackCheck(IBoard board)
        {
            var kingPosition = board.WhiteKingPosition.Key;
            return IsWhiteUnderAttack(board, kingPosition);
        }

        public bool AnyWhiteCheck(IBoard board)
        {
            var kingPosition = board.BlackKingPosition.Key;
            return IsBlackUnderAttack(board, kingPosition);
        }

        public IMove GetWhiteSmallCastle()
        {
            return _moves[(int) FigureKind.WhiteKing][32].FirstOrDefault(m=>m.To.Equals(_coordinateProvider.GetCoordinate(6,0)));
        }

        public IMove GetBlackSmallCastle()
        {
            return _moves[(int)FigureKind.BlackKing][39].FirstOrDefault(m => m.To.Equals(_coordinateProvider.GetCoordinate(6, 7)));
        }

        public IMove GetWhiteBigCastle()
        {
            return _moves[(int)FigureKind.WhiteKing][32].FirstOrDefault(m => m.To.Equals(_coordinateProvider.GetCoordinate(2, 0)));
        }

        public IMove GetBlackBigCastle()
        {
            return _moves[(int)FigureKind.BlackKnight][39].FirstOrDefault(m => m.To.Equals(_coordinateProvider.GetCoordinate(2, 7)));
        }

        public IMove GetMove(FigureKind figure, Coordinate from, Coordinate to)
        {
            return _moves[(int)figure][from.Key].FirstOrDefault(m => m.To.Equals(to));
        }

        public IMove GetAttack(FigureKind figure, Coordinate from, Coordinate to)
        {
            return _attacks[(int)figure][from.Key].FirstOrDefault(m => m.To.Equals(to));
        }

        public bool CanDoWhiteSmallCastle(IBoard board)
        {
            return !IsWhiteUnderAttack(board, 40) ;//&& !IsWhiteUnderAttack(board, 48);
        }

        public bool CanDoWhiteBigCastle(IBoard board)
        {
            return !IsWhiteUnderAttack(board, 24) ;//&& !IsWhiteUnderAttack(board, 16);
        }

        public bool CanDoBlackSmallCastle(IBoard board)
        {
            return !IsBlackUnderAttack(board, 47) ;//&& !IsBlackUnderAttack(board, 55);
        }

        public bool CanDoBlackBigCastle(IBoard board)
        {
            return !IsBlackUnderAttack(board, 31) ;//&& !IsBlackUnderAttack(board, 23);
        }

        #endregion

        #region Private

        private void SetDiagonalMoves(MoveOperation moveOperation, FigureKind figure, List<MoveBase>[] moves)
        {
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    var cF = _coordinateProvider.GetCoordinate(x, y);

                    int offset = 0;
                    var a = x + 1;
                    var b = y + 1;
                    while (a < 8 && b < 8)
                    {
                        var cT = _coordinateProvider.GetCoordinate(a, b);
                        var move = new Move(cF, cT, moveOperation, figure);
                        for (int i = 1; i <= offset; i++)
                        {
                            move.EmptyCells.Add(_coordinateProvider.GetCoordinate(x + i, y + i).Key);
                        }

                        moves[x * 8 + y].Add(move);
                        a++;
                        b++;
                        offset++;
                    }

                    offset = 0;
                    a = x - 1;
                    b = y + 1;
                    while (a > -1 && b < 8)
                    {
                        var cT = _coordinateProvider.GetCoordinate(a, b);
                        var move = new Move(cF, cT, moveOperation, figure);
                        for (int i = 1; i <= offset; i++)
                        {
                            move.EmptyCells.Add(_coordinateProvider.GetCoordinate(x - i, y + i).Key);
                        }

                        moves[x * 8 + y].Add(move);
                        a--;
                        b++;
                        offset++;
                    }

                    offset = 0;
                    a = x - 1;
                    b = y - 1;
                    while (a > -1 && b > -1)
                    {
                        var cT = _coordinateProvider.GetCoordinate(a, b);
                        var move = new Move(cF, cT, moveOperation, figure);
                        for (int i = 1; i <= offset; i++)
                        {
                            move.EmptyCells.Add(_coordinateProvider.GetCoordinate(x - i, y - i).Key);
                        }

                        moves[x * 8 + y].Add(move);
                        a--;
                        b--;
                        offset++;
                    }

                    offset = 0;
                    a = x + 1;
                    b = y - 1;
                    while (a < 8 && b > -1)
                    {
                        var cT = _coordinateProvider.GetCoordinate(a, b);
                        var move = new Move(cF, cT, moveOperation, figure);
                        for (int i = 1; i <= offset; i++)
                        {
                            move.EmptyCells.Add(_coordinateProvider.GetCoordinate(x + i, y - i).Key);
                        }

                        moves[x * 8 + y].Add(move);
                        a++;
                        b--;
                        offset++;
                    }
                }
            }
        }

        private void SetDiagonalAttacks(MoveOperation moveOperation, FigureKind figure, List<MoveBase>[] moves)
        {
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    var cF = _coordinateProvider.GetCoordinate(x, y);

                    int offset = 0;
                    var a = x + 1;
                    var b = y + 1;
                    while (a < 8 && b < 8)
                    {
                        var cT = _coordinateProvider.GetCoordinate(a, b);
                        var move = new Attack(cF, cT, moveOperation, figure);
                        for (int i = 1; i <= offset; i++)
                        {
                            move.EmptyCells.Add(_coordinateProvider.GetCoordinate(x + i, y + i).Key);
                        }

                        moves[x * 8 + y].Add(move);
                        a++;
                        b++;
                        offset++;
                    }

                    offset = 0;
                    a = x - 1;
                    b = y + 1;
                    while (a > -1 && b < 8)
                    {
                        var cT = _coordinateProvider.GetCoordinate(a, b);
                        var move = new Attack(cF, cT, moveOperation, figure);
                        for (int i = 1; i <= offset; i++)
                        {
                            move.EmptyCells.Add(_coordinateProvider.GetCoordinate(x - i, y + i).Key);
                        }

                        moves[x * 8 + y].Add(move);
                        a--;
                        b++;
                        offset++;
                    }

                    offset = 0;
                    a = x - 1;
                    b = y - 1;
                    while (a > -1 && b > -1)
                    {
                        var cT = _coordinateProvider.GetCoordinate(a, b);
                        var move = new Attack(cF, cT, moveOperation, figure);
                        for (int i = 1; i <= offset; i++)
                        {
                            move.EmptyCells.Add(_coordinateProvider.GetCoordinate(x - i, y - i).Key);
                        }

                        moves[x * 8 + y].Add(move);
                        a--;
                        b--;
                        offset++;
                    }

                    offset = 0;
                    a = x + 1;
                    b = y - 1;
                    while (a < 8 && b > -1)
                    {
                        var cT = _coordinateProvider.GetCoordinate(a, b);
                        var move = new Attack(cF, cT, moveOperation, figure);
                        for (int i = 1; i <= offset; i++)
                        {
                            move.EmptyCells.Add(_coordinateProvider.GetCoordinate(x + i, y - i).Key);
                        }

                        moves[x * 8 + y].Add(move);
                        a++;
                        b--;
                        offset++;
                    }
                }
            }
        }

        private void SetHorizontalMoves(MoveOperation moveOperation, FigureKind figure, List<MoveBase>[] moves)
        {
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    var cF = _coordinateProvider.GetCoordinate(x, y);

                    int offset = 0;
                    var a = x - 1;
                    while (a > -1)
                    {
                        var cT = _coordinateProvider.GetCoordinate(a, y);
                        var move = new Move(cF, cT, moveOperation, figure);
                        for (int i = 1; i <= offset; i++)
                        {
                            move.EmptyCells.Add(_coordinateProvider.GetCoordinate(x - i, y).Key);
                        }

                        moves[x * 8 + y].Add(move);
                        a--;
                        offset++;
                    }

                    offset = 0;
                    a = x + 1;
                    while (a < 8)
                    {
                        var cT = _coordinateProvider.GetCoordinate(a, y);
                        var move = new Move(cF, cT, moveOperation, figure);
                        for (int i = 1; i <= offset; i++)
                        {
                            move.EmptyCells.Add(_coordinateProvider.GetCoordinate(x + i, y).Key);
                        }

                        moves[x * 8 + y].Add(move);
                        a++;
                        offset++;
                    }

                    offset = 0;
                    var b = y - 1;
                    while (b > -1)
                    {
                        var cT = _coordinateProvider.GetCoordinate(x, b);
                        var move = new Move(cF, cT, moveOperation, figure);
                        for (int i = 1; i <= offset; i++)
                        {
                            move.EmptyCells.Add(_coordinateProvider.GetCoordinate(x, y - i).Key);
                        }

                        moves[x * 8 + y].Add(move);
                        b--;
                        offset++;
                    }

                    offset = 0;
                    b = y + 1;
                    while (b < 8)
                    {
                        var cT = _coordinateProvider.GetCoordinate(x, b);
                        var move = new Move(cF, cT, moveOperation, figure);
                        for (int i = 1; i <= offset; i++)
                        {
                            move.EmptyCells.Add(_coordinateProvider.GetCoordinate(x, y + i).Key);
                        }

                        moves[x * 8 + y].Add(move);
                        b++;
                        offset++;
                    }
                }
            }
        }

        private void SetHorizontalAttacks(MoveOperation moveOperation, FigureKind figure, List<MoveBase>[] moves)
        {
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    var cF = _coordinateProvider.GetCoordinate(x, y);

                    int offset = 0;
                    var a = x - 1;
                    while (a > -1)
                    {
                        var cT = _coordinateProvider.GetCoordinate(a, y);
                        var move = new Attack(cF, cT, moveOperation, figure);
                        for (int i = 1; i <= offset; i++)
                        {
                            move.EmptyCells.Add(_coordinateProvider.GetCoordinate(x - i, y).Key);
                        }

                        moves[x * 8 + y].Add(move);
                        a--;
                        offset++;
                    }

                    offset = 0;
                    a = x + 1;
                    while (a < 8)
                    {
                        var cT = _coordinateProvider.GetCoordinate(a, y);
                        var move = new Attack(cF, cT, moveOperation, figure);
                        for (int i = 1; i <= offset; i++)
                        {
                            move.EmptyCells.Add(_coordinateProvider.GetCoordinate(x + i, y).Key);
                        }

                        moves[x * 8 + y].Add(move);
                        a++;
                        offset++;
                    }

                    offset = 0;
                    var b = y - 1;
                    while (b > -1)
                    {
                        var cT = _coordinateProvider.GetCoordinate(x, b);
                        var move = new Attack(cF, cT, moveOperation, figure);
                        for (int i = 1; i <= offset; i++)
                        {
                            move.EmptyCells.Add(_coordinateProvider.GetCoordinate(x, y - i).Key);
                        }

                        moves[x * 8 + y].Add(move);
                        b--;
                        offset++;
                    }

                    offset = 0;
                    b = y + 1;
                    while (b < 8)
                    {
                        var cT = _coordinateProvider.GetCoordinate(x, b);
                        var move = new Attack(cF, cT, moveOperation, figure);
                        for (int i = 1; i <= offset; i++)
                        {
                            move.EmptyCells.Add(_coordinateProvider.GetCoordinate(x, y + i).Key);
                        }

                        moves[x * 8 + y].Add(move);
                        b++;
                        offset++;
                    }
                }
            }
        }

        private bool IsBlackUnderAttack(IBoard board, byte cell)
        {
            for (var index = 0; index < _whites.Length; index++)
            {
                var f = _whites[index];

                IEnumerable<int> positions = board.GetPositions(f);
                foreach (var position in positions)
                {
                    List<MoveWrapper> moveWrappers = _attacksTo[f][position][cell];
                    if (moveWrappers == null) continue;

                    for (var i = 0; i < moveWrappers.Count; i++)
                    {
                        if (moveWrappers[i].Move.IsLegal(board))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private bool IsWhiteUnderAttack(IBoard board, byte cell)
        {
            for (var index = 0; index < _blacks.Length; index++)
            {
                var f = _blacks[index];

                IEnumerable<int> positions = board.GetPositions(f);
                foreach (var position in positions)
                {
                    List<MoveWrapper> moveWrappers = _attacksTo[f][position][cell];
                    if (moveWrappers == null) continue;

                    for (var i = 0; i < moveWrappers.Count; i++)
                    {
                        if (moveWrappers[i].Move.IsLegal(board))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        #endregion
    }
}