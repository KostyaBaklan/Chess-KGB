using System.Collections.Generic;
using Infrastructure.Interfaces.Position;
using Infrastructure.Models.Enums;
using Infrastructure.Moves;

namespace Infrastructure.Interfaces.Moves
{
    public interface IMoveProvider
    {
        IEnumerable<IMove> GetMoves(Coordinate from, FigureKind figure, IBoard board);

        IEnumerable<IMove> GetAttacks(Coordinate from, FigureKind figure, IBoard board);
        bool AnyWhiteCheck(IBoard board);
        bool AnyBlackCheck(IBoard board);
        IMove GetWhiteSmallCastle();
        IMove GetBlackSmallCastle();
        IMove GetWhiteBigCastle();
        IMove GetBlackBigCastle();
        IMove GetMove(FigureKind figure, Coordinate from, Coordinate to);
        IMove GetAttack(FigureKind figure, Coordinate from, Coordinate to);
        bool CanDoWhiteSmallCastle(IBoard board);
        bool CanDoWhiteBigCastle(IBoard board);
        bool CanDoBlackSmallCastle(IBoard board);
        bool CanDoBlackBigCastle(IBoard board);
    }
}