using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Helpers;

namespace Engine.Models.Moves
{
    public abstract class MoveBase:IMove
    {
        private bool _isCheck;

        protected MoveBase()
        {
            _isCheck = false;
            EmptyBoard = new BitBoard(0ul);
        }

        #region Implementation of IMove

        public short Key { get; set; }
        public int Difference { get; set; }
        public int History { get; set; }
        public Piece Piece { get; set; }
        public Square From { get; set; }
        public Square To { get; set; }
        public MoveType Type { get; set; }
        public BitBoard EmptyBoard { get; protected set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsCheck()
        {
            return _isCheck;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetMoveResult(bool isCheck)
        {
            _isCheck = isCheck;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract bool IsLegal(IBoard board);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual bool IsLegalAttack(IBoard board)
        {
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual bool IsAttack()
        {
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual bool IsCastle()
        {
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual bool IsPromotion()
        {
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual bool IsReversable()
        {
            return Piece != Piece.WhitePawn && Piece != Piece.BlackPawn;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract void Make(IBoard board, ArrayStack<Piece?> figureHistory);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract void UnMake(IBoard board, ArrayStack<Piece?> figureHistory);

        public void Set(params int[] squares)
        {
            BitBoard v = new BitBoard();
            v = squares.Select(s => s.AsBitBoard())
                .Aggregate(v, (current, square) => current | square);

            EmptyBoard = EmptyBoard |= v;
        }

        #endregion

        #region Overrides of Object

        public override string ToString()
        {
            return $"[{From.AsString()} -> {To.AsString()}]";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            if (obj is IMove move)
            {
                return Key == move.Key;
            }

            return false;
        }

        #region Equality members

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(IMove other)
        {
            return Key == other.Key;
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(MoveBase left, MoveBase right)
        {
            return left.Key == right.Key;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(MoveBase left, MoveBase right)
        {
            return left.Key != right.Key;
        }

        #endregion

        #endregion
    }
}
