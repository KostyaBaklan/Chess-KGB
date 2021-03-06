using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Engine.DataStructures;
using Engine.DataStructures.Moves;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using Newtonsoft.Json;

namespace Engine.Services
{
    public class MoveProvider : IMoveProvider
    {
        private readonly MoveBase[] _all;
        private readonly DynamicArray<MoveList>[][] _moves;
        private readonly DynamicArray<AttackList>[][] _attacks;
        private List<List<MoveBase>>[][] _movesTemp;
        private List<List<Attack>>[][] _attacksTemp;
        private readonly Attack[][][][] _attacksTo;
        private readonly BitBoard[][] _attackPatterns;
        private static readonly int _squaresNumber = 64;
        private readonly int _piecesNumbers = 12;

        private readonly IEvaluationService _evaluationService;
        private IBoard _board;

        public MoveProvider(IEvaluationService evaluationService, IConfigurationProvider configurationProvider)
        {
            _evaluationService = evaluationService;
            _moves = new DynamicArray<MoveList>[_piecesNumbers][];
            _attacks = new DynamicArray<AttackList>[_piecesNumbers][];
            _movesTemp = new List<List<MoveBase>>[_piecesNumbers][];
            _attacksTemp = new List<List<Attack>>[_piecesNumbers][];
            _attackPatterns = new BitBoard[_piecesNumbers][];
            _attacksTo = new Attack[_piecesNumbers][][][];

            foreach (var piece in Enum.GetValues(typeof(Piece)).OfType<Piece>())
            {
                _moves[piece.AsByte()] = new DynamicArray<MoveList>[_squaresNumber];
                _attacks[piece.AsByte()] = new DynamicArray<AttackList>[_squaresNumber];
                _movesTemp[piece.AsByte()] = new List<List<MoveBase>>[_squaresNumber];
                _attacksTemp[piece.AsByte()] = new List<List<Attack>>[_squaresNumber];
                _attackPatterns[piece.AsByte()] = new BitBoard[_squaresNumber];
                _attacksTo[piece.AsByte()] = new Attack[_squaresNumber][][];
                for (int square = 0; square < _squaresNumber; square++)
                {
                    _movesTemp[piece.AsByte()][square] = new List<List<MoveBase>>();
                    _attacksTemp[piece.AsByte()][square] = new List<List<Attack>>();
                    _attackPatterns[piece.AsByte()][square] = new BitBoard(0);
                }

                SetMoves(piece);
                SetAttacks(piece);

                for (int i = 0; i < _squaresNumber; i++)
                {
                    Dictionary<byte, Attack[]> attacksTo = _attacksTemp[piece.AsByte()][i].SelectMany(m => m)
                        .GroupBy(g => g.To.AsByte())
                        .ToDictionary(key => key.Key, v => v.ToArray());
                    Attack[][] aTo = new Attack[_squaresNumber][];
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

            SetPawnAttackPatterns();

            SetValues();

            List<MoveBase> all = new List<MoveBase>();
            for (var i = 0; i < _attacksTemp.Length; i++)
            {
                for (var j = 0; j < _attacksTemp[i].Length; j++)
                {
                    for (var k = 0; k < _attacksTemp[i][j].Count; k++)
                    {
                        foreach (var attack in _attacksTemp[i][j][k])
                        {
                            all.Add(attack);
                        }
                    }
                }
            }

            for (var i = 0; i < _movesTemp.Length; i++)
            {
                for (var j = 0; j < _movesTemp[i].Length; j++)
                {
                    for (var k = 0; k < _movesTemp[i][j].Count; k++)
                    {
                        foreach (var move in _movesTemp[i][j][k])
                        {
                            all.Add(move);
                        }
                    }
                }
            }

            _all = all.ToArray();
            for (var i = 0; i < _all.Length; i++)
            {
                var move = _all[i];
                move.Key = (short) i;
                if (move.Piece == Piece.WhitePawn && move.From.AsByte() > 31)
                {
                    move.IsPassed = move.From.AsByte() > 39;
                    move.CanReduce = false;
                }
                else if (move.Piece == Piece.BlackPawn && move.From.AsByte() < 32)
                {

                    move.IsPassed = move.From.AsByte() < 24;
                    move.CanReduce = false;
                }
                else
                {
                    move.IsPassed = false;
                    move.CanReduce = !move.IsAttack && !move.IsPromotion;
                }
            }

            if (configurationProvider.GeneralConfiguration.UseHistory)
            {
                var text = File.ReadAllText(@"Config/History.json");
                var moveHistory = JsonConvert.DeserializeObject<Dictionary<short, short>>(text);
                for (var i = 0; i < _all.Length; i++)
                {
                    if (moveHistory.TryGetValue(_all[i].Key, out var history))
                    {
                        _all[i].History = history;
                    }
                } 
            }

            SetMoves();
            SetAttacks();
        }

        private void SetAttacks()
        {
            for (byte p = 0; p < _piecesNumbers; p++)
            {
                for (byte s = 0; s < _squaresNumber; s++)
                {
                    if (_attacksTemp[p][s] == null) continue;

                    var dynamicArray = new DynamicArray<AttackList>(_attacksTemp[p][s].Count);
                    for (var i = 0; i < _attacksTemp[p][s].Count; i++)
                    {
                        if (_attacksTemp[p][s][i] == null) continue;

                        AttackList moves = new AttackList(_attacksTemp[p][s][i].Count);
                        for (var j = 0; j < _attacksTemp[p][s][i].Count; j++)
                        {
                            moves.Add(_attacksTemp[p][s][i][j]);
                        }
                        dynamicArray.Add(moves);
                        _attacksTemp[p][s][i].Clear();
                        _attacksTemp[p][s][i] = null;
                    }

                    _attacksTemp[p][s].Clear();
                    _attacksTemp[p][s] = null;
                    _attacks[p][s] = dynamicArray;
                }

                Array.Clear(_attacksTemp[p], 0, _attacksTemp[p].Length);
                _attacksTemp[p] = null;
            }

            Array.Clear(_attacksTemp, 0, _attacksTemp.Length);
            _attacksTemp = null;
        }

        private void SetMoves()
        {
            for (byte p = 0; p < _piecesNumbers; p++)
            {
                for (byte s = 0; s < _squaresNumber; s++)
                {
                    if (_movesTemp[p][s] == null) continue;

                    var dynamicArray = new DynamicArray<MoveList>(_movesTemp[p][s].Count);
                    for (var i = 0; i < _movesTemp[p][s].Count; i++)
                    {
                        if (_movesTemp[p][s][i] == null) continue;

                        MoveList moves = new MoveList(_movesTemp[p][s][i].Count);
                        for (var j = 0; j < _movesTemp[p][s][i].Count; j++)
                        {
                            moves.Add(_movesTemp[p][s][i][j]);
                        }

                        dynamicArray.Add(moves);
                        _movesTemp[p][s][i].Clear();
                        _movesTemp[p][s][i] = null;
                    }

                    _movesTemp[p][s].Clear();
                    _movesTemp[p][s] = null;
                    _moves[p][s] = dynamicArray;
                }

                Array.Clear(_movesTemp[p], 0, _movesTemp[p].Length);
                _movesTemp[p] = null;
            }

            Array.Clear(_movesTemp, 0, _movesTemp.Length);
            _movesTemp = null;
        }

        private void SetPawnAttackPatterns()
        {
            _attackPatterns[Piece.WhitePawn.AsByte()][Squares.A1.AsByte()] = Squares.B2.AsBitBoard();
            _attackPatterns[Piece.WhitePawn.AsByte()][Squares.B1.AsByte()] = Squares.A2.AsBitBoard() | Squares.C2.AsBitBoard();
            _attackPatterns[Piece.WhitePawn.AsByte()][Squares.C1.AsByte()] = Squares.B2.AsBitBoard() | Squares.D2.AsBitBoard();
            _attackPatterns[Piece.WhitePawn.AsByte()][Squares.D1.AsByte()] = Squares.C2.AsBitBoard() | Squares.E2.AsBitBoard();
            _attackPatterns[Piece.WhitePawn.AsByte()][Squares.E1.AsByte()] = Squares.D2.AsBitBoard() | Squares.F2.AsBitBoard();
            _attackPatterns[Piece.WhitePawn.AsByte()][Squares.F1.AsByte()] = Squares.E2.AsBitBoard() | Squares.G2.AsBitBoard();
            _attackPatterns[Piece.WhitePawn.AsByte()][Squares.G1.AsByte()] = Squares.F2.AsBitBoard()| Squares.H2.AsBitBoard();
            _attackPatterns[Piece.WhitePawn.AsByte()][Squares.H1.AsByte()] = Squares.G2.AsBitBoard();

            _attackPatterns[Piece.BlackPawn.AsByte()][Squares.A8.AsByte()] = Squares.B7.AsBitBoard();
            _attackPatterns[Piece.BlackPawn.AsByte()][Squares.B8.AsByte()] = Squares.A7.AsBitBoard() | Squares.C7.AsBitBoard();
            _attackPatterns[Piece.BlackPawn.AsByte()][Squares.C8.AsByte()] = Squares.B7.AsBitBoard() | Squares.D7.AsBitBoard();
            _attackPatterns[Piece.BlackPawn.AsByte()][Squares.D8.AsByte()] = Squares.C7.AsBitBoard() | Squares.E7.AsBitBoard();
            _attackPatterns[Piece.BlackPawn.AsByte()][Squares.E8.AsByte()] = Squares.D7.AsBitBoard() | Squares.F7.AsBitBoard();
            _attackPatterns[Piece.BlackPawn.AsByte()][Squares.F8.AsByte()] = Squares.E7.AsBitBoard() | Squares.G7.AsBitBoard();
            _attackPatterns[Piece.BlackPawn.AsByte()][Squares.G8.AsByte()] = Squares.F7.AsBitBoard() | Squares.H7.AsBitBoard();
            _attackPatterns[Piece.BlackPawn.AsByte()][Squares.H8.AsByte()] = Squares.G7.AsBitBoard();
        }

        #region Private

        private void SetValues()
        {
            foreach (var move in from lists in _movesTemp from list in lists from moves in list from move in moves select move)
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
            foreach (List<List<Attack>> attacks in _attacksTemp[piece.AsByte()])
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
            var moves = _attacksTemp[(int)piece];
            SetStrightAttacks(piece, MoveType.EatByQueen, moves);
            SetDiagonalAttacks(piece, MoveType.EatByQueen, moves);
        }

        private void SetWhiteQueenAttacks()
        {
            var piece = Piece.WhiteQueen;
            var moves = _attacksTemp[(int)piece];
            SetStrightAttacks(piece, MoveType.EatByQueen, moves);
            SetDiagonalAttacks(piece, MoveType.EatByQueen, moves);
        }

        private void SetBlackQueenMoves()
        {
            var piece = Piece.BlackQueen;
            var moves = _movesTemp[(int)piece];
            SetDiagonalMoves(piece, MoveType.MoveQueen, moves);
            SetStrightMoves(piece, MoveType.MoveQueen, moves);
        }

        private void SetWhiteQueenMoves()
        {
            var piece = Piece.WhiteQueen;
            var moves = _movesTemp[(int)piece];
            SetDiagonalMoves(piece, MoveType.MoveQueen, moves);
            SetStrightMoves(piece, MoveType.MoveQueen, moves);
        }

        #endregion

        #region Rooks

        private void SetBlackRookAttacks()
        {
            var piece = Piece.BlackRook;
            var moves = _attacksTemp[(int)piece];
            SetStrightAttacks(piece, MoveType.EatByRook, moves);
        }

        private void SetWhiteRookAttacks()
        {
            var piece = Piece.WhiteRook;
            var moves = _attacksTemp[(int)piece];
            SetStrightAttacks(piece, MoveType.EatByRook, moves);
        }

        private void SetBlackRookMoves()
        {
            var piece = Piece.BlackRook;
            var moves = _movesTemp[(int)piece];
            SetStrightMoves(piece, MoveType.MoveRook, moves);
        }

        private void SetWhiteRookMoves()
        {
            var piece = Piece.WhiteRook;
            var moves = _movesTemp[(int)piece];
            SetStrightMoves(piece, MoveType.MoveRook, moves);
        }

        #endregion

        #region Bishops

        private void SetBlackBishopAttacks()
        {
            var piece = Piece.BlackBishop;
            var moves = _attacksTemp[(int)piece];
            SetDiagonalAttacks(piece, MoveType.EatByBishop, moves);
        }

        private void SetWhiteBishopAttacks()
        {
            var piece = Piece.WhiteBishop;
            var moves = _attacksTemp[(int)piece];
            SetDiagonalAttacks(piece, MoveType.EatByBishop, moves);
        }

        private void SetBlackBishopMoves()
        {
            var piece = Piece.BlackBishop;
            var moves = _movesTemp[(int) piece];
            SetDiagonalMoves(piece, MoveType.MoveBishop, moves);
        }

        private void SetMovesWhiteBishop()
        {
            var piece = Piece.WhiteBishop;
            var moves = _movesTemp[(int)piece];
            SetDiagonalMoves(piece, MoveType.MoveBishop, moves);
        }

        #endregion

        #region Kings

        private void SetBlackKingAttacks()
        {
            var figure = Piece.BlackKing;
            var type = MoveType.EatByKing;
            var moves = _attacksTemp[(int)figure];

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
            var moves = _attacksTemp[(int)figure];

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
            var moves = _movesTemp[(int)figure];

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
            var moves = _movesTemp[(int)figure];

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
            var moves = _attacksTemp[(int)figure];

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
            var moves = _attacksTemp[(int)figure];

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
            var moves = _movesTemp[(int)figure];

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
            var moves = _movesTemp[(int) figure];

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
            var moves = _attacksTemp[(int)figure];

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
                        VictimSquare = (byte) (i + 1)
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
                        VictimSquare = (byte) (i - 1)
                    };
                    moves[i].Add(new List<Attack> { a2});
                }
            }
        }

