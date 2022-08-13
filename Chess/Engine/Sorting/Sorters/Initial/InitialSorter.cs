using System.Runtime.CompilerServices;
using CommonServiceLocator;
using Engine.DataStructures;
using Engine.DataStructures.Moves;
using Engine.DataStructures.Moves.Collections.Initial;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;

namespace Engine.Sorting.Sorters.Initial
{
    public abstract class InitialSorter : MoveSorter
    {
        private readonly BitBoard _minorStartRanks;
        private readonly BitBoard _minorStartPositions;
        protected readonly PositionsList PositionsList;
        protected readonly AttackList AttackList;
        protected InitialMoveCollection InitialMoveCollection;

        protected readonly IMoveProvider MoveProvider = ServiceLocator.Current.GetInstance<IMoveProvider>();

        protected InitialSorter(IPosition position, IMoveComparer comparer) : base(position, comparer)
        {
            PositionsList = new PositionsList();
            AttackList = new AttackList();
            Comparer = comparer;
            _minorStartPositions = Squares.B1.AsBitBoard() | Squares.C1.AsBitBoard() | Squares.F1.AsBitBoard() |
                                   Squares.G1.AsBitBoard() | Squares.B8.AsBitBoard() | Squares.C8.AsBitBoard() |
                                   Squares.F8.AsBitBoard() | Squares.G8.AsBitBoard();
           _minorStartRanks = Board.GetRank(0)|Board.GetRank(7);
        }

