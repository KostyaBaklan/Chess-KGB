using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Infrastructure.Interfaces.Moves;
using Infrastructure.Interfaces.Position;
using Infrastructure.Models.Enums;

namespace Infrastructure.Moves
{
    public abstract class MoveBase : IMove
    {
        protected MoveBase(Coordinate from, Coordinate to, MoveOperation operation, FigureKind figure)
        {
            From = from;
            To = to;
            Operation = operation;
            Figure = figure;
        }

        #region Implementation of IMove

        public Coordinate From { get; }
        public Coordinate To { get; }
        public MoveOperation Operation { get; protected set; }
        public MoveResult Result { get; set; }
        public abstract MoveType Type { get; }
        public FigureKind Figure { get; }
        public int Value { get; set; }
        public int StaticValue { get; set; }
        public List<int> EmptyCells { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract bool IsLegal(IBoard board);

        #region Overrides of Object

        public override string ToString()
        {
            return $"{From}->{To}";
        }

        #endregion

        #endregion

        #region Relational members

        public int CompareTo(IMove other)
        {
            return Value.CompareTo(other.Value);
        }

        public static bool operator <(MoveBase left, MoveBase right)
        {
            return left.Value<right.Value;
        }

        public static bool operator >(MoveBase left, MoveBase right)
        {
            return left.Value > right.Value;
        }

        public static bool operator <=(MoveBase left, MoveBase right)
        {
            return left.Value <= right.Value;
        }

        public static bool operator >=(MoveBase left, MoveBase right)
        {
            return left.Value >= right.Value;
        }

        #endregion
    }
}