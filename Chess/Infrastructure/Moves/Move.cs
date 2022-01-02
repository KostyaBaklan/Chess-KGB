using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Infrastructure.Interfaces.Position;
using Infrastructure.Models.Enums;

namespace Infrastructure.Moves
{
    public class Move: MoveBase
    {
        public Move(Coordinate from, Coordinate to, MoveOperation operation, FigureKind figure) : base(from, to, operation, figure)
        {
            EmptyCells = new List<int>{to.Key};
        }

        public override MoveType Type => MoveType.Move;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool IsLegal(IBoard board)
        {
            return board.IsEmpty(EmptyCells);
        }
    }
}