using System.Collections.Generic;
using Infrastructure.Models.Enums;
using Infrastructure.Moves;

namespace Infrastructure.Interfaces.Position
{
    public interface IBoard
    {
        FigureKind? this[byte k] { get; set; }
        FigureKind? this[Coordinate cell] { get; set; }
        Coordinate WhiteKingPosition { get;  }
        Coordinate BlackKingPosition { get;  }
        void SmallWhiteCastle(bool isDo);
        void SmallBlackCastle(bool isDo);
        void BigWhiteCastle(bool isDo);
        void BigBlackCastle(bool isDo);
        bool IsEmpty(int coordinate);
        bool IsEmpty(List<int> coordinate);
        bool CanDoSmallCastle(FigureKind figure);
        bool CanDoBigCastle(FigureKind figure);
        bool CanAttackOver(byte x, byte y, FigureKind figure);
        void SetOver(byte x, byte y, bool isDo);
        bool IsOnCell(byte cell, FigureKind figure);
        int GetValue();
        byte GetValue(byte k);
        IEnumerable<Coordinate> GetWhiteCells();
        IEnumerable<Coordinate> GetBlackCells();
        bool IsOpposite(int cell, bool isWhite);
        IEnumerable<int> GetPositions(int figure);
    }
}