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
        }

        #region Overrides of MoveSorter

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ProcessMove(MoveBase move)
        {
            var phase = Position.GetPhase();
            switch (move.Piece)
            {
                case Piece.WhitePawn:
                    if (Board.IsWhitePass(move.To.AsByte()))
                    {
                        InitialMoveCollection.AddSuggested(move);
                        return;
                    }

                    break;
                case Piece.WhiteKnight:
                case Piece.WhiteBishop:
                case Piece.BlackKnight:
                case Piece.BlackBishop:
                    if (phase == Phase.Opening)
                    {
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
                    }

                    break;
                case Piece.WhiteRook:
                    if (move.From == Squares.A1 && MoveHistoryService.CanDoWhiteBigCastle() ||
                        move.From == Squares.H1 && MoveHistoryService.CanDoWhiteSmallCastle())
                    {
                        if (phase == Phase.Opening)
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
                case Piece.WhiteQueen:
                    if (phase == Phase.Opening && move.To == Squares.D1)
                    {
                        InitialMoveCollection.AddNonSuggested(move);
                        return;
                    }
                    break;
                case Piece.BlackQueen:
                    if (phase == Phase.Opening && move.To == Squares.D8)
                    {
                        InitialMoveCollection.AddNonSuggested(move);
                        return;
                    }

                    break;
                case Piece.WhiteKing:
                    if (!MoveHistoryService.IsLastMoveWasCheck())
                    {
                        bool canCastle = MoveHistoryService.CanDoWhiteCastle();
                        if (phase == Phase.Opening)
                        {
                            if (canCastle)
                            {
                                InitialMoveCollection.AddBad(move);
                            }
                            else
                            {
                                InitialMoveCollection.AddNonSuggested(move);
                            }

                            return;
                        }

                        if (phase == Phase.Middle && canCastle)
                        {
                            InitialMoveCollection.AddNonSuggested(move);
                            return;
                        }
                    }

                    break;
                case Piece.BlackPawn:
                    if (Board.IsBlackPass(move.To.AsByte()))
                    {
                        InitialMoveCollection.AddSuggested(move);
                        return;
                    }

                    break;
                case Piece.BlackRook:
                    if (move.From == Squares.A8 && MoveHistoryService.CanDoBlackBigCastle() ||
                        move.From == Squares.H8 && MoveHistoryService.CanDoBlackSmallCastle())
                    {
                        if (phase == Phase.Opening)
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
                case Piece.BlackKing:
                    if (!MoveHistoryService.IsLastMoveWasCheck())
                    {
                        bool canCastle = MoveHistoryService.CanDoBlackCastle();
                        if (phase == Phase.Opening)
                        {
                            if (canCastle)
                            {
                                InitialMoveCollection.AddBad(move);
                            }
                            else
                            {
                                InitialMoveCollection.AddNonSuggested(move);
                            }

                            return;
                        }

                        if (phase == Phase.Middle && canCastle)
                        {
                            InitialMoveCollection.AddNonSuggested(move);
                            return;
                        }
                    }

                    break;
            }

            Position.Make(move);
            try
            {
                if (move.Piece.IsWhite())
                {
                    if (MoveHistoryService.GetPly() > 30 && MoveHistoryService.IsThreefoldRepetition(Board.GetKey()))
                    {
                        if (Board.GetValue() > 0)
                        {
                            InitialMoveCollection.AddBad(move);
                            return;
                        }
                    }
                    if (WhiteUnderAttack(move) || IsBadWhiteSee(move))
                    {
                        return;
                    }

                    if (move.Piece == Piece.WhitePawn)
                    {
                        if ((MoveProvider.GetAttackPattern(Piece.WhitePawn.AsByte(), move.To.AsByte()) &
                            Board.GetBlackBits()).Any() || move.IsPassed)
                        {
                            InitialMoveCollection.AddSuggested(move);
                            return;
                        }
                    }

                    else if (move.Piece == Piece.WhiteKnight &&
                        (MoveProvider.GetAttackPattern(Piece.WhiteKnight.AsByte(), move.To.AsByte()) &
                         Board.GetBlackBits()).Any())
                    {
                        InitialMoveCollection.AddSuggested(move);
                        return;
                    }

                    else if (move.Piece == Piece.WhiteBishop &&
                        (move.To.AsByte().BishopAttacks(Board.GetOccupied()) &
                         Board.GetBlackBits()).Any())
                    {
                        InitialMoveCollection.AddSuggested(move);
                        return;
                    }
                }
                else
                {
                    if (MoveHistoryService.GetPly() > 30 && MoveHistoryService.IsThreefoldRepetition(Board.GetKey()))
                    {
                        if (Board.GetValue() < 0)
                        {
                            InitialMoveCollection.AddBad(move);
                            return;
                        }
                    }
                    if (BlackUnderAttack(move) || IsBadBlackSee(move))
                    {
                        return;
                    }

                    if (move.Piece == Piece.BlackPawn)
                    {
                        if ((MoveProvider.GetAttackPattern(Piece.BlackPawn.AsByte(), move.To.AsByte()) &
                             Board.GetWhiteBits()).Any() || move.IsPassed)
                        {
                            InitialMoveCollection.AddSuggested(move);
                            return;
                        }
                    }

                    else if (move.Piece == Piece.BlackKnight &&
                        (MoveProvider.GetAttackPattern(Piece.BlackKnight.AsByte(), move.To.AsByte()) &
                         Board.GetWhiteBits()).Any())
                    {
                        InitialMoveCollection.AddSuggested(move);
                        return;
                    }

                    else if (move.Piece == Piece.BlackBishop &&
                        (move.To.AsByte().BishopAttacks(Board.GetOccupied()) &
                         Board.GetWhiteBits()).Any())
                    {
                        InitialMoveCollection.AddSuggested(move);
                        return;
                    }
                }

                if (move.IsCheck)
                {
                    InitialMoveCollection.AddSuggested(move);
                }
                else
                {
                    InitialMoveCollection.AddNonCapture(move);
                }
            }
            finally
            {
                Position.UnMake();
            }
        }

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
                    if (move.To == Squares.D1)
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
                if (WhiteUnderAttack(move) || IsBadWhiteSee(move))
                {
                    return;
                }

                if (move.Piece == Piece.WhitePawn)
                {
                    if ((MoveProvider.GetAttackPattern(Piece.WhitePawn.AsByte(), move.To.AsByte()) &
                         Board.GetBlackBits()).Any())
                    {
                        InitialMoveCollection.AddSuggested(move);
                        return;
                    }
                }

                else if (move.Piece == Piece.WhiteKnight &&
                         (MoveProvider.GetAttackPattern(Piece.WhiteKnight.AsByte(), move.To.AsByte()) &
                          Board.GetBlackBits()).Any())
                {
                    InitialMoveCollection.AddSuggested(move);
                    return;
                }

                else if (move.Piece == Piece.WhiteBishop &&
                         (move.To.AsByte().BishopAttacks(Board.GetOccupied()) &
                          Board.GetBlackBits()).Any())
                {
                    InitialMoveCollection.AddSuggested(move);
                    return;
                }

                if (IsCheckToBlack(move))
                {
                    InitialMoveCollection.AddSuggested(move);
                }
                else
                {
                    InitialMoveCollection.AddNonCapture(move);
                }
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
                    if (move.To == Squares.D8)
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
                if (BlackUnderAttack(move) || IsBadBlackSee(move))
                {
                    return;
                }

                if (move.Piece == Piece.BlackPawn)
                {
                    if ((MoveProvider.GetAttackPattern(Piece.BlackPawn.AsByte(), move.To.AsByte()) &
                         Board.GetWhiteBits()).Any())
                    {
                        InitialMoveCollection.AddSuggested(move);
                        return;
                    }
                }

                else if (move.Piece == Piece.BlackKnight &&
                         (MoveProvider.GetAttackPattern(Piece.BlackKnight.AsByte(), move.To.AsByte()) &
                          Board.GetWhiteBits()).Any())
                {
                    InitialMoveCollection.AddSuggested(move);
                    return;
                }

                else if (move.Piece == Piece.BlackBishop &&
                         (move.To.AsByte().BishopAttacks(Board.GetOccupied()) &
                          Board.GetWhiteBits()).Any())
                {
                    InitialMoveCollection.AddSuggested(move);
                    return;
                }

                if (IsCheckToWhite(move))
                {
                    InitialMoveCollection.AddSuggested(move);
                }
                else
                {
                    InitialMoveCollection.AddNonCapture(move);
                }
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
                case Piece.WhitePawn:
                    if (Board.IsWhitePass(move.To.AsByte()))
                    {
                        InitialMoveCollection.AddSuggested(move);
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
                if (WhiteUnderAttack(move) || IsBadWhiteSee(move))
                {
                    return;
                }

                if (move.Piece == Piece.WhitePawn)
                {
                    if ((MoveProvider.GetAttackPattern(Piece.WhitePawn.AsByte(), move.To.AsByte()) &
                         Board.GetBlackBits()).Any() || move.IsPassed)
                    {
                        InitialMoveCollection.AddSuggested(move);
                        return;
                    }
                }

                else if (move.Piece == Piece.WhiteKnight &&
                         (MoveProvider.GetAttackPattern(Piece.WhiteKnight.AsByte(), move.To.AsByte()) &
                          Board.GetBlackBits()).Any())
                {
                    InitialMoveCollection.AddSuggested(move);
                    return;
                }

                else if (move.Piece == Piece.WhiteBishop &&
                         (move.To.AsByte().BishopAttacks(Board.GetOccupied()) &
                          Board.GetBlackBits()).Any())
                {
                    InitialMoveCollection.AddSuggested(move);
                    return;
                }

                if (IsCheckToBlack(move))
                {
                    InitialMoveCollection.AddSuggested(move);
                }
                else
                {
                    InitialMoveCollection.AddNonCapture(move);
                }
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
                case Piece.BlackPawn:
                    if (Board.IsBlackPass(move.To.AsByte()))
                    {
                        InitialMoveCollection.AddSuggested(move);
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
                if (BlackUnderAttack(move) || IsBadBlackSee(move))
                {
                    return;
                }

                if (move.Piece == Piece.BlackPawn)
                {
                    if ((MoveProvider.GetAttackPattern(Piece.BlackPawn.AsByte(), move.To.AsByte()) &
                         Board.GetWhiteBits()).Any() || move.IsPassed)
                    {
                        InitialMoveCollection.AddSuggested(move);
                        return;
                    }
                }

                else if (move.Piece == Piece.BlackKnight &&
                         (MoveProvider.GetAttackPattern(Piece.BlackKnight.AsByte(), move.To.AsByte()) &
                          Board.GetWhiteBits()).Any())
                {
                    InitialMoveCollection.AddSuggested(move);
                    return;
                }

                else if (move.Piece == Piece.BlackBishop &&
                         (move.To.AsByte().BishopAttacks(Board.GetOccupied()) &
                          Board.GetWhiteBits()).Any())
                {
                    InitialMoveCollection.AddSuggested(move);
                    return;
                }

                if (IsCheckToWhite(move))
                {
                    InitialMoveCollection.AddSuggested(move);
                }
                else
                {
                    InitialMoveCollection.AddNonCapture(move);
                }
            }
            finally
            {
                Position.UnMake();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ProcessWhiteEndMove(MoveBase move)
        {
            if (move.Piece == Piece.WhitePawn && Board.IsWhitePass(move.To.AsByte()))
            {
                InitialMoveCollection.AddSuggested(move);
                return;
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
                if (WhiteUnderAttack(move) || IsBadWhiteSee(move))
                {
                    return;
                }

                if (move.Piece == Piece.WhitePawn)
                {
                    if ((MoveProvider.GetAttackPattern(Piece.WhitePawn.AsByte(), move.To.AsByte()) &
                         Board.GetBlackBits()).Any() || move.IsPassed)
                    {
                        InitialMoveCollection.AddSuggested(move);
                        return;
                    }
                }

                else if (move.Piece == Piece.WhiteKnight &&
                         (MoveProvider.GetAttackPattern(Piece.WhiteKnight.AsByte(), move.To.AsByte()) &
                          Board.GetBlackBits()).Any())
                {
                    InitialMoveCollection.AddSuggested(move);
                    return;
                }

                else if (move.Piece == Piece.WhiteBishop &&
                         (move.To.AsByte().BishopAttacks(Board.GetOccupied()) &
                          Board.GetBlackBits()).Any())
                {
                    InitialMoveCollection.AddSuggested(move);
                    return;
                }

                if (IsCheckToBlack(move))
                {
                    InitialMoveCollection.AddSuggested(move);
                }
                else
                {
                    InitialMoveCollection.AddNonCapture(move);
                }
            }
            finally
            {
                Position.UnMake();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ProcessBlackEndMove(MoveBase move)
        {
            if (move.Piece == Piece.BlackPawn && Board.IsWhitePass(move.To.AsByte()))
            {
                InitialMoveCollection.AddSuggested(move);
                return;
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
                if (BlackUnderAttack(move) || IsBadBlackSee(move))
                {
                    return;
                }

                if (move.Piece == Piece.BlackPawn)
                {
                    if ((MoveProvider.GetAttackPattern(Piece.BlackPawn.AsByte(), move.To.AsByte()) &
                         Board.GetWhiteBits()).Any() || move.IsPassed)
                    {
                        InitialMoveCollection.AddSuggested(move);
                        return;
                    }
                }

                else if (move.Piece == Piece.BlackKnight &&
                         (MoveProvider.GetAttackPattern(Piece.BlackKnight.AsByte(), move.To.AsByte()) &
                          Board.GetWhiteBits()).Any())
                {
                    InitialMoveCollection.AddSuggested(move);
                    return;
                }

                else if (move.Piece == Piece.BlackBishop &&
                         (move.To.AsByte().BishopAttacks(Board.GetOccupied()) &
                          Board.GetWhiteBits()).Any())
                {
                    InitialMoveCollection.AddSuggested(move);
                    return;
                }

                if (IsCheckToWhite(move))
                {
                    InitialMoveCollection.AddSuggested(move);
                }
                else
                {
                    InitialMoveCollection.AddNonCapture(move);
                }
            }
            finally
            {
                Position.UnMake();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool BlackUnderAttack(MoveBase move)
        {
            var wp = Board.GetWhitePawnAttacks();
            if ((wp & Board.GetPieceBits(Piece.BlackRook)).Any())
            {
                InitialMoveCollection.AddNonSuggested(move);
                return true;
            }

            if ((wp & Board.GetPieceBits(Piece.BlackQueen)).Any())
            {
                InitialMoveCollection.AddBad(move);
                return true;
            }

            return BlackRookUnderAttack(move) || BlackQueenUnderAttack(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool WhiteUnderAttack(MoveBase move)
        {
            var bp = Board.GetBlackPawnAttacks();
            if ((bp & Board.GetPieceBits(Piece.WhiteRook)).Any())
            {
                InitialMoveCollection.AddNonSuggested(move);
                return true;
            }

            if ((bp & Board.GetPieceBits(Piece.WhiteQueen)).Any())
            {
                InitialMoveCollection.AddBad(move);
                return true;
            }

            return WhiteRookUnderAttack(move) || WhiteQueenUnderAttack(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool WhiteRookUnderAttack(MoveBase move)
        {
            var positionCollection = Board.GetPiecePositions(Piece.WhiteRook.AsByte());
            for (int i = 0; i < positionCollection.Count; i++)
            {
                byte rookPosition = positionCollection[i];
                if (MoveProvider.GetBlackKnightAttackTo(Board, rookPosition).Any() ||
                    MoveProvider.GetBlackBishopAttackTo(Board, rookPosition).Any())
                {
                    InitialMoveCollection.AddNonSuggested(move);
                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool BlackRookUnderAttack(MoveBase move)
        {
            var positionCollection = Board.GetPiecePositions(Piece.BlackRook.AsByte());
            for (int i = 0; i < positionCollection.Count; i++)
            {
                byte rookPosition = positionCollection[i];
                if (MoveProvider.GetWhiteKnightAttackTo(Board, rookPosition).Any() ||
                    MoveProvider.GetWhiteBishopAttackTo(Board, rookPosition).Any())
                {
                    InitialMoveCollection.AddNonSuggested(move);
                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool BlackQueenUnderAttack(MoveBase move)
        {
            var positionCollection = Board.GetPiecePositions(Piece.BlackQueen.AsByte());
            for (int i = 0; i < positionCollection.Count; i++)
            {
                byte queenPosition = positionCollection[i];
                if (MoveProvider.GetWhiteKnightAttackTo(Board, queenPosition).Any() ||
                    MoveProvider.GetWhiteBishopAttackTo(Board, queenPosition).Any() ||
                    MoveProvider.GetWhiteRookAttackTo(Board, queenPosition).Any())
                {
                    InitialMoveCollection.AddBad(move);
                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool WhiteQueenUnderAttack(MoveBase move)
        {
            var positionCollection = Board.GetPiecePositions(Piece.WhiteQueen.AsByte());
            for (int i = 0; i < positionCollection.Count; i++)
            {
                byte queenPosition = positionCollection[i];
                if (MoveProvider.GetBlackKnightAttackTo(Board, queenPosition).Any() ||
                    MoveProvider.GetBlackBishopAttackTo(Board, queenPosition).Any() ||
                    MoveProvider.GetBlackRookAttackTo(Board, queenPosition).Any())
                {
                    InitialMoveCollection.AddBad(move);
                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool IsCheckToWhite(MoveBase move)
        {
            var whiteKingPosition = Board.GetWhiteKingPosition();
            return MoveProvider.AnyLegalAttacksTo(move.Piece, move.To, whiteKingPosition);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool IsBadBlackSee(MoveBase move)
        {
            var attackTo = MoveProvider.GetWhitePawnAttackTo(Board, move.To.AsByte());
            if (attackTo.Any() && IsBadSee(move, attackTo, Piece.WhitePawn)) return true;

            attackTo = MoveProvider.GetWhiteKnightAttackTo(Board, move.To.AsByte());
            if (attackTo.Any() && IsBadSee(move, attackTo, Piece.WhiteKnight)) return true;

            attackTo = MoveProvider.GetWhiteBishopAttackTo(Board, move.To.AsByte());
            if (attackTo.Any() && IsBadSee(move, attackTo, Piece.WhiteBishop)) return true;

            attackTo = MoveProvider.GetWhiteRookAttackTo(Board, move.To.AsByte());
            if (attackTo.Any() && IsBadSee(move, attackTo, Piece.WhiteRook)) return true;

            attackTo = MoveProvider.GetWhiteQueenAttackTo(Board, move.To.AsByte());
            if (attackTo.Any() && IsBadSee(move, attackTo, Piece.WhiteQueen)) return true;

            attackTo = MoveProvider.GetWhiteKingAttackTo(Board, move.To.AsByte());
            if (attackTo.Any() && IsBadSee(move, attackTo, Piece.WhiteKing)) return true;

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool IsCheckToBlack(MoveBase move)
        {
            var blackKingPosition = Board.GetBlackKingPosition();
            return MoveProvider.AnyLegalAttacksTo(move.Piece, move.To, blackKingPosition);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool IsBadWhiteSee(MoveBase move)
        {
            var attackTo = MoveProvider.GetBlackPawnAttackTo(Board, move.To.AsByte());
            if (attackTo.Any() && IsBadSee(move, attackTo, Piece.BlackPawn)) return true;

            attackTo = MoveProvider.GetBlackKnightAttackTo(Board, move.To.AsByte());
            if (attackTo.Any() && IsBadSee(move, attackTo, Piece.BlackKnight)) return true;

            attackTo = MoveProvider.GetBlackBishopAttackTo(Board, move.To.AsByte());
            if (attackTo.Any() && IsBadSee(move, attackTo, Piece.BlackBishop)) return true;

            attackTo = MoveProvider.GetBlackRookAttackTo(Board, move.To.AsByte());
            if (attackTo.Any() && IsBadSee(move, attackTo, Piece.BlackRook)) return true;

            attackTo = MoveProvider.GetBlackQueenAttackTo(Board, move.To.AsByte());
            if (attackTo.Any() && IsBadSee(move, attackTo, Piece.BlackQueen)) return true;

            attackTo = MoveProvider.GetBlackKingAttackTo(Board, move.To.AsByte());
            if (attackTo.Any() && IsBadSee(move, attackTo, Piece.BlackKing)) return true;

            return false;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool IsBadSee(MoveBase move, BitBoard attackTo,
            Piece piece)
        {
            attackTo.GetPositions(PositionsList);
            for (var i = 0; i < PositionsList.Count; i++)
            {
                MoveProvider.GetAttacks(piece, PositionsList[i], AttackList);
                for (var x = 0; x < AttackList.Count; x++)
                {
                    AttackList[x].Captured = Board.GetPiece(AttackList[x].To);
                    if (Board.StaticExchange(AttackList[x]) <= 0) continue;

                    if (piece == Piece.BlackQueen || piece == Piece.WhiteQueen)
                    {
                        InitialMoveCollection.AddBad(move);
                    }
                    else
                    {
                        InitialMoveCollection.AddNonSuggested(move);
                    }

                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}