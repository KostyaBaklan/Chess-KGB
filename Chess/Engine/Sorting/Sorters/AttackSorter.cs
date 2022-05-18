using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.DataStructures.Moves;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;

namespace Engine.Sorting.Sorters
{
    public class AttackSorter:IMoveSorter
    {
        private readonly AttackSeeComparer _seeComparer;
        private readonly IBoard _board;

        public AttackSorter(IBoard board)
        {
            _board = board;
            _seeComparer = new AttackSeeComparer();
        }

        #region Implementation of IMoveSorter

        public MoveBase[] Order(AttackList attacks, MoveList moves, MoveBase pvNode)
        {
            throw new NotSupportedException();
        }

        public MoveBase[] Order(AttackList attacks)
        {
            if (attacks.Count == 0) return new MoveBase[0];
            if (attacks.Count == 1) return new MoveBase[] { attacks[0] };

            for (var i = 0; i < attacks.Count; i++)
            {
                var attack = attacks[i];
                attack.Captured = _board.GetPiece(attack.To);
                attack.See = _board.StaticExchange(attack);
            }

            attacks.FullSort(_seeComparer);

            MoveBase[] moves = new MoveBase[attacks.Count];
            for (var i = 0; i < moves.Length; i++)
            {
                moves[i] = attacks[i];
            }
            return moves;
        }

        public void Add(MoveBase move)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
