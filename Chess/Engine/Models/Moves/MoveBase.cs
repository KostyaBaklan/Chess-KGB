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

        public short Static { get; set; }
        public short Difference { get; set; }
        public int Value { get; set; }
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
        public abstract void Make(IBoard board, ArrayStack<Piece?> figureHistory);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract void UnMake(IBoard board, ArrayStack<Piece?> figureHistory);

        public void Set(params int[] squares)
        {
            BitBoard v = new BitBoard();
            foreach (var square in squares.Select(s=>s.AsBitBoard()))
            {
                v |= square;
            }

            EmptyBoard = EmptyBoard |= v;
        }

        #endregion

        #region Overrides of Object

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(IMove other)
        {
            return Value.CompareTo(other.Value);
        }

        public override string ToString()
        {
            return $"[{From.AsString()} -> {To.AsString()}]";
        }

        #endregion
    }
}
