﻿using System.Runtime.CompilerServices;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Enums;

namespace Engine.Models.Moves
{
    public class Attack : MoveBase
    {
        #region Overrides of MoveBase

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool IsAttack()
        {
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Make(IBoard board, ArrayStack<Piece?> figureHistory)
        {
            Piece piece = board.GetPiece(To);
            board.Remove(piece, To);
            figureHistory.Push(piece);
            board.Move(Piece, From,To);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void UnMake(IBoard board, ArrayStack<Piece?> figureHistory)
        {
            board.Move(Piece, To, From);
            Piece piece = figureHistory.Pop().Value;
            board.Add(piece, To);
        }

        #endregion

        #region Overrides of MoveBase

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool IsLegal(IBoard board)
        {
            return board.IsEmpty(EmptyBoard) && board.IsOpposite(To.AsBitBoard(), Piece);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool IsLegalAttack(IBoard board)
        {
            return board.IsEmpty(EmptyBoard);
        }

        #endregion
    }
}