        #region Overrides of MoveSorter

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ProcessWhiteOpeningMove(MoveBase move)
        {
            switch (move.Piece)
            {
                case Piece.WhiteKnight:
                case Piece.WhiteBishop:
                    if ((move.To.AsBitBoard() & Board.GetPerimeter()).Any())
                    {
                        InitialMoveCollection.AddNonSuggested(move);
                        return;
                    }

                    if ((_minorStartPositions & move.From.AsBitBoard()).IsZero())
                    {
                        InitialMoveCollection.AddNonSuggested(move);
                        return;
                    }

                    break;
                case Piece.WhiteRook:
                    if (move.From == Squares.A1 && MoveHistoryService.CanDoWhiteBigCastle() ||
                        move.From == Squares.H1 && MoveHistoryService.CanDoWhiteSmallCastle())
                    {
                        InitialMoveCollection.AddBad(move);
                        return;
                    }

                    break;
                case Piece.WhiteQueen:
                    if(MoveHistoryService.GetPly() < 7 || move.To == Squares.D1)
                    {
                        InitialMoveCollection.AddNonSuggested(move);
                        return;
                    }
                    break;
                case Piece.WhiteKing:
                    if (!MoveHistoryService.IsLastMoveWasCheck())
                    {
                        if (MoveHistoryService.CanDoWhiteCastle())
                        {
                            InitialMoveCollection.AddBad(move);
                        }
                        else
                        {
                            InitialMoveCollection.AddNonSuggested(move);
                        }

                        return;
                    }

                    break;
            }

            Position.Make(move);
            try
            {
                if (IsBadAttackToWhite(move))
                {
                    return;
                }
                if (move.IsCheck)
                {
                    InitialMoveCollection.AddSuggested(move);
                    return;
                }

                if (IsGoodAttackForWhite(move))
                {
                    return;
                }

                InitialMoveCollection.AddNonCapture(move);
            }
            finally
            {
                Position.UnMake();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ProcessBlackOpeningMove(MoveBase move)
        {
            switch (move.Piece)
            {
                case Piece.BlackKnight:
                case Piece.BlackBishop:
                    if ((move.To.AsBitBoard() & Board.GetPerimeter()).Any())
                    {
                        InitialMoveCollection.AddNonSuggested(move);
                        return;
                    }

                    if ((_minorStartPositions & move.From.AsBitBoard()).IsZero())
                    {
                        InitialMoveCollection.AddNonSuggested(move);
                        return;
                    }

                    break;
                case Piece.BlackQueen:
                    if (MoveHistoryService.GetPly() < 8 || move.To == Squares.D8)
                    {
                        InitialMoveCollection.AddNonSuggested(move);
                        return;
                    }
                    break;
                case Piece.BlackRook:
                    if (move.From == Squares.A8 && MoveHistoryService.CanDoBlackBigCastle() ||
                        move.From == Squares.H8 && MoveHistoryService.CanDoBlackSmallCastle())
                    {
                        InitialMoveCollection.AddBad(move);
                        return;
                    }

                    break;
                case Piece.BlackKing:
                    if (!MoveHistoryService.IsLastMoveWasCheck())
                    {
                        if (MoveHistoryService.CanDoBlackCastle())
                        {
                            InitialMoveCollection.AddBad(move);
                        }
                        else
                        {
                            InitialMoveCollection.AddNonSuggested(move);
                        }

                        return;
                    }

                    break;
            }

            Position.Make(move);
            try
            {
                if (IsBadAttackToBlack(move))
                {
                    return;
                }
                if (move.IsCheck)
                {
                    InitialMoveCollection.AddSuggested(move);
                    return;
                }

                if (IsGoodAttackForBlack(move))
                {
                    return;
                }

                InitialMoveCollection.AddNonCapture(move);
            }
            finally
            {
                Position.UnMake();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ProcessWhiteMiddleMove(MoveBase move)
        {
            switch (move.Piece)
            {
                case Piece.WhiteKnight:
                case Piece.WhiteBishop:
                    if ((move.To.AsBitBoard() & _minorStartRanks).Any())
                    {
                        InitialMoveCollection.AddNonSuggested(move);
                        return;
                    }

                    break;
                case Piece.WhiteRook:
                    if (move.From == Squares.A1 && MoveHistoryService.CanDoWhiteBigCastle() ||
                        move.From == Squares.H1 && MoveHistoryService.CanDoWhiteSmallCastle())
                    {
                        InitialMoveCollection.AddNonSuggested(move);
                        return;
                    }

                    break;
                case Piece.WhiteKing:
                    if (!MoveHistoryService.IsLastMoveWasCheck() && MoveHistoryService.CanDoWhiteCastle())
                    {
                        InitialMoveCollection.AddNonSuggested(move);
                        return;
                    }

                    break;
            }

            Position.Make(move);
            try
            {
                if (MoveHistoryService.GetPly() > 30 && MoveHistoryService.IsThreefoldRepetition(Board.GetKey()))
                {
                    if (Board.GetValue() > 0)
                    {
                        InitialMoveCollection.AddBad(move);
                        return;
                    }
                }
                if (IsBadAttackToWhite(move))
                {
                    return;
                }
                if (move.IsCheck)
                {
                    InitialMoveCollection.AddSuggested(move);
                    return;
                }

                if (IsGoodAttackForWhite(move))
                {
                    return;
                }
                if (move.Piece == Piece.WhitePawn && Board.IsWhitePass(move.To.AsByte()))
                {
                    InitialMoveCollection.AddSuggested(move);
                    return;
                }

                InitialMoveCollection.AddNonCapture(move);
            }
            finally
            {
                Position.UnMake();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ProcessBlackMiddleMove(MoveBase move)
        {
            switch (move.Piece)
            {
                case Piece.BlackKnight:
                case Piece.BlackBishop:
                    if ((move.To.AsBitBoard() & _minorStartRanks).Any())
                    {
                        InitialMoveCollection.AddNonSuggested(move);
                        return;
                    }

                    break;
                case Piece.BlackRook:
                    if (move.From == Squares.A8 && MoveHistoryService.CanDoBlackBigCastle() ||
                        move.From == Squares.H8 && MoveHistoryService.CanDoBlackSmallCastle())
                    {
                        InitialMoveCollection.AddNonSuggested(move); return;
                    }

                    break;
                case Piece.BlackKing:
                    if (!MoveHistoryService.IsLastMoveWasCheck() && MoveHistoryService.CanDoBlackCastle())
                    {
                        InitialMoveCollection.AddNonSuggested(move);
                        return;
                    }

                    break;
            }

            Position.Make(move);
            try
            {
                if (MoveHistoryService.GetPly() > 30 && MoveHistoryService.IsThreefoldRepetition(Board.GetKey()))
                {
                    if (Board.GetValue() < 0)
                    {
                        InitialMoveCollection.AddBad(move);
                        return;
                    }
                }

                if (IsBadAttackToBlack(move))
                {
                    return;
                }

                if (move.IsCheck)
                {
                    InitialMoveCollection.AddSuggested(move);
                    return;
                }

                if (IsGoodAttackForBlack(move))
                {
                    return;
                }
                if (move.Piece == Piece.BlackPawn && Board.IsBlackPass(move.To.AsByte()))
                {
                    InitialMoveCollection.AddSuggested(move);
                    return;
                }

                InitialMoveCollection.AddNonCapture(move);
            }
            finally
            {
                Position.UnMake();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ProcessWhiteEndMove(MoveBase move)
        {
            Position.Make(move);
            try
            {
                if (MoveHistoryService.IsThreefoldRepetition(Board.GetKey()))
                {
                    if (Board.GetValue() > 0)
                    {
                        InitialMoveCollection.AddBad(move);
                        return;
                    }
                }

                if (IsBadAttackToWhite(move))
                {
                    return;
                }

                if (move.IsCheck|| move.Piece == Piece.WhitePawn && Board.IsWhitePass(move.To.AsByte()))
                {
                    InitialMoveCollection.AddSuggested(move);
                    return;
                }

                if (IsGoodAttackForWhite(move))
                {
                    return;
                }

                InitialMoveCollection.AddNonCapture(move);
            }
            finally
            {
                Position.UnMake();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ProcessBlackEndMove(MoveBase move)
        {
            Position.Make(move);
            try
            {
                if (MoveHistoryService.IsThreefoldRepetition(Board.GetKey()))
                {
                    if (Board.GetValue() < 0)
                    {
                        InitialMoveCollection.AddBad(move);
                        return;
                    }
                }

                if (IsBadAttackToBlack(move))
                {
                    return;
                }
                if (move.IsCheck|| move.Piece == Piece.BlackPawn && Board.IsBlackPass(move.To.AsByte()))
                {
                    InitialMoveCollection.AddSuggested(move);
                    return;
                }

                if (IsGoodAttackForBlack(move))
                {
                    return;
                }

                InitialMoveCollection.AddNonCapture(move);
            }
            finally
            {
                Position.UnMake();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsGoodAttackForBlack(MoveBase move)
        {
            AttackList attacks = Position.GetBlackAttacks();
            return attacks.Count > 0 && IsWinCapture(move, attacks);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsBadAttackToBlack(MoveBase move)
        {
            AttackList attacks = Position.GetWhiteAttacks();
            return attacks.Count > 0 && IsOpponentWinCapture(move, attacks);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsGoodAttackForWhite(MoveBase move)
        {
            AttackList attacks = Position.GetWhiteAttacks();
            return attacks.Count > 0 && IsWinCapture(move, attacks);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsBadAttackToWhite(MoveBase move)
        {
            AttackList attacks = Position.GetBlackAttacks();
            return attacks.Count > 0 && IsOpponentWinCapture(move, attacks);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsWinCapture(MoveBase move, AttackList attacks)
        {
            for (int i = 0; i < attacks.Count; i++)
            {
                var attack = attacks[i];
                attack.Captured = Board.GetPiece(attack.To);

                int attackValue = Board.StaticExchange(attack);
                if (attackValue > 0)
                {
                    InitialMoveCollection.AddSuggested(move);

                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsOpponentWinCapture(MoveBase move, AttackList attacks)
        {
            for (int i = 0; i < attacks.Count; i++)
            {
                var attack = attacks[i];
                attack.Captured = Board.GetPiece(attack.To);

                int attackValue = Board.StaticExchange(attack);
                if (attackValue > 0)
                {
                    InitialMoveCollection.AddNonSuggested(move);

                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void IsCheck(MoveBase move)
        {
            if (move.IsCheck)
            {
                InitialMoveCollection.AddSuggested(move);
            }
            else
            {
                InitialMoveCollection.AddNonCapture(move);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ProcessPromotion(MoveBase move)
        {
            Position.Make(move);
            try
            {
                if (move.Piece.IsWhite())
                {
                    MoveProvider.GetWhiteAttacksTo(move.To.AsByte(), AttackList);
                    StaticExchange(move, Piece.WhitePawn);
                }
                else
                {
                    MoveProvider.GetBlackAttacksTo(move.To.AsByte(), AttackList);
                    StaticExchange(move, Piece.BlackPawn);
                }
            }
            finally
            {
                Position.UnMake();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void StaticExchange(MoveBase move, Piece captured)
        {
            if (AttackList.Count == 0)
            {
                InitialMoveCollection.AddWinCapture(move);
            }
            else
            {
                int max = short.MinValue;
                for (int i = 0; i < AttackList.Count; i++)
                {
                    var attack = AttackList[i];
                    attack.Captured = captured;
                    var see = Board.StaticExchange(attack);
                    if (see > max)
                    {
                        max = see;
                    }
                }

                if (max < 0)
                {
                    InitialMoveCollection.AddWinCapture(move);
                }
                else if (max > 0)
                {
                    InitialMoveCollection.AddLooseCapture(move);
                }
                else
                {
                    InitialMoveCollection.AddTrade(move);
                }
            }
        }

        #endregion
    }
}