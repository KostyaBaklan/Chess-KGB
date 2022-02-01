using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;

namespace Engine.Services
{
    public class MoveProvider : IMoveProvider
    {
        private readonly MoveBase[] _all;
        private readonly List<List<MoveBase>>[][] _moves;
        private readonly List<List<Attack>>[][] _attacks;
        private readonly List<Attack>[][][] _attacksTo;
        private readonly BitBoard[][] _attackPatterns;
        private static readonly int _squaresNumber = 64;
        private readonly int _piecesNumbers = 12;

        private readonly IEvaluationService _evaluationService;

        public MoveProvider(IEvaluationService evaluationService)
        {
            _evaluationService = evaluationService;
            _moves = new List<List<MoveBase>>[_piecesNumbers][];
            _attacks = new List<List<Attack>>[_piecesNumbers][];
            _attackPatterns = new BitBoard[_piecesNumbers][];
            _attacksTo = new List<Attack>[_piecesNumbers][][];

            foreach (var piece in Enum.GetValues(typeof(Piece)).OfType<Piece>())
            {
                _moves[piece.AsByte()] = new List<List<MoveBase>>[_squaresNumber];
                _attacks[piece.AsByte()] = new List<List<Attack>>[_squaresNumber];
                _attackPatterns[piece.AsByte()] = new BitBoard[_squaresNumber];
                _attacksTo[piece.AsByte()] = new List<Attack>[_squaresNumber][];
                for (int square = 0; square < _squaresNumber; square++)
                {
                    _moves[piece.AsByte()][square] = new List<List<MoveBase>>();
                    _attacks[piece.AsByte()][square] = new List<List<Attack>>();
                    _attackPatterns[piece.AsByte()][square] = new BitBoard(0);
                }

                SetMoves(piece);
                SetAttacks(piece);

                for (int i = 0; i < _squaresNumber; i++)
                {
                    Dictionary<byte, List<Attack>> attacksTo = _attacks[piece.AsByte()][i].SelectMany(m => m)
                        .GroupBy(g => g.To.AsByte())
                        .ToDictionary(key => key.Key, v => v.ToList());
                    List<Attack>[] aTo = new List<Attack>[_squaresNumber];
                    for (byte q = 0; q < aTo.Length; q++)
                    {
                        if (attacksTo.TryGetValue(q, out var list))
                        {
                            aTo[q] = list;
                        }
                    }

                    _attacksTo[piece.AsByte()][i] = aTo;
                }
            }

            foreach (var piece in Enum.GetValues(typeof(Piece)).OfType<Piece>())
            {
                SetAttackPatterns(piece);
            }

            SetValues();

            List<MoveBase> all = new List<MoveBase>();
            for (var i = 0; i < _attacks.Length; i++)
            {
                for (var j = 0; j < _attacks[i].Length; j++)
                {
                    for (var k = 0; k < _attacks[i][j].Count; k++)
                    {
                        foreach (var attack in _attacks[i][j][k])
                        {
                            all.Add(attack);
                        }
                    }
                }
            }

            for (var i = 0; i < _moves.Length; i++)
            {
                for (var j = 0; j < _moves[i].Length; j++)
                {
                    for (var k = 0; k < _moves[i][j].Count; k++)
                    {
                        foreach (var move in _moves[i][j][k])
                        {
                            all.Add(move);
                        }
                    }
                }
            }

            _all = all.ToArray();
            for (var i = 0; i < _all.Length; i++)
            {
                _all[i].Key = (short) i;
            }
        }

        #region Private

        private void SetValues()
        {
            foreach (var move in from lists in _moves from list in lists from moves in list from move in moves select move)
            {
                SetValueForMove(move);
            }
        }

        private void SetValueForMove(MoveBase move)
        {
            var value = _evaluationService.GetValue(move.Piece.AsByte(), move.To.AsByte(), Phase.Opening);
            move.Difference = value - _evaluationService.GetValue(move.Piece.AsByte(), move.From.AsByte(), Phase.Opening);
        }

        private void SetAttackPatterns(Piece piece)
        {
            foreach (List<List<Attack>> attacks in _attacks[piece.AsByte()])
            {
                if (attacks == null) continue;

                foreach (var moves in attacks)
                {
                    foreach (var move in moves)
                    {
                        _attackPatterns[piece.AsByte()][move.From.AsByte()] = _attackPatterns[piece.AsByte()][move.From.AsByte()] | move.EmptyBoard|move.To.AsBitBoard();
                    }
                }
            }
        }

