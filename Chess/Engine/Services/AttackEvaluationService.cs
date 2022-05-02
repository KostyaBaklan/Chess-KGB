﻿using System;
using System.Runtime.CompilerServices;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;

namespace Engine.Services
{
    public class AttackEvaluationService : IAttackEvaluationService
    {
        private BitBoard[] _boards;
        private Phase _phase;
        private BitBoard _occupied;
        private BitBoard _to;
        private readonly PositionsList _positions;

        private readonly IEvaluationService _evaluationService;
        private readonly IMoveProvider _moveProvider;

        public AttackEvaluationService(IEvaluationService evaluationService, IMoveProvider moveProvider)
        {
            _evaluationService = evaluationService;
            _moveProvider = moveProvider;
            _positions = new PositionsList();
        }

        #region Implementation of IAttackEvaluationService

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Initialize(BitBoard[] boards, BitBoard occ, Phase phase)
        {
            _phase = phase;
            _occupied = occ;

            _boards = new BitBoard[boards.Length];
            Array.Copy(boards, _boards, _boards.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int StaticExchange(AttackBase attack)
        {
            BitBoard mayXRay = _boards[Piece.BlackPawn.AsByte()] |
                               _boards[Piece.BlackRook.AsByte()] |
                               _boards[Piece.BlackBishop.AsByte()] |
                               _boards[Piece.BlackQueen.AsByte()] |
                               _boards[Piece.WhitePawn.AsByte()] |
                               _boards[Piece.WhiteBishop.AsByte()] |
                               _boards[Piece.WhiteRook.AsByte()] |
                               _boards[Piece.WhiteQueen.AsByte()];

            _to = attack.To.AsBitBoard();
            BitBoard attackers = GetAttackers();

            AttackerBoard board = new AttackerBoard
            {
                Board = attack.From.AsBitBoard(),
                Piece = attack.Piece
            };

            var target = attack.Captured;
            var v = 0;
            bool first = true;
            while (board.Board.Any())
            {
                var value = _evaluationService.GetValue(target.AsByte(), _phase);
                if (first)
                {
                    var x = v + value;
                    if (x < 0) return x;

                    v = x;
                }
                else
                {
                    var x = v - value;
                    if (x > 0) return x;
                    v = x;
                }

                first = !first;

                attackers ^= board.Board; // reset bit in set to traverse
                _occupied ^= board.Board; // reset bit in temporary occupancy (for x-Rays)

                _boards[board.Piece.AsByte()] ^= board.Board | _to;

                if (board.Piece.IsWhite())
                {
                    if ((board.Board & mayXRay).Any())
                    {
                        attackers |= ConsiderWhiteXrays();
                    }

                    if (attackers.IsZero()) break;

                    target = board.Piece;
                    board = GetNextAttackerToWhite(attackers);
                }
                else
                {
                    if ((board.Board & mayXRay).Any())
                    {
                        attackers |= ConsiderBlackXrays();
                    }

                    if (attackers.IsZero()) break;

                    target = board.Piece;
                    board = GetNextAttackerToBlack(attackers);
                }
            }

            return v;
        }

        #endregion


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private AttackerBoard GetNextAttackerToBlack(BitBoard attackers)
        {
            var bit = attackers & _boards[0];
            if (bit.Any())
            {
                return new AttackerBoard {Board = new BitBoard(bit.Lsb()), Piece = Piece.WhitePawn};
            }

            bit = attackers & _boards[1];
            if (bit.Any())
            {
                return new AttackerBoard {Board = new BitBoard(bit.Lsb()), Piece = Piece.WhiteKnight};
            }

            bit = attackers & _boards[2];
            if (bit.Any())
            {
                return new AttackerBoard {Board = new BitBoard(bit.Lsb()), Piece = Piece.WhiteBishop};
            }

            bit = attackers & _boards[3];
            if (bit.Any())
            {
                return new AttackerBoard {Board = new BitBoard(bit.Lsb()), Piece = Piece.WhiteRook};
            }

            bit = attackers & _boards[4];
            if (bit.Any())
            {
                return new AttackerBoard {Board = new BitBoard(bit.Lsb()), Piece = Piece.WhiteQueen};
            }

            bit = attackers & _boards[5];
            if (bit.Any())
            {
                return new AttackerBoard {Board = new BitBoard(bit.Lsb()), Piece = Piece.WhiteKing};
            }

            return new AttackerBoard {Board = new BitBoard(0)};
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private AttackerBoard GetNextAttackerToWhite(BitBoard attackers)
        {
            var bit = attackers & _boards[6];
            if (bit.Any())
            {
                return new AttackerBoard {Board = new BitBoard(bit.Lsb()), Piece = Piece.BlackPawn};
            }

            bit = attackers & _boards[7];
            if (bit.Any())
            {
                return new AttackerBoard {Board = new BitBoard(bit.Lsb()), Piece = Piece.BlackKnight};
            }

            bit = attackers & _boards[8];
            if (bit.Any())
            {
                return new AttackerBoard {Board = new BitBoard(bit.Lsb()), Piece = Piece.BlackBishop};
            }

            bit = attackers & _boards[9];
            if (bit.Any())
            {
                return new AttackerBoard {Board = new BitBoard(bit.Lsb()), Piece = Piece.BlackRook};
            }

            bit = attackers & _boards[10];
            if (bit.Any())
            {
                return new AttackerBoard {Board = new BitBoard(bit.Lsb()), Piece = Piece.BlackQueen};
            }

            bit = attackers & _boards[11];
            if (bit.Any())
            {
                return new AttackerBoard {Board = new BitBoard(bit.Lsb()), Piece = Piece.BlackKing};
            }

            return new AttackerBoard {Board = new BitBoard(0)};
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private BitBoard ConsiderBlackXrays()
        {
            BitBoard bit = new BitBoard(0);

            (_boards[Piece.BlackBishop.AsByte()] | _boards[Piece.BlackQueen.AsByte()]).GetPositions(_positions);
            for (int i = 0; i < _positions.Count; i++)
            {
                if (!(_positions[i].BishopAttacks(_occupied) & _to).Any()) continue;
                bit = bit.Add(_positions[i]);
                break;
            }

            (_boards[Piece.BlackRook.AsByte()] | _boards[Piece.BlackQueen.AsByte()]).GetPositions(_positions);
            for (int i = 0; i < _positions.Count; i++)
            {
                if (!(_positions[i].RookAttacks(_occupied) & _to).Any()) continue;
                bit = bit.Add(_positions[i]);
                break;
            }

            return bit;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private BitBoard ConsiderWhiteXrays()
        {
            BitBoard bit = new BitBoard(0);

            (_boards[Piece.WhiteBishop.AsByte()] | _boards[Piece.WhiteQueen.AsByte()]).GetPositions(_positions);
            for (int i = 0; i < _positions.Count; i++)
            {
                if (!(_positions[i].BishopAttacks(_occupied) & _to).Any()) continue;
                bit = bit.Add(_positions[i]);
                break;
            }

            (_boards[Piece.WhiteRook.AsByte()] | _boards[Piece.WhiteQueen.AsByte()]).GetPositions(_positions);
            for (int i = 0; i < _positions.Count; i++)
            {
                if (!(_positions[i].RookAttacks(_occupied) & _to).Any()) continue;
                bit = bit.Add(_positions[i]);
                break;
            }

            return bit;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private BitBoard GetAttackers()
        {
            return GetWhiteAttackers() | GetBlackAttackers();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private BitBoard GetBlackAttackers()
        {
            BitBoard attackers = new BitBoard(0);

            _boards[Piece.BlackPawn.AsByte()].GetPositions(_positions);
            for (var p = 0; p < _positions.Count; p++)
            {
                var pattern = _moveProvider.GetBlackPawnAttackPattern(_positions[p]);
                if (pattern.IsSet(_to))
                {
                    attackers = attackers.Add(_positions[p]);
                }
            }

            _boards[Piece.BlackKnight.AsByte()].GetPositions(_positions);
            for (var p = 0; p < _positions.Count; p++)
            {
                var pattern = _moveProvider.GetBlackKnightAttackPattern(_positions[p]);
                if (pattern.IsSet(_to))
                {
                    attackers = attackers.Add(_positions[p]);
                }
            }

            _boards[Piece.BlackBishop.AsByte()].GetPositions(_positions);
            for (var p = 0; p < _positions.Count; p++)
            {
                var pattern = _positions[p].GetBlackBishopAttackPattern(_occupied);
                if (pattern.IsSet(_to))
                {
                    attackers = attackers.Add(_positions[p]);
                }
            }

            _boards[Piece.BlackRook.AsByte()].GetPositions(_positions);
            for (var p = 0; p < _positions.Count; p++)
            {
                var pattern = _positions[p].GetBlackRookAttackPattern(_occupied);
                if (pattern.IsSet(_to))
                {
                    attackers = attackers.Add(_positions[p]);
                }
            }

            _boards[Piece.BlackQueen.AsByte()].GetPositions(_positions);
            for (var p = 0; p < _positions.Count; p++)
            {
                var pattern = _positions[p].GetBlackQueenAttackPattern(_occupied);
                if (pattern.IsSet(_to))
                {
                    attackers = attackers.Add(_positions[p]);
                }
            }

            _boards[Piece.BlackKing.AsByte()].GetPositions(_positions);
            for (var p = 0; p < _positions.Count; p++)
            {
                var pattern = _moveProvider.GetBlackKingAttackPattern(_positions[p]);
                if (pattern.IsSet(_to))
                {
                    attackers = attackers.Add(_positions[p]);
                }
            }

            return attackers;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private BitBoard GetWhiteAttackers()
        {
            BitBoard attackers = new BitBoard(0);

            _boards[Piece.WhitePawn.AsByte()].GetPositions(_positions);
            for (var p = 0; p < _positions.Count; p++)
            {
                var pattern = _moveProvider.GetWhitePawnAttackPattern(_positions[p]);
                if (pattern.IsSet(_to))
                {
                    attackers = attackers.Add(_positions[p]);
                }
            }

            _boards[Piece.WhiteKnight.AsByte()].GetPositions(_positions);
            for (var p = 0; p < _positions.Count; p++)
            {
                var pattern = _moveProvider.GetWhiteKnightAttackPattern(_positions[p]);
                if (pattern.IsSet(_to))
                {
                    attackers = attackers.Add(_positions[p]);
                }
            }

            _boards[Piece.WhiteBishop.AsByte()].GetPositions(_positions);
            for (var p = 0; p < _positions.Count; p++)
            {
                var pattern = _positions[p].GetWhiteBishopAttackPattern(_occupied);
                if (pattern.IsSet(_to))
                {
                    attackers = attackers.Add(_positions[p]);
                }
            }

            _boards[Piece.WhiteRook.AsByte()].GetPositions(_positions);
            for (var p = 0; p < _positions.Count; p++)
            {
                var pattern = _positions[p].GetWhiteRookAttackPattern(_occupied);
                if (pattern.IsSet(_to))
                {
                    attackers = attackers.Add(_positions[p]);
                }
            }

            _boards[Piece.WhiteQueen.AsByte()].GetPositions(_positions);
            for (var p = 0; p < _positions.Count; p++)
            {
                var pattern = _positions[p].GetWhiteQueenAttackPattern(_occupied);
                if (pattern.IsSet(_to))
                {
                    attackers = attackers.Add(_positions[p]);
                }
            }

            _boards[Piece.WhiteKing.AsByte()].GetPositions(_positions);
            for (var p = 0; p < _positions.Count; p++)
            {
                var pattern = _moveProvider.GetWhiteKingAttackPattern(_positions[p]);
                if (pattern.IsSet(_to))
                {
                    attackers = attackers.Add(_positions[p]);
                }
            }

            return attackers;
        }
    }
}
