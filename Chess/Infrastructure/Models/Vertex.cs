using System.Collections.Generic;
using Infrastructure.Interfaces.Moves;
using Infrastructure.Models.Enums;

namespace Infrastructure.Models
{
    class Vertex
    {
        public IMove Move { get; }

        public Dictionary<IMove, Vertex> Children { get; }

        public Vertex(string move, int d, IMoveFormatter formatter)
        {
            Move = formatter.Parse(move, d % 2 == 0 ? Turn.White : Turn.Black);
            Children = new Dictionary<IMove, Vertex>();
        }

        #region Overrides of Object

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Vertex)obj);
        }

        #region Equality members

        protected bool Equals(Vertex other)
        {
            return Move == other.Move;
        }

        public override int GetHashCode()
        {
            return Move.GetHashCode();
        }

        public static bool operator ==(Vertex left, Vertex right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Vertex left, Vertex right)
        {
            return !Equals(left, right);
        }

        #endregion

        #endregion
    }
}