        private void SetAttacks(Piece piece)
        {
            switch (piece)
            {
                case Piece.WhitePawn:
                    SetWhitePawnAttacks();
                    break;
                case Piece.WhiteKnight:
                    SetWhiteKnightAttacks();
                    break;
                case Piece.WhiteBishop:
                    SetWhiteBishopAttacks();
                    break;
                case Piece.WhiteRook:
                    SetWhiteRookAttacks();
                    break;
                case Piece.WhiteKing:
                    SetWhiteKingAttacks();
                    break;
                case Piece.WhiteQueen:
                    SetWhiteQueenAttacks();
                    break;
                case Piece.BlackPawn:
                    SetBlackPawnAttacks();
                    break;
                case Piece.BlackKnight:
                    SetBlackKnightAttacks();
                    break;
                case Piece.BlackBishop:
                    SetBlackBishopAttacks();
                    break;
                case Piece.BlackRook:
                    SetBlackRookAttacks();
                    break;
                case Piece.BlackKing:
                    SetBlackKingAttacks();
                    break;
                case Piece.BlackQueen:
                    SetBlackQueenAttacks();
                    break;
            }
        }

        private void SetMoves(Piece piece)
        {
            switch (piece)
            {
                case Piece.WhitePawn:
                    SetWhitePawnMoves();
                    break;
                case Piece.WhiteKnight:
                    SetWhiteKnightMoves();
                    break;
                case Piece.WhiteBishop:
                    SetMovesWhiteBishop();
                    break;
                case Piece.WhiteRook:
                    SetWhiteRookMoves();
                    break;
                case Piece.WhiteKing:
                    SetWhiteKingMoves();
                    break;
                case Piece.WhiteQueen:
                    SetWhiteQueenMoves();
                    break;
                case Piece.BlackPawn:
                    SetBlackPawnMoves();
                    break;
                case Piece.BlackKnight:
                    SetBlackKnightMoves();
                    break;
                case Piece.BlackBishop:
                    SetBlackBishopMoves();
                    break;
                case Piece.BlackRook:
                    SetBlackRookMoves();
                    break;
                case Piece.BlackKing:
                    SetBlackKingMoves();
                    break;
                case Piece.BlackQueen:
                    SetBlackQueenMoves();
                    break;
            }
        }

        #region Queens

        private void SetBlackQueenAttacks()
        {
            var piece = Piece.BlackQueen;
            var moves = _attacks[(int)piece];
            SetStrightAttacks(piece, MoveType.EatByQueen, moves);
            SetDiagonalAttacks(piece, MoveType.EatByQueen, moves);
        }

        private void SetWhiteQueenAttacks()
        {
            var piece = Piece.WhiteQueen;
            var moves = _attacks[(int)piece];
            SetStrightAttacks(piece, MoveType.EatByQueen, moves);
            SetDiagonalAttacks(piece, MoveType.EatByQueen, moves);
        }

        private void SetBlackQueenMoves()
        {
            var piece = Piece.BlackQueen;
            var moves = _moves[(int)piece];
            SetDiagonalMoves(piece, MoveType.MoveQueen, moves);
            SetStrightMoves(piece, MoveType.MoveQueen, moves);
        }

        private void SetWhiteQueenMoves()
        {
            var piece = Piece.WhiteQueen;
            var moves = _moves[(int)piece];
            SetDiagonalMoves(piece, MoveType.MoveQueen, moves);
            SetStrightMoves(piece, MoveType.MoveQueen, moves);
        }

        #endregion

        #region Rooks

        private void SetBlackRookAttacks()
        {
            var piece = Piece.BlackRook;
            var moves = _attacks[(int)piece];
            SetStrightAttacks(piece, MoveType.EatByRook, moves);
        }

        private void SetWhiteRookAttacks()
        {
            var piece = Piece.WhiteRook;
            var moves = _attacks[(int)piece];
            SetStrightAttacks(piece, MoveType.EatByRook, moves);
        }

        private void SetBlackRookMoves()
        {
            var piece = Piece.BlackRook;
            var moves = _moves[(int)piece];
            SetStrightMoves(piece, MoveType.MoveRook, moves);
        }

        private void SetWhiteRookMoves()
        {
            var piece = Piece.WhiteRook;
            var moves = _moves[(int)piece];
            SetStrightMoves(piece, MoveType.MoveRook, moves);
        }

        #endregion

        #region Bishops

        private void SetBlackBishopAttacks()
        {
            var piece = Piece.BlackBishop;
            var moves = _attacks[(int)piece];
            SetDiagonalAttacks(piece, MoveType.EatByBishop, moves);
        }

        private void SetWhiteBishopAttacks()
        {
            var piece = Piece.WhiteBishop;
            var moves = _attacks[(int)piece];
            SetDiagonalAttacks(piece, MoveType.EatByBishop, moves);
        }

        private void SetBlackBishopMoves()
        {
            var piece = Piece.BlackBishop;
            var moves = _moves[(int) piece];
            SetDiagonalMoves(piece, MoveType.MoveBishop, moves);
        }

