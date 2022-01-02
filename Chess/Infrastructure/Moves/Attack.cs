using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Infrastructure.Interfaces.Position;
using Infrastructure.Models.Enums;

namespace Infrastructure.Moves
{
    public class Attack:MoveBase
    {
        private readonly bool _isWhite;

        public Attack(Coordinate from, Coordinate to, MoveOperation operation, FigureKind figure) : base(from, to,
            operation, figure)
        {
            _isWhite = figure.IsWhite();
            EmptyCells = new List<int>();
        }
        public override MoveType Type => MoveType.Attack;

        #region Overrides of MoveBase

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool IsLegal(IBoard board)
        {
            return board.IsEmpty(EmptyCells) && board.IsOpposite(To.Key,_isWhite);
        }

        #endregion
    }
}