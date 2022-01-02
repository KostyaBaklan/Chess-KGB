using System.Collections.Generic;
using System.Linq;
using Infrastructure.Interfaces.Moves;
using Infrastructure.Models.Enums;

namespace Infrastructure.Sorters
{
   public class ValueMoveSorter : IMoveSorter
    {
        #region Implementation of IMoveSorter

        public IEnumerable<IMove> Order(IEnumerable<IMove> moves, IMove pvNode, IMove cutMove)
        {
            List<IMove> m = new List<IMove>(32);
            foreach (var move in moves)
            {
                if (move.Type == MoveType.Attack || move.Type == MoveType.PawnOverAttack)
                {
                    yield return move;
                }
                else
                {
                    m.Add(move);
                }
            }

            foreach (var move in m.OrderByDescending(x => x.Value))
                yield return move;
        }

        #endregion
    }
}