        private void SetMovesWhiteBishop()
        {
            var piece = Piece.WhiteBishop;
            var moves = _moves[(int)piece];
            SetDiagonalMoves(piece, MoveType.MoveBishop, moves);
        }

        #endregion

        #region Kings

        private void SetBlackKingAttacks()
        {
            var figure = Piece.BlackKing;
            var type = MoveType.EatByKing;
            var moves = _attacks[(int)figure];

            for (int from = 0; from < _squaresNumber; from++)
            {
                foreach (int to in KingMoves(from).Where(IsIn))
                {
                    var move = new SimpleAttack
                    { From = new Square(from), To = new Square(to), Piece = figure, Type = type };
                    moves[from].Add(new List<Attack> {move});
                }
            }
        }

        private void SetWhiteKingAttacks()
        {
            var figure = Piece.WhiteKing;
            var type = MoveType.EatByKing;
            var moves = _attacks[(int)figure];

            for (int from = 0; from < _squaresNumber; from++)
            {
                foreach (int to in KingMoves(from).Where(IsIn))
                {
                    var move = new SimpleAttack
                    { From = new Square(from), To = new Square(to), Piece = figure, Type = type };
                    moves[from].Add(new List<Attack> { move});
                }
            }
        }

        private void SetBlackKingMoves()
        {
            var figure = Piece.BlackKing;
            var type = MoveType.MoveKing;
            var moves = _moves[(int)figure];

            var small = new SmallCastle
            { From = new Square(60), To = new Square(62), Piece = figure };
            small.Set(61, 62);
            moves[60].Add(new List<MoveBase>{small});

            var big = new BigCastle
            { From = new Square(60), To = new Square(58), Piece = figure };
            big.Set(58, 59);
            moves[60].Add(new List<MoveBase> { big});

            for (int from = 0; from < _squaresNumber; from++)
            {
                foreach (int to in KingMoves(from).Where(IsIn))
                {
                    var move = new Move
                    { From = new Square(from), To = new Square(to), Piece = figure, Type = type };
                    move.Set(to);
                    moves[from].Add(new List<MoveBase> { move});
                }
            }
        }

        private void SetWhiteKingMoves()
        {
            var figure = Piece.WhiteKing;
            var type = MoveType.MoveKing;
            var moves = _moves[(int)figure];

            var small = new SmallCastle
            { From = new Square(4), To = new Square(6), Piece = figure };
            small.Set(5, 6);
            moves[4].Add(new List<MoveBase> { small});

            var big = new BigCastle
            { From = new Square(4), To = new Square(2), Piece = figure };
            big.Set(2, 3);
            moves[4].Add(new List<MoveBase> { big});

            for (int from = 0; from < _squaresNumber; from++)
            {
                foreach (int to in KingMoves(from).Where(IsIn))
                {
                    var move = new Move
                    { From = new Square(from), To = new Square(to), Piece = figure, Type = type };
                    move.Set(to);
                    moves[from].Add(new List<MoveBase> { move});
                }
            }
        }

        private IEnumerable<int> KingMoves(int f)
        {
            if (f == 0)
            {
                return new[] {1, 9, 8};
            }

            if (f == 7)
            {
                return new[] { 6, 14, 15 };
            }
            if (f == 56)
            {
                return new[] { 48, 49, 57 };
            }
            if (f == 63)
            {
                return new[] { 62, 54, 55 };
            }

            if (f % 8 == 0) //B1 => A1,C1,B2,A2,C2
            {
                return new[] { f+8, f+9, f+1, f-7,f-8 };
            }
            if (f % 8 == 7)//B8 => A8,C8,B7,A7,C7
            {
                return new[] { f + 8, f + 7, f - 1, f - 9, f - 8 };
            }

            if (f / 8 == 0)
            {
                return new[] { f + 1, f -1, f + 7, f + 9, f + 8 };
            }
            if (f / 8 == 7)
            {
                return new[] { f + 1, f - 7, f - 1, f - 9, f - 8 };
            }

            return new[] { f + 8, f + 7, f - 1, f + 9, f + 1, f - 9, f - 7, f - 8 };
        }

        #endregion

        #region Knights

        private void SetBlackKnightAttacks()
        {
            var figure = Piece.BlackKnight;
            var type = MoveType.EatByKnight;
            var moves = _attacks[(int)figure];

            for (int from = 0; from < _squaresNumber; from++)
            {
                foreach (var to in KnightMoves(from).Where(IsIn))
                {
                    var move = new SimpleAttack
                    { From = new Square(from), To = new Square(to), Piece = figure, Type = type };
                    moves[from].Add(new List<Attack> { move});
                }
            }
        }

