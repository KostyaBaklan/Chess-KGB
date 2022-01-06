using System.Collections.Generic;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Sorting.Sorters;

namespace Engine.Interfaces
{
    public interface IPosition
    {
        ulong GetKey();
        int GetValue();
        Turn GetTurn();
        bool GetPiece(Square cell, out Piece? piece);

        void Make(IMove move);
        bool IsCheck();
        void UnMake();

        IEnumerable<IAttack> GetAllAttacks(Square cell, Piece piece);
        IEnumerable<IMove> GetAllMoves(Square cell, Piece piece);
        IMoveCollection GetAllAttacks(IMoveSorter sorter);
        IMoveCollection GetAllMoves(IMoveSorter sorter, IMove pvMove = null, IMove cutMove = null);
        int GetPieceValue(Square square);
        IBoard GetBoard();
        IEnumerable<IMove> GetHistory();
    }
}
