using System.Collections.Generic;
using Infrastructure.Interfaces.Moves;
using Infrastructure.Models.Enums;
using Infrastructure.Moves;

namespace Infrastructure.Interfaces.Position
{
    public interface IPosition
    {
        Turn Turn { get; }
        ulong Key { get; }
        IBoard Board { get; }
        FigureKind? GetFigure(Coordinate cell);

        int GetValue();

        void Make(IMove move);
        void UnMake();

        bool IsCheck();

        IEnumerable<IMove> GetAllMoves(Coordinate cell);
        IEnumerable<IMove> GetAllMoves(IMoveSorter sorter, IMove pvMove = null, IMove cutMove = null);
        IEnumerable<IMove> GetAllAttacks(Coordinate cell);
        IEnumerable<IMove> GetAllAttacks();
        string GetHistory();
        byte GetFigureValue(byte toKey);
        void MakeTest(IMove move);
        bool IsCheckTest();
    }
}