        private void SetWhiteKnightAttacks()
        {
            var figure = Piece.WhiteKnight;
            var type = MoveType.EatByKnight;
            var moves = _attacks[(int)figure];

            for (int from = 0; from < _squaresNumber; from++)
            {
                foreach (var to in KnightMoves(from).Where(IsIn))
                {
                    var move = new SimpleAttack
                    { From = new Square(from), To = new Square(to), Piece = figure, Type = type };
                    moves[from].Add(new List<Attack> { move});
                }
            }
        }

        private void SetBlackKnightMoves()
        {
            var figure = Piece.BlackKnight;
            var type = MoveType.MoveKnight;
            var moves = _moves[(int)figure];

            for (int from = 0; from < _squaresNumber; from++)
            {
                foreach (var to in KnightMoves(from).Where(IsIn))
                {
                    var move = new Move
                    { From = new Square(from), To = new Square(to), Piece = figure, Type = type };
                    move.Set(to);
                    moves[from].Add(new List<MoveBase> { move});
                }
            }
        }

        private void SetWhiteKnightMoves()
        {
            var figure = Piece.WhiteKnight;
            var type = MoveType.MoveKnight;
            var moves = _moves[(int) figure];

            for (int from = 0; from < _squaresNumber; from++)
            {
                foreach (var to in KnightMoves(from).Where(IsIn))
                {
                    var move = new Move
                        { From = new Square(from), To = new Square(to), Piece = figure, Type = type };
                    move.Set(to);
                    moves[from].Add(new List<MoveBase> { move});
                }
            }
        }

        private static IEnumerable<int> KnightMoves(int i)
        {
            if (i / 8 - 1 == (i - 10) / 8)
            {
                yield return i - 10;
            }
            if (i / 8 - 2 == (i - 17) / 8)
            {
                yield return i - 17;
            }
            if (i / 8 + 1 == (i + 6) / 8)
            {
                yield return i + 6;
            }
            if (i / 8 + 1 == (i + 10) / 8)
            {
                yield return i + 10;
            }
            if (i / 8 - 1 == (i - 6) / 8)
            {
                yield return i - 6;
            }
            if (i / 8 - 2 == (i - 15) / 8)
            {
                yield return i - 15;
            }
            if (i / 8 + 2 == (i + 15) / 8)
            {
                yield return i + 15;
            }
            if (i / 8 + 2 == (i + 17) / 8)
            {
                yield return i + 17;
            }
        }

        #endregion

        #region Pawns

        private void SetBlackPawnAttacks()
        {
            var figure = Piece.BlackPawn;
            var moves = _attacks[(int)figure];

            for (int i = 16; i < 56; i++)
            {
                int x = i % 8;

                if (x < 7)
                {
                    var a1 = new SimpleAttack
                    {
                        From = new Square(i),
                        To = new Square(i - 7),
                        Piece = figure,
                        Type = MoveType.EatByPawn
                    };
                    moves[i].Add(new List<Attack> { a1});
                }

                if (x > 0)
                {
                    var a2 = new SimpleAttack
                    {
                        From = new Square(i),
                        To = new Square(i - 9),
                        Piece = figure,
                        Type = MoveType.EatByPawn
                    };
                    moves[i].Add(new List<Attack> { a2});
                }
            }

            for (int i = 8; i < 16; i++)
            {
                var listLeft = new List<Attack>(4);
                var listRight = new List<Attack>(4);
                Dictionary<MoveType, Piece> types = new Dictionary<MoveType, Piece>
                {
                    { MoveType.PawnPromotionToQueen,Piece.BlackQueen},
                    { MoveType.PawnPromotionToRook,Piece.BlackRook},
                    { MoveType.PawnPromotionToBishop,Piece.BlackBishop},
                    { MoveType.PawnPromotionToKnight,Piece.BlackKnight}
                };
                foreach (var type in types)
                {
                    if (i < 15)
                    {
                        var a1 = new PromotionAttack
                        {
                            From = new Square(i),
                            To = new Square(i - 7),
                            Piece = figure,
                            Type = type.Key,
                            PromotionPiece = type.Value
                        };
                        listLeft.Add(a1);
                    }

                    if (i > 8)
                    {
                        var a2 = new PromotionAttack
                        {
                            From = new Square(i),
                            To = new Square(i - 9),
                            Piece = figure,
                            Type = type.Key,
                            PromotionPiece = type.Value
                        };
                        listRight.Add(a2);
                    }
                }

                moves[i].Add(listLeft);
                moves[i].Add(listRight);
            }

            for (int i = 24; i < 32; i++)
            {
                if (i < 31)
                {
                    var a1 = new PawnOverAttack
                    {
                        From = new Square(i),
                        To = new Square(i - 7),
                        Piece = figure,
                        Victim = Piece.WhitePawn,
                        VictimSquare = i + 1
                    };
                    moves[i].Add(new List<Attack> { a1});
                }

                if (i > 24)
                {
                    var a2 = new PawnOverAttack
                    {
                        From = new Square(i),
                        To = new Square(i - 9),
                        Piece = figure,
                        Victim = Piece.WhitePawn,
                        VictimSquare = i - 1
                    };
                    moves[i].Add(new List<Attack> { a2});
                }
            }
        }

