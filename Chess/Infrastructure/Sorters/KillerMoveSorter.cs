using System.Collections.Generic;
using CommonServiceLocator;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.Moves;
using Infrastructure.Models.Enums;

namespace Infrastructure.Sorters
{
    public class KillerMoveSorter : IMoveSorter
    {
        private readonly IKillerMoveCollection[] _moves;
        private readonly IMoveHistoryService _moveHistoryService;

        public KillerMoveSorter()
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
            if (depth <= 0) return moves;

            return OrderInternal(moves, _moves[depth]);
        }

        private static IEnumerable<IMove> OrderInternal(IEnumerable<IMove> moves, IKillerMoveCollection collection)
        {
            var otherMoves = new Queue<IMove>();

            foreach (var move in moves)
            {
                if (collection.Contains(move))
                {
                    yield return move;
                }
                else if(move.Type == MoveType.Move || move.Type == MoveType.PawnOverMove)
                {
                    otherMoves.Enqueue(move);
                }
                else
                {
                    yield return move;
                }
            }

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