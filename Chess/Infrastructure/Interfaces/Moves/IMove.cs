using System;
using Infrastructure.Interfaces.Position;
using Infrastructure.Models.Enums;
using Infrastructure.Moves;

namespace Infrastructure.Interfaces.Moves
{
    public interface IMove:IComparable<IMove>
    {
        Coordinate From { get; }
        Coordinate To { get; }
        MoveOperation Operation { get; }
        MoveResult Result { get; set; }
        MoveType Type { get; }
        FigureKind Figure { get; }
        int Value { get; }
        int StaticValue { get; }
        bool IsLegal(IBoard board);
    }
}
