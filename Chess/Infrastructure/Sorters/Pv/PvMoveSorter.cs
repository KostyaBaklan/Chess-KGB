using System.Collections.Generic;
using CommonServiceLocator;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.Moves;
using Infrastructure.Models.Enums;
using Infrastructure.Sorters.Comparers;

namespace Infrastructure.Sorters.Pv
{
    public abstract class PvMoveSorter : IMoveSorter
    {
        private readonly IKillerMoveCollection[] _moves;
        private readonly IMoveHistoryService _moveHistoryService;
        protected IMoveComparer Comparer;

        protected PvMoveSorter()
        {
            _moves = new IKillerMoveCollection[256];
            for (var i = 0; i < _moves.Length; i++)
            {
                _moves[i] = ServiceLocator.Current.GetInstance<IKillerMoveCollection>();
            }

            _moveHistoryService = ServiceLocator.Current.GetInstance<IMoveHistoryService>();
        }

        #region Implementation of IMoveSorter

        public IEnumerable<IMove> Order(IEnumerable<IMove> moves, IMove pvNode, IMove cutMove)
        {
            int depth = _moveHistoryService.Ply;
            if (depth < 0) return moves;

            if (pvNode != null)
            {
                return OrderInternal(moves, _moves[depth], pvNode, cutMove);
            }

            if (cutMove != null)
                return OrderInternal(moves, _moves[depth], cutMove);
            return OrderInternal(moves, _moves[depth]);
        }

        protected virtual IEnumerable<IMove> OrderInternal(IEnumerable<IMove> moves, IKillerMoveCollection collection, IMove cutMove)
        {
            var otherMoves = new List<IMove>(64);
            var killer = collection.GetMoves(cutMove);

            foreach (var move in moves)
            {
                if (killer.Contains(move))
                {
                    yield return move;
                }
                else if (move.Type == MoveType.Move || move.Type == MoveType.PawnOverMove)
                {
                    otherMoves.Add(move);
                }
                else
                {
                    yield return move;
                }
            }

            otherMoves.Sort(Comparer);

            foreach (var move in otherMoves)
            {
                yield return move;
            }
        }

        protected virtual IEnumerable<IMove> OrderInternal(IEnumerable<IMove> moves, IKillerMoveCollection collection)
        {
            var otherMoves = new List<IMove>(64);
            var killer = collection.GetMoves();

            foreach (var move in moves)
            {
                if (killer.Contains(move))
                {
                    yield return move;
                }
                else if (move.Type == MoveType.Move || move.Type == MoveType.PawnOverMove)
                {
                    otherMoves.Add(move);
                }
                else
                {
                    yield return move;
                }
            }

            otherMoves.Sort(Comparer);

            foreach (var move in otherMoves)
            {
                yield return move;
            }
        }

        protected virtual IEnumerable<IMove> OrderInternal(IEnumerable<IMove> moves, IKillerMoveCollection collection,
            IMove pvNode, IMove cutMove)
        {
            var otherMoves = new List<IMove>(64);
            var killerMoves = new Queue<IMove>(4);
            var attacks = new Queue<IMove>(12);
            var killer = collection.GetMoves(cutMove);

            foreach (var move in moves)
            {
                if (pvNode.Equals(move))
                {
                    yield return move;
                }
                if (killer.Contains(move))
                {
                    killerMoves.Enqueue(move);
                }
                else if (move.Type == MoveType.Move || move.Type == MoveType.PawnOverMove)
                {
                    otherMoves.Add(move);
                }
                else
                {
                    attacks.Enqueue(move);
                }
            }

            foreach (var move in killerMoves)
            {
                yield return move;
            }

            foreach (var move in attacks)
            {
                yield return move;
            }

            otherMoves.Sort(Comparer);

            foreach (var move in otherMoves)
            {
                yield return move;
            }
        }

        #endregion

        public void Add(IMove move)
        {
            int depth = _moveHistoryService.Ply;
            _moves[depth].Add(move);
        }
    }
}