        private void SetWhitePawnAttacks()
        {
            var figure = Piece.WhitePawn;
            var moves = _attacks[(int)figure];

            for (int i = 8; i < 48; i++)
            {
                int x = i % 8;

                if (x > 0)
                {
                    var a1 = new SimpleAttack
                    {
                        From = new Square(i),
                        To = new Square(i + 7),
                        Piece = figure,
                        Type = MoveType.EatByPawn
                    };
                    moves[i].Add(new List<Attack> { a1});
                }

                if (x < 7)
                {
                    var a2 = new SimpleAttack
                    {
                        From = new Square(i),
                        To = new Square(i + 9),
                        Piece = figure,
                        Type = MoveType.EatByPawn
                    };
                    moves[i].Add(new List<Attack> { a2});
                }
            }

            for (int i = 48; i < 56; i++)
            {
                var listLeft = new List<Attack>(4);
                var listRight = new List<Attack>(4);
                Dictionary<MoveType, Piece> types = new Dictionary<MoveType, Piece>
                {
                    { MoveType.PawnPromotionToQueen,Piece.WhiteQueen},
                    { MoveType.PawnPromotionToRook,Piece.WhiteRook},
                    { MoveType.PawnPromotionToBishop,Piece.WhiteBishop},
                    { MoveType.PawnPromotionToKnight,Piece.WhiteKnight}
                };
                foreach (var type in types)
                {
                    if (i > 48)
                    {
                        var a1 = new PromotionAttack
                        {
                            From = new Square(i),
                            To = new Square(i + 7),
                            Piece = figure,
                            Type = type.Key,PromotionPiece = type.Value
                        };
                        listLeft.Add(a1);
                    }

                    if (i < 55)
                    {
                        var a2 = new PromotionAttack
                        {
                            From = new Square(i),
                            To = new Square(i + 9),
                            Piece = figure,
                            Type = type.Key,
                            PromotionPiece = type.Value
                        };
                        listRight.Add(a2);
                    }
                }

                moves[i].Add(listLeft);
                moves[i].Add(listRight);
            }

            for (int i = 32; i < 40; i++)
            {
                if (i > 32)
                {
                    var a1 = new PawnOverAttack
                    {
                        From = new Square(i),
                        To = new Square(i + 7),
                        Piece = figure,
                        Victim = Piece.BlackPawn,
                        VictimSquare = i - 1
                    };
                    moves[i].Add(new List<Attack> { a1});
                }

                if (i < 39)
                {
                    var a2 = new PawnOverAttack
                    {
                        From = new Square(i),
                        To = new Square(i + 9),
                        Piece = figure,
                        Victim = Piece.BlackPawn,
                        VictimSquare = i + 1
                    };
                    moves[i].Add(new List<Attack> { a2});
                }
            }
        }

        private void SetBlackPawnMoves()
        {
            var figure = Piece.BlackPawn;
            var moves = _moves[(int)figure];
            for (int i = 48; i < 56; i++)
            {
                var move = new PawnOverMove
                    { From = new Square(i), To = new Square(i - 16), Piece = figure, Type = MoveType.MovePawn };
                move.Set(i - 8, i - 16);
                moves[i].Add(new List<MoveBase> { move});
            }

            for (int i = 8; i < 16; i++)
            {
                var list = new List<MoveBase>(4);
                Dictionary<MoveType,Piece> types = new Dictionary<MoveType, Piece>
                {
                    { MoveType.PawnPromotionToQueen,Piece.BlackQueen},
                    { MoveType.PawnPromotionToRook,Piece.BlackRook},
                    { MoveType.PawnPromotionToBishop,Piece.BlackBishop},
                    { MoveType.PawnPromotionToKnight,Piece.BlackKnight}
                };
                foreach (var type in types)
                {
                    var move = new PromotionMove
                    {
                        From = new Square(i),
                        To = new Square(i - 8),
                        Piece = figure,
                        Type = type.Key,
                        PromotionPiece = type.Value
                    };


                    move.Set(i - 8);
                    list.Add(move);
                }
                moves[i].Add(list);
            }

            for (int i = 16; i < 56; i++)
            {
                var move = new Move
                    { From = new Square(i), To = new Square(i - 8), Piece = figure, Type = MoveType.MovePawn };
                move.Set(i - 8);
                moves[i].Add(new List<MoveBase> { move});
            }
        }

