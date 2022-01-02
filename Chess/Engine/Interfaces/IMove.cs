using System;
using Engine.DataStructures;
using Engine.Models.Boards;
using Engine.Models.Enums;

namespace Engine.Interfaces
{
    public interface IMove:IComparable<IMove>
    {
        short Static { get; }
        short Difference { get; }
        int Value { get; set; }
        Piece Piece { get; }
        Square From { get; }
        Square To { get; }
        MoveType Type { get; }
        bool IsCheck();
        void SetMoveResult(bool isCheck);
        bool IsLegal(IBoard board);
        bool IsLegalAttack(IBoard board);
        bool IsAttack();
        bool IsCastle();
        bool IsPromotion();
        void Make(IBoard board, ArrayStack<Piece?> figureHistory);
        void UnMake(IBoard board, ArrayStack<Piece?> figureHistory);
    }
}
