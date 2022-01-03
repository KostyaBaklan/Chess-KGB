using System.Runtime.CompilerServices;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Enums;

namespace Engine.Models.Helpers
{
    public static class MoveExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsCheckToBlack(this IMoveProvider moveProvider,IBoard board)
        {
            var kingPosition = board.GetBlackKingPosition().AsBitBoard();
            return moveProvider.IsWhiteAttackTo(board, kingPosition);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsCheckToWhite(this IMoveProvider moveProvider, IBoard board)
        {
            var kingPosition = board.GetWhiteKingPosition().AsBitBoard();
            return moveProvider.IsBlackAttackTo(board, kingPosition);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBlackAttackTo(this IMoveProvider moveProvider, IBoard board, BitBoard to)
        {
            return moveProvider.IsBlackBishopAttackTo(board, to)
                   || moveProvider.IsBlackKnightAttackTo(board, to)
                   || moveProvider.IsBlackQueenAttackTo(board, to)
                   || moveProvider.IsBlackRookAttackTo(board, to)
                   || moveProvider.IsBlackPawnAttackTo(board, to)
                   || moveProvider.IsBlackKingAttackTo(board, to);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWhiteAttackTo(this IMoveProvider moveProvider, IBoard board, BitBoard to)
        {
            return moveProvider.IsWhiteBishopAttackTo(board, to)
                || moveProvider.IsWhiteKnightAttackTo(board, to)
                || moveProvider.IsWhiteQueenAttackTo(board, to)
                || moveProvider.IsWhiteRookAttackTo(board, to)
                || moveProvider.IsWhitePawnAttackTo(board, to)
                || moveProvider.IsWhiteKingAttackTo(board, to);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWhiteKnightAttackTo(this IMoveProvider moveProvider, IBoard board, BitBoard to)
        {
            var positions = board.GetPositions(Piece.WhiteKnight.AsByte());
            for (var i = 0; i < positions.Count; i++)
            {
                BitBoard pattern = GetWhiteKnightAttackPattern(moveProvider, positions[i]);
                if (pattern.IsSet(to))
                {
                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWhiteBishopAttackTo(this IMoveProvider moveProvider, IBoard board, BitBoard to)
        {
            var positions = board.GetPositions(Piece.WhiteBishop.AsByte());
            for (var i = 0; i < positions.Count; i++)
            {
                BitBoard pattern = GetWhiteBishopAttackPattern(positions[i], board.GetOccupied());
                if (pattern.IsSet(to))
                {
                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWhiteQueenAttackTo(this IMoveProvider moveProvider, IBoard board, BitBoard to)
        {
            var positions = board.GetPositions(Piece.WhiteQueen.AsByte());
            for (var i = 0; i < positions.Count; i++)
            {
                BitBoard pattern = GetWhiteQueenAttackPattern(positions[i], board.GetOccupied());
                if (pattern.IsSet(to))
                {
                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWhiteRookAttackTo(this IMoveProvider moveProvider, IBoard board, BitBoard to)
        {
            var positions = board.GetPositions(Piece.WhiteRook.AsByte());
            for (var i = 0; i < positions.Count; i++)
            {
                BitBoard pattern = GetWhiteRookAttackPattern(positions[i], board.GetOccupied());
                if (pattern.IsSet(to))
                {
                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWhitePawnAttackTo(this IMoveProvider moveProvider, IBoard board, BitBoard to)
        {
            var positions = board.GetPositions(Piece.WhitePawn.AsByte());
            for (var i = 0; i < positions.Count; i++)
            {
                BitBoard pattern = GetWhitePawnAttackPattern(moveProvider, positions[i]);
                if (pattern.IsSet(to))
                {
                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWhiteKingAttackTo(this IMoveProvider moveProvider, IBoard board, BitBoard to)
        {
            var positions = board.GetPositions(Piece.WhiteKing.AsByte());
            for (var i = 0; i < positions.Count; i++)
            {
                BitBoard pattern = GetWhiteKingAttackPattern(moveProvider, positions[i]);
                if (pattern.IsSet(to))
                {
                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBlackKnightAttackTo(this IMoveProvider moveProvider, IBoard board, BitBoard to)
        {
            var positions = board.GetPositions(Piece.BlackKnight.AsByte());
            for (var i = 0; i < positions.Count; i++)
            {
                BitBoard pattern = GetBlackKnightAttackPattern(moveProvider, positions[i]);
                if (pattern.IsSet(to))
                {
                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBlackBishopAttackTo(this IMoveProvider moveProvider, IBoard board, BitBoard to)
        {
            var positions = board.GetPositions(Piece.BlackBishop.AsByte());
            for (var i = 0; i < positions.Count; i++)
            {
                BitBoard pattern = GetBlackBishopAttackPattern(positions[i], board.GetOccupied());
                if (pattern.IsSet(to))
                {
                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBlackQueenAttackTo(this IMoveProvider moveProvider, IBoard board, BitBoard to)
        {
            var positions = board.GetPositions(Piece.BlackQueen.AsByte());
            for (var i = 0; i < positions.Count; i++)
            {
                BitBoard pattern = GetBlackQueenAttackPattern(positions[i], board.GetOccupied());
                if (pattern.IsSet(to))
                {
                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBlackRookAttackTo(this IMoveProvider moveProvider, IBoard board, BitBoard to)
        {
            var positions = board.GetPositions(Piece.BlackRook.AsByte());
            for (var i = 0; i < positions.Count; i++)
            {
                BitBoard pattern = GetBlackRookAttackPattern(positions[i], board.GetOccupied());
                if (pattern.IsSet(to))
                {
                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBlackPawnAttackTo(this IMoveProvider moveProvider, IBoard board, BitBoard to)
        {
            var positions = board.GetPositions(Piece.BlackPawn.AsByte());
            for (var i = 0; i < positions.Count; i++)
            {
                BitBoard pattern = GetBlackPawnAttackPattern(moveProvider, positions[i]);
                if (pattern.IsSet(to))
                {
                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBlackKingAttackTo(this IMoveProvider moveProvider, IBoard board, BitBoard to)
        {
            var positions = board.GetPositions(Piece.BlackKing.AsByte());
            for (var i = 0; i < positions.Count; i++)
            {
                BitBoard pattern = GetBlackKingAttackPattern(moveProvider, positions[i]);
                if (pattern.IsSet(to))
                {
                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BitBoard GetBlackPawnAttackPattern(IMoveProvider moveProvider, int position)
        {
            return moveProvider.GetAttackPattern(Piece.BlackPawn.AsByte(), position);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BitBoard GetBlackRookAttackPattern(int position, BitBoard occupied)
        {
            return position.RookAttacks(occupied);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BitBoard GetBlackQueenAttackPattern(int position, BitBoard occupied)
        {
            return position.QueenAttacks(occupied);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BitBoard GetBlackBishopAttackPattern(int position, BitBoard occupied)
        {
            return position.BishopAttacks(occupied);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BitBoard GetBlackKingAttackPattern(IMoveProvider moveProvider, int position)
        {
            return moveProvider.GetAttackPattern(Piece.BlackKing.AsByte(), position);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BitBoard GetBlackKnightAttackPattern(IMoveProvider moveProvider, int position)
        {
            return moveProvider.GetAttackPattern(Piece.BlackKnight.AsByte(), position);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BitBoard GetWhitePawnAttackPattern(IMoveProvider moveProvider, int position)
        {
            return moveProvider.GetAttackPattern(Piece.WhitePawn.AsByte(), position);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BitBoard GetWhiteRookAttackPattern(int position, BitBoard occupied)
        {
            return position.RookAttacks(occupied);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BitBoard GetWhiteQueenAttackPattern(int position, BitBoard occupied)
        {
            return position.QueenAttacks(occupied);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BitBoard GetWhiteBishopAttackPattern(int position, BitBoard occupied)
        {
            return position.BishopAttacks(occupied);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BitBoard GetWhiteKnightAttackPattern(IMoveProvider moveProvider, int position)
        {
            return moveProvider.GetAttackPattern(Piece.WhiteKnight.AsByte(), position);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BitBoard GetWhiteKingAttackPattern(IMoveProvider moveProvider, int position)
        {
            return moveProvider.GetAttackPattern(Piece.WhiteKing.AsByte(), position);
        }
    }
}