        private void SetWhitePawnMoves()
        {
            var figure = Piece.WhitePawn;
            var moves = _moves[(int)figure];
            for (int i = 8; i < 16; i++)
            {
                var move = new PawnOverMove
                { From = new Square(i), To = new Square(i + 16), Piece = figure, Type = MoveType.MovePawn };
                move.Set(i + 8, i + 16);
                moves[i].Add(new List<MoveBase> { move});
            }

            for (int i = 48; i < 56; i++)
            {
                var list = new List<MoveBase>(4);
                Dictionary<MoveType, Piece> types = new Dictionary<MoveType, Piece>
                {
                    { MoveType.PawnPromotionToQueen,Piece.WhiteQueen},
                    { MoveType.PawnPromotionToRook,Piece.WhiteRook},
                    { MoveType.PawnPromotionToBishop,Piece.WhiteBishop},
                    { MoveType.PawnPromotionToKnight,Piece.WhiteKnight}
                };
                foreach (var type in types)
                {
                    var move = new PromotionMove
                    {
                        From = new Square(i),
                        To = new Square(i + 8),
                        Piece = figure,
                        Type = type.Key,
                        PromotionPiece = type.Value
                    };
                    move.Set(i + 8);
                    list.Add(move);
                }
                moves[i].Add(list);
            }

            for (int i = 8; i < 48; i++)
            {
                var move = new Move
                { From = new Square(i), To = new Square(i + 8), Piece = figure, Type = MoveType.MovePawn };
                move.Set(i + 8);
                moves[i].Add(new List<MoveBase> { move});
            }
        }

        #endregion