        private void SetWhitePawnAttacks()
        {
            var figure = Piece.WhitePawn;
            var moves = _attacksTemp[(int)figure];

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
                        VictimSquare = (byte) (i - 1)
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
                        VictimSquare = (byte) (i + 1)
                    };
                    moves[i].Add(new List<Attack> { a2});
                }
            }
        }

        private void SetBlackPawnMoves()
        {
            var figure = Piece.BlackPawn;
            var moves = _movesTemp[(int)figure];
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
            var moves = _movesTemp[(int)figure];
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

        public int MovesCount => _all.Length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MoveBase Get(short key)
        {
            return _all[key];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<MoveBase> GetAll()
        {
            return _all;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<AttackBase> GetAttacks(Piece piece, Square cell)
        {
            var lists = _attacks[piece.AsByte()][cell.AsByte()];
            for (var i = 0; i < lists.Count; i++)
            {
                var list = lists[i];
                for (var j = 0; j < list.Count; j++)
                {
                    var m = list[j];
                    if (m.IsLegal(_board))
                        yield return m;

                    else if (!_board.IsEmpty(m.EmptyBoard))
                    {
                        break;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetAttacks(byte piece, Square cell, AttackList attackList)
        {
            attackList.Clear();
            var lists = _attacks[piece][cell.AsByte()];
            for (var i = 0; i < lists.Count; i++)
            {
                var list = lists[i];
                for (var j = 0; j < list.Count; j++)
                {
                    var m = list[j];
                    if (m.IsLegal(_board))
                        attackList.Add(m);

                    else if (!_board.IsEmpty(m.EmptyBoard))
                    {
                        break;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetAttacks(Piece piece, byte @from, AttackList attackList)
        {
            attackList.Clear();
            var lists = _attacks[piece.AsByte()][from];
            for (var i = 0; i < lists.Count; i++)
            {
                var list = lists[i];
                for (var j = 0; j < list.Count; j++)
                {
                    var m = list[j];
                    if (m.IsLegal(_board))
                        attackList.Add(m);

                    else if (!_board.IsEmpty(m.EmptyBoard))
                    {
                        break;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AnyLegalAttacksTo(Piece piece, Square @from, byte to)
        {
            var attacks = _attacksTo[piece.AsByte()][@from.AsByte()][to];
            if (attacks == null) return false;

            for (var i = 0; i < attacks.Length; i++)
            {
                if (attacks[i].IsLegalAttack(_board))
                {
                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<AttackBase> GetAttacks(Piece piece, Square @from, byte to)
        {
            return _attacksTo[piece.AsByte()][@from.AsByte()][to];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<AttackBase> GetAttacks(Piece piece, byte @from, byte to)
        {
            return _attacksTo[piece.AsByte()][from][to];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<MoveBase> GetMoves(Piece piece, Square cell)
        {
            var lists = _moves[piece.AsByte()][cell.AsByte()];
            for (var i = 0; i < lists.Count; i++)
            {
                var list = lists[i];
                for (var j = 0; j < list.Count; j++)
                {
                    var m = list[j];
                    if (m.IsLegal(_board))
                        yield return m;
                    else
                    {
                        break;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetMoves(byte piece, Square cell, MoveList moveList)
        {
            moveList.Clear();
            var lists = _moves[piece][cell.AsByte()];
            for (var i = 0; i < lists.Count; i++)
            {
                var list = lists[i];
                for (var j = 0; j < list.Count; j++)
                {
                    var m = list[j];
                    if (m.IsLegal(_board))
                        moveList.Add(m);
                    else
                    {
                        break;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AnyBlackCheck()
        {
            return  IsWhiteUnderAttack(_board.GetWhiteKingPosition());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AnyWhiteCheck()
        {
            return  IsBlackUnderAttack(_board.GetBlackKingPosition());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsWhiteUnderAttack(byte to)
        {
            return IsUnderAttack(Piece.BlackBishop.AsByte(), to) ||
                   IsUnderAttack(Piece.BlackKnight.AsByte(), to) ||
                   IsUnderAttack(Piece.BlackQueen.AsByte(), to) ||
                   IsUnderAttack(Piece.BlackRook.AsByte(), to) ||
                   (_board.GetBlackPawnAttacks() & to.AsBitBoard()).Any() ||
                   IsUnderAttack(Piece.BlackKing.AsByte(), to);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBlackUnderAttack(byte to)
        {
            return IsUnderAttack(Piece.WhiteBishop.AsByte(), to) ||
                   IsUnderAttack(Piece.WhiteKnight.AsByte(), to) ||
                   IsUnderAttack(Piece.WhiteQueen.AsByte(), to) ||
                   IsUnderAttack(Piece.WhiteRook.AsByte(), to) ||
                   (_board.GetWhitePawnAttacks() & to.AsBitBoard()).Any() ||
                   IsUnderAttack(Piece.WhiteKing.AsByte(), to);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsWhiteUnderAttack(Square square)
        {
            return IsWhiteUnderAttack(square.AsByte());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBlackUnderAttack(Square square)
        {
            return IsBlackUnderAttack(square.AsByte());
        }

        public void SetBoard(IBoard b)
        {
            _board = b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AgeHistory()
        {
            for (var i = 0; i < _all.Length; i++)
            {
                if (_all[i].History > 0)
                {
                    _all[i].History /= 2;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MoveBase GetMoveTo(Piece piece, Square @from, Square to)
        {
            var lists = _moves[piece.AsByte()][@from.AsByte()];
            for (var i = 0; i < lists.Count; i++)
            {
                var list = lists[i];
                for (var j = 0; j < list.Count; j++)
                {
                    var m = list[j];

                    if (m.To == to)
                    {
                        return m; 
                    }
                }
            }

            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<MoveBase> GetPossibleMoves(Piece piece, Square cell)
        {
            var lists = _moves[piece.AsByte()][cell.AsByte()];
            for (var i = 0; i < lists.Count; i++)
            {
                var list = lists[i];
                for (var j = 0; j < list.Count; j++)
                {
                    var m = list[j];

                    yield return m;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsUnderAttack(byte piece, byte to)
        {
            var positions = _board.GetPiecePositions(piece);
            for (var p = 0; p < positions.Count; p++)
            {
                var moveWrappers = _attacksTo[piece][positions[p]][to];
                if (moveWrappers == null) continue;

                for (var i = 0; i < moveWrappers.Length; i++)
                {
                    if (moveWrappers[i].IsLegalAttack(_board))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetWhiteAttacksTo(byte to, AttackList attackList)
        {
            attackList.Clear();
            GetAttacksTo(Piece.WhitePawn.AsByte(), to, attackList);
            GetAttacksTo(Piece.WhiteKnight.AsByte(), to, attackList);
            GetAttacksTo(Piece.WhiteQueen.AsByte(), to, attackList);
            GetAttacksTo(Piece.WhiteBishop.AsByte(), to, attackList);
            GetAttacksTo(Piece.WhiteRook.AsByte(), to, attackList);
            GetAttacksTo(Piece.WhiteKing.AsByte(), to, attackList);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetBlackAttacksTo(byte to, AttackList attackList)
        {
            attackList.Clear();
            GetAttacksTo(Piece.BlackPawn.AsByte(), to, attackList);
            GetAttacksTo(Piece.BlackKnight.AsByte(), to, attackList);
            GetAttacksTo(Piece.BlackQueen.AsByte(), to, attackList);
            GetAttacksTo(Piece.BlackBishop.AsByte(), to, attackList);
            GetAttacksTo(Piece.BlackRook.AsByte(), to, attackList);
            GetAttacksTo(Piece.BlackKing.AsByte(), to, attackList);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GetAttacksTo(byte piece, byte to, AttackList attackList)
        {
            var positions = _board.GetPiecePositions(piece);
            for (var p = 0; p < positions.Count; p++)
            {
                var moveWrappers = _attacksTo[piece][positions[p]][to];
                if (moveWrappers == null) continue;

                for (var i = 0; i < moveWrappers.Length; i++)
                {
                    if (moveWrappers[i].IsLegalAttack(_board))
                    {
                        attackList.Add(moveWrappers[i]);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard GetAttackPattern(byte piece, byte position)
        {
            return _attackPatterns[piece][position];
        }

        #endregion
    }
}
