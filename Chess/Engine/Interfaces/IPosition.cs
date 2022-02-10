using System.Collections.Generic;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Sorting.Sorters;

namespace Engine.Interfaces
{
    public interface IPosition
    {
        ulong GetKey();
        short GetValue();
        int GetStaticValue();
        int GetOpponentMaxValue();
        Turn GetTurn();
        bool GetPiece(Square cell, out Piece? piece);

        void Make(IMove move);
        void UnMake();
        void Do(IMove move);
        void UnDo(IMove move);
        void SwapTurn();

        IEnumerable<IAttack> GetAllAttacks(Square cell, Piece piece);
        IEnumerable<IMove> GetAllMoves(Square cell, Piece piece);
        IMove[] GetAllAttacks(IMoveSorter sorter);
        IMove[] GetAllMoves(IMoveSorter sorter, IMove pvMove = null);
        int GetPieceValue(Square square);
        IBoard GetBoard();
        IEnumerable<IMove> GetHistory();
        Phase GetPhase();
    }
}
