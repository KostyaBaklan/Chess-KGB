using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;

namespace OpeningTool
{
    public class MoveFilter
    {
        readonly HashSet<Square> _whiteSet = new HashSet<Square>
        {
            Squares.A2,Squares.B2,Squares.C2,Squares.D2,Squares.E2,Squares.F2,Squares.G2,Squares.H2
        };

        readonly HashSet<Square> _blackSet = new HashSet<Square>
        {
            Squares.A7,Squares.B7,Squares.C7,Squares.D7,Squares.E7,Squares.F7,Squares.G7,Squares.H7
        };

       // private Dictionary<Piece, List<MoveBase>> _pieces;
        private readonly IMoveProvider _moveProvider;

        public MoveFilter(IMoveProvider moveProvider)
        {
            _moveProvider = moveProvider;

            //_pieces = moveProvider.GetAll()
            //    .Where(m => !m.IsAttack)
            //    .GroupBy(m => m.Piece)
            //    .ToDictionary(k => k.Key, v => v.ToList());
        }

        public bool IsBad(string key)
        {
            var moveBase = _moveProvider.Get(short.Parse(key));
            if (moveBase.IsAttack) return false;

            switch (moveBase.Piece)
            {
                case Piece.WhitePawn:
                    return IsBadWhitePawn(moveBase);
                case Piece.WhiteKnight:
                    return IsBadWhiteKnight(moveBase);
                case Piece.WhiteBishop:
                    return IsBadWhiteBishop(moveBase);
                case Piece.BlackPawn:
                    return IsBadBlackPawn(moveBase);
                case Piece.BlackKnight:
                    return IsBadBlackKnight(moveBase);
                case Piece.BlackBishop:
                    return IsBadBlackBishop(moveBase);
                case Piece.WhiteRook:
                //case Piece.WhiteQueen:
                case Piece.BlackRook:
                //case Piece.BlackQueen:
                    return true;
                case Piece.WhiteKing:
                case Piece.BlackKing:
                    return !moveBase.IsCastle;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool IsBadBlackBishop(MoveBase move)
        {
            if (move.To == Squares.A3 || move.To == Squares.H3) return true;
            return move.From != Squares.C8 && move.From != Squares.F8;
        }

        private bool IsBadWhiteBishop(MoveBase move)
        {
            if (move.To == Squares.A6 || move.To == Squares.H6) return true;
            return move.From != Squares.C1 && move.From != Squares.F1;
        }

        private bool IsBadBlackKnight(MoveBase move)
        {
            if (move.To == Squares.A6 || move.To == Squares.H6) return true;
            return move.From != Squares.B8 && move.From != Squares.G8;
        }

        private bool IsBadWhiteKnight(MoveBase move)
        {
            if (move.To == Squares.A3 || move.To == Squares.H3) return true;
            return move.From != Squares.B1 && move.From != Squares.G1;
        }

        private bool IsBadBlackPawn(MoveBase move)
        {
            return !_blackSet.Contains(move.From);
        }

        private bool IsBadWhitePawn(MoveBase move)
        {
            return !_whiteSet.Contains(move.From);
        }
    }
}
