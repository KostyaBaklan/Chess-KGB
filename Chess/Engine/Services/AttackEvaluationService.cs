using System;
using System.Runtime.CompilerServices;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;

namespace Engine.Services
{
    public class AttackEvaluationService: IAttackEvaluationService
    {
        private BitBoard[] _boards;
        private Phase _phase;
        private BitBoard _occupied;
        private int[] _pieceCount;
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
        public void Initialize(BitBoard[] boards, BitBoard occ, Phase phase, int[] pieceCount)
        {
            _phase = phase;
            _occupied = occ;
            _pieceCount = pieceCount;

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
            bool positive = true;
            while (board.Board.Any())
            {
                v = positive
                    ? v + _evaluationService.GetValue(target.AsByte(), _phase)
                    : v - _evaluationService.GetValue(target.AsByte(), _phase);

                positive = !positive;

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
            for (byte i = 0; i < 6; i++)
            {
                var bit = attackers & _boards[i];
                if (bit.Any())
                {
                    return new AttackerBoard { Board = new BitBoard(bit.Lsb()), Piece = (Piece)i };
                }
            }

            return new AttackerBoard { Board = new BitBoard(0), Piece = Piece.WhitePawn };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private AttackerBoard GetNextAttackerToWhite(BitBoard attackers)
        {
            for (byte i = 6; i < 12; i++)
            {
                var bit = attackers & _boards[i];
                if (bit.Any())
                {
                    return new AttackerBoard { Board = new BitBoard(bit.Lsb()), Piece = (Piece)i };
                }
            }

            return new AttackerBoard { Board = new BitBoard(0), Piece = Piece.WhitePawn };
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
        private BitBoard ConsiderXrays( Piece piece)
        {
            BitBoard bit = new BitBoard(0);
            if (piece.IsWhite())
            {
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

                //_boards[Piece.WhiteQueen.AsByte()].GetPositions(_positions);
                //for (int i = 0; i < _positions.Count; i++)
                //{
                //    if (!(_positions[i].QueenAttacks(_occupied) & _to).Any()) continue;
                //    bit = bit.Add(_positions[i]);
                //    break;
                //}
            }
            else
            {
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

                //_boards[Piece.BlackQueen.AsByte()].GetPositions(_positions);
                //for (int i = 0; i < _positions.Count; i++)
                //{
                //    if (!(_positions[i].QueenAttacks(_occupied) & _to).Any()) continue;
                //    bit = bit.Add(_positions[i]);
                //    break;
                //}
            }

            return bit;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private AttackerBoard GetNextAttacker(BitBoard attackers, Piece piece)
        {
            if (piece.IsWhite())
            {
                for (byte i = 6; i < 12; i++)
                {
                    var bit = attackers & _boards[i];
                    if (bit.Any())
                    {
                        return new AttackerBoard { Board = new BitBoard(bit.Lsb()), Piece = (Piece)i };
                    }
                }
            }
            else
            {
                for (byte i = 0; i < 6; i++)
                {
                    var bit = attackers & _boards[i];
                    if (bit.Any())
                    {
                        return new AttackerBoard { Board = new BitBoard(bit.Lsb()), Piece = (Piece)i };
                    }
                }
            }

            return new AttackerBoard { Board = new BitBoard(0), Piece = Piece.WhitePawn };
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
            var positions = GetPositionInternal(Piece.BlackPawn.AsByte());
            for (var p = 0; p < positions.Length; p++)
            {
                var pattern = _moveProvider.GetBlackPawnAttackPattern(positions[p]);
                if (pattern.IsSet(_to))
                {
                    attackers = attackers.Add(positions[p]);
                }
            }

            positions = GetPositionInternal(Piece.BlackKnight.AsByte());
            for (var p = 0; p < positions.Length; p++)
            {
                var pattern = _moveProvider.GetBlackKnightAttackPattern(positions[p]);
                if (pattern.IsSet(_to))
                {
                    attackers = attackers.Add(positions[p]);
                }
            }

            positions = GetPositionInternal(Piece.BlackBishop.AsByte());
            for (var p = 0; p < positions.Length; p++)
            {
                var pattern = positions[p].GetBlackBishopAttackPattern(_occupied);
                if (pattern.IsSet(_to))
                {
                    attackers = attackers.Add(positions[p]);
                }
            }

            positions = GetPositionInternal(Piece.BlackRook.AsByte());
            for (var p = 0; p < positions.Length; p++)
            {
                var pattern = positions[p].GetBlackRookAttackPattern(_occupied);
                if (pattern.IsSet(_to))
                {
                    attackers = attackers.Add(positions[p]);
                }
            }

            positions = GetPositionInternal(Piece.BlackQueen.AsByte());
            for (var p = 0; p < positions.Length; p++)
            {
                var pattern = positions[p].GetBlackQueenAttackPattern(_occupied);
                if (pattern.IsSet(_to))
                {
                    attackers = attackers.Add(positions[p]);
                }
            }

            positions = GetPositionInternal(Piece.BlackKing.AsByte());
            for (var p = 0; p < positions.Length; p++)
            {
                var pattern = _moveProvider.GetBlackKingAttackPattern(positions[p]);
                if (pattern.IsSet(_to))
                {
                    attackers = attackers.Add(positions[p]);
                }
            }

            return attackers;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private BitBoard GetWhiteAttackers()
        {
            BitBoard attackers = new BitBoard(0);
            var positions = GetPositionInternal(Piece.WhitePawn.AsByte());
            for (var p = 0; p < positions.Length; p++)
            {
                var pattern = _moveProvider.GetWhitePawnAttackPattern(positions[p]);
                if (pattern.IsSet(_to))
                {
                    attackers = attackers.Add(positions[p]);
                }
            }

            positions = GetPositionInternal(Piece.WhiteKnight.AsByte());
            for (var p = 0; p < positions.Length; p++)
            {
                var pattern = _moveProvider.GetWhiteKnightAttackPattern(positions[p]);
                if (pattern.IsSet(_to))
                {
                    attackers = attackers.Add(positions[p]);
                }
            }

            positions = GetPositionInternal(Piece.WhiteBishop.AsByte());
            for (var p = 0; p < positions.Length; p++)
            {
                var pattern = positions[p].GetWhiteBishopAttackPattern(_occupied);
                if (pattern.IsSet(_to))
                {
                    attackers = attackers.Add(positions[p]);
                }
            }

            positions = GetPositionInternal(Piece.WhiteRook.AsByte());
            for (var p = 0; p < positions.Length; p++)
            {
                var pattern = positions[p].GetWhiteRookAttackPattern(_occupied);
                if (pattern.IsSet(_to))
                {
                    attackers = attackers.Add(positions[p]);
                }
            }

            positions = GetPositionInternal(Piece.WhiteQueen.AsByte());
            for (var p = 0; p < positions.Length; p++)
            {
                var pattern = positions[p].GetWhiteQueenAttackPattern(_occupied);
                if (pattern.IsSet(_to))
                {
                    attackers = attackers.Add(positions[p]);
                }
            }

            positions = GetPositionInternal(Piece.WhiteKing.AsByte());
            for (var p = 0; p < positions.Length; p++)
            {
                var pattern = _moveProvider.GetWhiteKingAttackPattern(positions[p]);
                if (pattern.IsSet(_to))
                {
                    attackers = attackers.Add(positions[p]);
                }
            }

            return attackers;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte[] GetPositionInternal(byte piece)
        {
            var b = _boards[piece];
            byte[] positions = new byte[_pieceCount[piece]];
            for (var i = 0; i < positions.Length; i++)
            {
                var position = b.BitScanForward();
                positions[i] = position;
                b = b.Remove(position);
            }

            return positions;
        }
    }
}
