using System.Collections.Generic;
using Engine.Models.Enums;

namespace Engine.Models.Moves
{
    public class MaterialBalance
    {
        public MaterialBalance()
        {
            White = new Queue<Piece>(8);
            Black = new Queue<Piece>(8);
        }

        public Queue<Piece> White { get; }
        public Queue<Piece> Black { get; }
    }
}
