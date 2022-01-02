using System.Collections.Generic;
using Infrastructure.Models.Enums;
using Infrastructure.Moves;
using Infrastructure.Positions;

namespace Infrastructure.Interfaces.Position
{
    public interface IFigureMap
    {
        ZobristHash Hash { get; }
        Coordinate WhiteKingPosition { get; }
        Coordinate BlackKingPosition { get; }
        int GetValue(IBoard board);
        void Set(byte coordinate, FigureKind figure);
        void Remove(byte coordinate, FigureKind figure);
        IEnumerable<Coordinate> GetWhiteCells();
        IEnumerable<Coordinate> GetBlackCells();
        void WhiteCastle(bool isDo);
        void BlackCastle(bool isDo);
        IEnumerable<int> GetPositions(int figure);
    }
}