        private static void SetStrightMoves(Piece piece, MoveType type, List<List<MoveBase>>[] moves)
        {
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    var cF = y*8+x;

                    var l = new List<MoveBase>();
                    int offset = 1;
                    var a = x - 1;
                    while (a > -1)
                    {
                        var cT = y*8+a;
                        var move = new Move { From = new Square(cF), To = new Square(cT), Piece = piece, Type = type };
                        for (int i = 1; i <= offset; i++)
                        {
                            move.Set(y*8+x-i);
                        }

                        l.Add(move);
                        a--;
                        offset++;
                    }
                    moves[cF].Add(l);

                    l = new List<MoveBase>();
                    offset = 1;
                    a = x + 1;
                    while (a < 8)
                    {
                        var cT = y * 8 + a;
                        var move = new Move { From = new Square(cF), To = new Square(cT), Piece = piece, Type = type };
                        for (int i = 1; i <= offset; i++)
                        {
                            move.Set(y * 8 + x + i);
                        }

                        l.Add(move);
                        a++;
                        offset++;
                    }
                    moves[cF].Add(l);

                    l = new List<MoveBase>();
                    offset = 1;
                    var b = y - 1;
                    while (b > -1)
                    {
                        var cT = b * 8 + x;
                        var move = new Move { From = new Square(cF), To = new Square(cT), Piece = piece, Type = type };
                        for (int i = 1; i <= offset; i++)
                        {
                            move.Set((y-i) * 8 + x);
                        }

                        l.Add(move);
                        b--;
                        offset++;
                    }
                    moves[cF].Add(l);

                    l = new List<MoveBase>();
                    offset = 1;
                    b = y + 1;
                    while (b < 8)
                    {
                        var cT = b * 8 + x;
                        var move = new Move { From = new Square(cF), To = new Square(cT), Piece = piece, Type = type };
                        for (int i = 1; i <= offset; i++)
                        {
                            move.Set((y + i) * 8 + x);
                        }

                        l.Add(move);
                        b++;
                        offset++;
                    }
                    moves[cF].Add(l);
                }
            }
        }

        private static void SetStrightAttacks(Piece piece, MoveType type, List<List<Attack>>[] moves)
        {
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    var cF = y * 8 + x;

                    var l = new List<Attack>();
                    int offset = 1;
                    var a = x - 1;
                    while (a > -1)
                    {
                        var cT = y * 8 + a;
                        var move = new Attack { From = new Square(cF), To = new Square(cT), Piece = piece, Type = type };
                        for (int i = 1; i < offset; i++)
                        {
                            move.Set(y * 8 + x - i);
                        }

                        l.Add(move);
                        a--;
                        offset++;
                    }
                    moves[cF].Add(l);

                    l = new List<Attack>();
                    offset = 1;
                    a = x + 1;
                    while (a < 8)
                    {
                        var cT = y * 8 + a;
                        var move = new Attack { From = new Square(cF), To = new Square(cT), Piece = piece, Type = type };
                        for (int i = 1; i < offset; i++)
                        {
                            move.Set(y * 8 + x + i);
                        }

                        l.Add(move);
                        a++;
                        offset++;
                    }
                    moves[cF].Add(l);


                    l = new List<Attack>();
                    offset = 1;
                    var b = y - 1;
                    while (b > -1)
                    {
                        var cT = b * 8 + x;
                        var move = new Attack { From = new Square(cF), To = new Square(cT), Piece = piece, Type = type };
                        for (int i = 1; i < offset; i++)
                        {
                            move.Set((y - i) * 8 + x);
                        }

                        l.Add(move);
                        b--;
                        offset++;
                    }
                    moves[cF].Add(l);

                    l = new List<Attack>();
                    offset = 1;
                    b = y + 1;
                    while (b < 8)
                    {
                        var cT = b * 8 + x;
                        var move = new Attack { From = new Square(cF), To = new Square(cT), Piece = piece, Type = type };
                        for (int i = 1; i < offset; i++)
                        {
                            move.Set((y + i) * 8 + x);
                        }

                        l.Add(move);
                        b++;
                        offset++;
                    }
                    moves[cF].Add(l);
                }
            }
        }

        private static void SetDiagonalMoves(Piece piece, MoveType type, List<List<MoveBase>>[] moves)
        {
            for (int i = 0; i < _squaresNumber; i++)
            {
                int x = i % 8;
                int y = i / 8;

                int a = x + 1;
                int b = y + 1;

                var l = new List<MoveBase>();
                int to = i + 9;
                while (to < _squaresNumber && a < 8 && b < 8)
                {
                    var m = new Move { From = new Square(i), To = new Square(to), Piece = piece, Type = type };
                    for (int j = i + 9; j <= to; j += 9)
                    {
                        m.Set(j);
                    }
                    l.Add(m);
                    to += 9;
                    a++;
                    b++;
                }
                moves[i].Add(l);

                l = new List<MoveBase>();
                a = x - 1;
                b = y + 1;
                to = i + 7;
                while (to < _squaresNumber&& a > -1 && b < 8)
                {
                    var m = new Move { From = new Square(i), To = new Square(to), Piece = piece, Type = type };
                    for (int j = i + 7; j <= to; j += 7)
                    {
                        m.Set(j);
                    }
                    l.Add(m);

                    to += 7;
                    a--;
                    b++;
                }
                moves[i].Add(l);

                l = new List<MoveBase>();
                a = x + 1;
                b = y - 1;
                to = i - 7;
                while (to > -1 && a < 8 && b > -1)
                {
                    var m = new Move { From = new Square(i), To = new Square(to), Piece = piece, Type = type };
                    for (int j = i - 7; j >= to; j -= 7)
                    {
                        m.Set(j);
                    }
                    l.Add(m);

                    to -= 7;
                    a++;
                    b--;
                }
                moves[i].Add(l);


                l = new List<MoveBase>();
                a = x - 1;
                b = y - 1;
                to = i - 9;
                while (to > -1 && a > -1 && b > -1)
                {
                    var m = new Move { From = new Square(i), To = new Square(to), Piece = piece, Type = type };
                    for (int j = i - 9; j >= to; j -= 9)
                    {
                        m.Set(j);
                    }
                    l.Add(m);

                    to -= 9;
                    a--;
                    b--;
                }
                moves[i].Add(l);
            }
        }

        private static void SetDiagonalAttacks(Piece piece, MoveType type, List<List<Attack>>[] moves)
        {
            for (int i = 0; i < _squaresNumber; i++)
            {
                int x = i % 8;
                int y = i / 8;

                int a = x + 1;
                int b = y + 1;

                var l = new List<Attack>();
                int to = i + 9;
                while (to < _squaresNumber && a < 8 && b < 8)
                {
                    var m = new Attack { From = new Square(i), To = new Square(to), Piece = piece, Type = type };
                    for (int j = i + 9; j < to; j += 9)
                    {
                        m.Set(j);
                    }

                    l.Add(m);
                    to += 9;
                    a++;
                    b++;
                }
                moves[i].Add(l);

                l = new List<Attack>();
                a = x - 1;
                b = y + 1;
                to = i + 7;
                while (to < _squaresNumber && a > -1 && b < 8)
                {
                    var m = new Attack { From = new Square(i), To = new Square(to), Piece = piece, Type = type };
                    for (int j = i + 7; j < to; j += 7)
                    {
                        m.Set(j);
                    }

                    l.Add(m);

                    to += 7;
                    a--;
                    b++;
                }
                moves[i].Add(l);

                l = new List<Attack>();
                a = x + 1;
                b = y - 1;
                to = i - 7;
                while (to > -1 && a < 8 && b > -1)
                {
                    var m = new Attack { From = new Square(i), To = new Square(to), Piece = piece, Type = type };
                    for (int j = i - 7; j > to; j -= 7)
                    {
                        m.Set(j);
                    }

                    l.Add(m);

                    to -= 7;
                    a++;
                    b--;
                }
                moves[i].Add(l);

                l = new List<Attack>();
                a = x - 1;
                b = y - 1;
                to = i - 9;
                while (to > -1 && a > -1 && b > -1)
                {
                    var m = new Attack { From = new Square(i), To = new Square(to), Piece = piece, Type = type };
                    for (int j = i - 9; j > to; j -= 9)
                    {
                        m.Set(j);
                    }

                    l.Add(m);

                    to -= 9;
                    a--;
                    b--;
                }
                moves[i].Add(l);
            }
        }

        private bool IsIn(int i)
        {
            return i > -1 && i < 64;
        }

        #endregion

        #region Implementation of IMoveProvider

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IMove Get(short key)
        {
            return _all[key];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<IMove> GetAll()
        {
            return _all;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<IAttack> GetAttacks(Piece piece, Square cell, IBoard board)
        {
            var lists = _attacks[piece.AsByte()][cell.AsByte()];
            for (var i = 0; i < lists.Count; i++)
            {
                var list = lists[i];
                for (var j = 0; j < list.Count; j++)
                {
                    var m = list[j];
                    if (m.IsLegal(board))
                        yield return m;

                    else if (!board.IsEmpty(m.EmptyBoard))
                    {
                        break;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<IAttack> GetAttacks(Piece piece, int @from, IBoard board)
        {
            var lists = _attacks[piece.AsByte()][from];
            for (var i = 0; i < lists.Count; i++)
            {
                var list = lists[i];
                for (var j = 0; j < list.Count; j++)
                {
                    var m = list[j];
                    if (m.IsLegal(board))
                        yield return m;

                    else if (!board.IsEmpty(m.EmptyBoard))
                    {
                        break;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<IAttack> GetAttacks(Piece piece, Square @from, int to)
        {
            return _attacksTo[piece.AsByte()][@from.AsByte()][to];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<IAttack> GetAttacks(Piece piece, int @from, int to)
        {
            return _attacksTo[piece.AsByte()][from][to];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<IMove> GetMoves(Piece piece, Square cell, IBoard board)
        {
            var lists = _moves[piece.AsByte()][cell.AsByte()];
            for (var i = 0; i < lists.Count; i++)
            {
                var list = lists[i];
                for (var j = 0; j < list.Count; j++)
                {
                    var m = list[j];
                    if (m.IsLegal(board))
                        yield return m;
                    else
                    {
                        break;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AnyBlackCheck(IBoard board)
        {
            var kingPosition = board.GetWhiteKingPosition();

            return  IsWhiteUnderAttack(board, kingPosition);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AnyWhiteCheck(IBoard board)
        {
            var kingPosition = board.GetBlackKingPosition();

            return  IsBlackUnderAttack(board, kingPosition);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsWhiteUnderAttack(IBoard board, Square square)
        {
            var to = square.AsByte();
            return IsUnderAttack(board, Piece.BlackBishop.AsByte(), to) ||
                   IsUnderAttack(board, Piece.BlackKnight.AsByte(), to) ||
                   IsUnderAttack(board, Piece.BlackQueen.AsByte(), to) ||
                   IsUnderAttack(board, Piece.BlackRook.AsByte(), to) ||
                   IsUnderAttack(board, Piece.BlackPawn.AsByte(), to) ||
                   IsUnderAttack(board, Piece.BlackKing.AsByte(), to);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBlackUnderAttack(IBoard board, Square square)
        {
            var to = square.AsByte();
            return IsUnderAttack(board, Piece.WhiteBishop.AsByte(), to) ||
                   IsUnderAttack(board, Piece.WhiteKnight.AsByte(), to) ||
                   IsUnderAttack(board, Piece.WhiteQueen.AsByte(), to) ||
                   IsUnderAttack(board, Piece.WhiteRook.AsByte(), to) ||
                   IsUnderAttack(board, Piece.WhitePawn.AsByte(), to) ||
                   IsUnderAttack(board, Piece.WhiteKing.AsByte(), to);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsUnderAttack(IBoard board, int piece, byte to)
        {
            var positions = board.GetPositions(piece);
            for (var p = 0; p < positions.Count; p++)
            {
                var moveWrappers = _attacksTo[piece][positions[p]][to];
                if (moveWrappers == null) continue;

                for (var i = 0; i < moveWrappers.Count; i++)
                {
                    if (moveWrappers[i].IsLegalAttack(board))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard GetAttackPattern(byte piece, int position)
        {
            return _attackPatterns[piece][position];
        }

        #endregion
    }
}
